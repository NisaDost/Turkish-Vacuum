namespace TurkishVacuum.Web.Models
{
    // Her sayfanın Türkçe ve İngilizce adresi. Controller rotaları, menü bağlantıları,
    // dil değiştirici, canonical/hreflang etiketleri ve sitemap aynı tablodan beslenir.
    public static class SiteRoutes
    {
        public const string EnglishPrefix = "en";

        public const string IndexTr = "";
        public const string IndexEn = EnglishPrefix;
        public const string YedekParcaTr = "yedek-parca";
        public const string YedekParcaEn = EnglishPrefix + "/spare-parts";
        public const string DestekTr = "destek";
        public const string DestekEn = EnglishPrefix + "/support";
        public const string HakkimizdaTr = "hakkimizda";
        public const string HakkimizdaEn = EnglishPrefix + "/about";
        public const string IletisimTr = "iletisim";
        public const string IletisimEn = EnglishPrefix + "/contact";

        // Anahtar: HomeController'daki action adı
        private static readonly Dictionary<string, (string Tr, string En)> Pages = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Index"] = (IndexTr, IndexEn),
            ["Yedek_Parca"] = (YedekParcaTr, YedekParcaEn),
            ["Destek"] = (DestekTr, DestekEn),
            ["Hakkimizda"] = (HakkimizdaTr, HakkimizdaEn),
            ["Iletisim"] = (IletisimTr, IletisimEn)
        };

        public static bool IsPage(string? action) => action is not null && Pages.ContainsKey(action);

        // Sayfanın kök göreli yolu, ör. "/destek" veya "/en/support"
        public static string PathFor(string action, bool english)
        {
            var (tr, en) = Pages[action];
            return "/" + (english ? en : tr);
        }

        public static bool IsEnglishPath(PathString path) =>
            path.StartsWithSegments("/" + EnglishPrefix, StringComparison.OrdinalIgnoreCase);
    }
}
