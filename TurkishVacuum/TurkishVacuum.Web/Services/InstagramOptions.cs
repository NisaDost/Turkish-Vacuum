namespace TurkishVacuum.Web.Services
{
    // appsettings.json içindeki "Instagram" bölümü. Token git'e girmez; sunucudaki
    // appsettings.json veya Instagram__AccessToken ortam değişkeniyle verilir.
    public class InstagramOptions
    {
        public const string SectionName = "Instagram";

        // Instagram profesyonel hesabı için uzun ömürlü (60 gün) erişim anahtarı.
        // Sunucu bu anahtarı kendisi yeniler; yalnızca ilk kurulumda girilmesi yeterlidir.
        public string? AccessToken { get; set; }

        // Yenilenen anahtarın saklandığı dosya (uygulama klasörüne göre).
        public string TokenStorePath { get; set; } = "App_Data/instagram-token.json";

        // Sayfada gösterilecek gönderi sayısı.
        public int PostCount { get; set; } = 4;

        // En popülerler, son kaç gönderi arasından seçilsin. Böylece eski bir gönderi sonsuza
        // kadar listede kalmaz, yeni paylaşımlar da sıralamaya girer.
        public int CandidateCount { get; set; } = 50;

        // Gönderi listesinin kaç dakikada bir Instagram'dan yeniden çekileceği.
        public int RefreshMinutes { get; set; } = 360;

        public string ApiBaseUrl { get; set; } = "https://graph.instagram.com/";

        // Boş bırakılırsa Meta'nın uygulama için ayarlı varsayılan API sürümü kullanılır.
        public string? ApiVersion { get; set; }
    }
}
