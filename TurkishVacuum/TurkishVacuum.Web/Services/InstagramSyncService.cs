using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace TurkishVacuum.Web.Services
{
    // Arka planda çalışır:
    //  * Instagram'daki son gönderileri çekip etkileşime göre en popülerlerini InstagramFeed'e yazar,
    //  * 60 günde süresi dolan erişim anahtarını haftada bir yeniler ve dosyaya kaydeder.
    // Böylece Instagram'a yeni paylaşım yapıldığında siteyi elle güncellemek gerekmez.
    public class InstagramSyncService : BackgroundService
    {
        public const string HttpClientName = "instagram";

        private static readonly TimeSpan TokenRefreshInterval = TimeSpan.FromDays(7);
        private static readonly TimeSpan RetryDelay = TimeSpan.FromMinutes(30);

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly InstagramFeed _feed;
        private readonly InstagramOptions _options;
        private readonly string _tokenStorePath;
        private readonly ILogger<InstagramSyncService> _logger;

        private StoredToken? _token;

        public InstagramSyncService(
            IHttpClientFactory httpClientFactory,
            InstagramFeed feed,
            IOptions<InstagramOptions> options,
            IHostEnvironment environment,
            ILogger<InstagramSyncService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _feed = feed;
            _options = options.Value;
            _tokenStorePath = Path.Combine(environment.ContentRootPath, _options.TokenStorePath);
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _token = LoadToken();
            if (_token is null)
            {
                _logger.LogInformation("Instagram erişim anahtarı tanımlı değil; sitede sabit gönderiler gösterilecek.");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                var delay = TimeSpan.FromMinutes(Math.Max(15, _options.RefreshMinutes));
                try
                {
                    await RefreshTokenIfDueAsync(stoppingToken);

                    var permalinks = await FetchPopularPermalinksAsync(stoppingToken);
                    _feed.Update(permalinks);
                    _logger.LogInformation("Instagram gönderileri güncellendi ({Count} gönderi).", permalinks.Count);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    // Hata siteyi durdurmasın: son başarılı liste (veya sabit gönderiler) gösterilmeye devam eder.
                    _logger.LogWarning("Instagram gönderileri alınamadı: {Message}", ex.Message);
                    delay = RetryDelay;
                }

                await Task.Delay(delay, stoppingToken);
            }
        }

        private async Task<IReadOnlyList<string>> FetchPopularPermalinksAsync(CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient(HttpClientName);
            var candidates = new List<MediaItem>();
            var pageSize = Math.Clamp(_options.CandidateCount, 1, 50);

            string? url = ApiPath($"me/media?fields=permalink,like_count,comments_count,timestamp&limit={pageSize}")
                + "&access_token=" + Uri.EscapeDataString(_token!.Token);

            while (url is not null && candidates.Count < _options.CandidateCount)
            {
                var page = await GetAsync<MediaPage>(client, url, cancellationToken);
                candidates.AddRange(page.Data.Where(media => !string.IsNullOrEmpty(media.Permalink)));
                url = page.Paging?.Next;
            }

            // Yorum, beğeniye göre daha güçlü bir etkileşim sinyali olduğu için iki kat sayılır.
            // Hesapta beğeni sayıları gizliyse like_count gelmez; sıralama yorum ve tarihe göre yapılır.
            return candidates
                .Take(_options.CandidateCount)
                .OrderByDescending(media => (media.LikeCount ?? 0) + 2 * (media.CommentsCount ?? 0))
                .ThenByDescending(media => media.Timestamp, StringComparer.Ordinal)
                .Take(Math.Max(1, _options.PostCount))
                .Select(media => media.Permalink!)
                .ToList();
        }

        private async Task RefreshTokenIfDueAsync(CancellationToken cancellationToken)
        {
            if (DateTimeOffset.UtcNow - _token!.RefreshedAt < TokenRefreshInterval) return;

            try
            {
                var client = _httpClientFactory.CreateClient(HttpClientName);
                var response = await GetAsync<TokenResponse>(client,
                    "refresh_access_token?grant_type=ig_refresh_token&access_token=" + Uri.EscapeDataString(_token.Token),
                    cancellationToken);

                if (string.IsNullOrEmpty(response.AccessToken)) return;

                _token = _token with { Token = response.AccessToken, RefreshedAt = DateTimeOffset.UtcNow };
                SaveToken(_token);
                _logger.LogInformation("Instagram erişim anahtarı yenilendi.");
            }
            catch (HttpRequestException ex)
            {
                // Mevcut anahtar hâlâ geçerliyse gönderi çekme işlemi devam edebilir.
                _logger.LogWarning("Instagram erişim anahtarı yenilenemedi: {Message}", ex.Message);
            }
        }

        private StoredToken? LoadToken()
        {
            var configuredToken = _options.AccessToken?.Trim();
            if (string.IsNullOrEmpty(configuredToken)) return null;

            var seed = Fingerprint(configuredToken);
            try
            {
                if (File.Exists(_tokenStorePath))
                {
                    var stored = JsonSerializer.Deserialize<StoredToken>(File.ReadAllText(_tokenStorePath));

                    // Dosyadaki anahtar, yapılandırmadaki anahtardan türetildiyse onu kullan.
                    // Yapılandırmaya yeni bir anahtar girildiyse dosya yok sayılır.
                    if (stored is not null && stored.Seed == seed && !string.IsNullOrEmpty(stored.Token))
                    {
                        return stored;
                    }
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                _logger.LogWarning("Instagram anahtar dosyası okunamadı: {Message}", ex.Message);
            }

            // Meta yalnızca en az 24 saatlik anahtarı yenilediği için yeni anahtarın ilk yenilemesi bir gün sonraya planlanır.
            var token = new StoredToken(configuredToken, DateTimeOffset.UtcNow - TokenRefreshInterval + TimeSpan.FromDays(1), seed);
            SaveToken(token);
            return token;
        }

        private void SaveToken(StoredToken token)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_tokenStorePath)!);
                var tempPath = _tokenStorePath + ".tmp";
                File.WriteAllText(tempPath, JsonSerializer.Serialize(token));
                File.Move(tempPath, _tokenStorePath, overwrite: true);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                _logger.LogWarning("Instagram erişim anahtarı kaydedilemedi ({Path}): {Message}", _tokenStorePath, ex.Message);
            }
        }

        private string ApiPath(string pathAndQuery) =>
            string.IsNullOrWhiteSpace(_options.ApiVersion) ? pathAndQuery : $"{_options.ApiVersion.Trim('/')}/{pathAndQuery}";

        private static async Task<T> GetAsync<T>(HttpClient client, string url, CancellationToken cancellationToken)
        {
            using var response = await client.GetAsync(url, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                // Adres erişim anahtarını içerdiği için hata mesajına yazılmaz.
                throw new HttpRequestException($"Instagram API {(int)response.StatusCode}: {ReadErrorMessage(body) ?? response.ReasonPhrase}");
            }

            return JsonSerializer.Deserialize<T>(body) ?? throw new JsonException("Instagram API boş yanıt döndü.");
        }

        private static string? ReadErrorMessage(string body)
        {
            try
            {
                using var document = JsonDocument.Parse(body);
                return document.RootElement.TryGetProperty("error", out var error) && error.TryGetProperty("message", out var message)
                    ? message.GetString()
                    : null;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static string Fingerprint(string value) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))[..16];

        private sealed record StoredToken(string Token, DateTimeOffset RefreshedAt, string Seed);

        private sealed class MediaPage
        {
            [JsonPropertyName("data")]
            public List<MediaItem> Data { get; set; } = new();

            [JsonPropertyName("paging")]
            public PagingInfo? Paging { get; set; }
        }

        private sealed class PagingInfo
        {
            [JsonPropertyName("next")]
            public string? Next { get; set; }
        }

        private sealed class MediaItem
        {
            [JsonPropertyName("permalink")]
            public string? Permalink { get; set; }

            [JsonPropertyName("like_count")]
            public int? LikeCount { get; set; }

            [JsonPropertyName("comments_count")]
            public int? CommentsCount { get; set; }

            // Instagram "2025-10-20T12:34:56+0000" biçiminde UTC döndürür; metin olarak sıralanabilir.
            [JsonPropertyName("timestamp")]
            public string? Timestamp { get; set; }
        }

        private sealed class TokenResponse
        {
            [JsonPropertyName("access_token")]
            public string? AccessToken { get; set; }
        }
    }
}
