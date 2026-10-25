namespace TurkishVacuum.Web.Services
{
    // Ana sayfada gösterilecek Instagram gönderilerini tutar. Liste arka planda
    // InstagramSyncService tarafından güncellenir; sayfa isteği Instagram'ı hiç beklemez.
    public class InstagramFeed
    {
        // Instagram bağlantısı kurulmamışsa veya ulaşılamazsa gösterilen gönderiler
        public static readonly IReadOnlyList<string> FallbackPermalinks = new[]
        {
            "https://www.instagram.com/reel/DQExjBnDJEh/",
            "https://www.instagram.com/reel/DP52felCSLZ/",
            "https://www.instagram.com/reel/DRpCYAaj-6-/",
            "https://www.instagram.com/reel/DRkgiCSgH8E/"
        };

        private volatile IReadOnlyList<string> _permalinks = FallbackPermalinks;

        public IReadOnlyList<string> Permalinks => _permalinks;

        public DateTimeOffset? UpdatedAt { get; private set; }

        public void Update(IReadOnlyList<string> permalinks)
        {
            if (permalinks.Count == 0) return;

            _permalinks = permalinks;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
