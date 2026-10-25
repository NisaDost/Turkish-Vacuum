namespace TurkishVacuum.Web.Models
{
    // Sitenin birçok yerinde tekrar eden iletişim ve firma bilgileri tek yerden yönetilir.
    public static class SiteInfo
    {
        public const string BaseUrl = "https://turkishvacuum.com";
        public const string Name = "Turkish Vakum Cihazları ve Teknolojisi";
        public const string ShortName = "Turkish Vakum";

        public const string Phone = "+905053662656";
        public const string PhoneDisplay = "0505 366 26 56";
        public const string PhoneDisplayInternational = "+90 505 366 26 56";
        public const string ContactPerson = "İbrahim Dost";
        public const string Email = "idost38@gmail.com";

        public const string StreetAddress = "Aşık Veysel Mah. 5847. Sk. No:11";
        public const string District = "Karabağlar";
        public const string City = "İzmir";
        public const string PostalCode = "35100";
        public const string Address = StreetAddress + ", " + PostalCode + " " + District + "/" + City;

        public const string MapsUrl = "https://www.google.com/maps?cid=8953888573542443137";
        public const string MapsEmbedUrl = "https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3127.5889501079105!2d27.135113375603648!3d38.38162947183865!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x14bbdf3be3a56009%3A0x7c429a37c86d2881!2sTurkish%20Vakum%20Cihazlar%C4%B1%20ve%20Teknolojisi%20%C4%B0zmir!5e0!3m2!1str!2str!4v1764594045522!5m2!1str!2str";
        public const string FacebookUrl = "https://www.facebook.com/people/Turk%C4%B1sh-vakum-cihazlar%C4%B1-ve-teknolojisi-izmir/100083208278701/";
        public const string InstagramUrl = "https://www.instagram.com/ibrahim.dost/";

        public const string DefaultWhatsAppMessage = "Merhaba, vakum cihazımda bir sorun yaşıyorum. Yardımcı olabilir misiniz?";
        public const string DefaultWhatsAppMessageEn = "Hello, I have a problem with my vacuum pump. Can you help?";
        public const string LoanerPumpMessage = "Merhaba, pompam tamirdeyken kullanmak için yedek (konsinye) vakum pompası talep etmek istiyorum.";
        public const string LoanerPumpMessageEn = "Hello, I'd like to request a loaner (consignment) vacuum pump to use while my pump is being repaired.";

        public static string PhoneLink => "tel:" + Phone;

        public static string WhatsAppLink(string message = DefaultWhatsAppMessage) =>
            $"https://wa.me/{Phone.TrimStart('+')}?text={Uri.EscapeDataString(message)}";

        // Google'ın işletmeyi tanıması için schema.org LocalBusiness verisi (JSON-LD)
        public static Dictionary<string, object> LocalBusinessSchema(bool english) => new()
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "LocalBusiness",
            ["@id"] = BaseUrl + "/#isletme",
            ["name"] = Name,
            ["alternateName"] = new[] { ShortName, "Turkish Vacuum" },
            ["description"] = english
                ? "24/7 vacuum pump repair, maintenance, overhaul and spare parts based in İzmir, Türkiye. Loaner (consignment) pumps available during repairs."
                : "İzmir merkezli 7/24 vakum pompası tamir, bakım, revizyon ve yedek parça hizmeti. Tamir süresince yedek (konsinye) pompa desteği.",
            ["url"] = BaseUrl + "/",
            ["knowsLanguage"] = new[] { "tr", "en" },
            ["logo"] = BaseUrl + "/images/Logo.png",
            ["image"] = BaseUrl + "/images/slide1.jpg",
            ["telephone"] = Phone,
            ["email"] = Email,
            ["foundingDate"] = "2015",
            ["address"] = new Dictionary<string, object>
            {
                ["@type"] = "PostalAddress",
                ["streetAddress"] = StreetAddress,
                ["addressLocality"] = District,
                ["addressRegion"] = City,
                ["postalCode"] = PostalCode,
                ["addressCountry"] = "TR"
            },
            ["geo"] = new Dictionary<string, object>
            {
                ["@type"] = "GeoCoordinates",
                ["latitude"] = 38.3816,
                ["longitude"] = 27.1351
            },
            ["hasMap"] = MapsUrl,
            ["openingHoursSpecification"] = new Dictionary<string, object>
            {
                ["@type"] = "OpeningHoursSpecification",
                ["dayOfWeek"] = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" },
                ["opens"] = "00:00",
                ["closes"] = "23:59"
            },
            ["areaServed"] = new Dictionary<string, object>
            {
                ["@type"] = "Country",
                ["name"] = "Türkiye"
            },
            ["sameAs"] = new[] { FacebookUrl, InstagramUrl },
            ["makesOffer"] = (english
                ? new[]
                {
                    "Vacuum pump repair",
                    "Periodic vacuum pump maintenance",
                    "Vacuum pump overhaul",
                    "Vacuum pump spare parts",
                    "Loaner (consignment) vacuum pump during repairs",
                    "New and used vacuum pump supply"
                }
                : new[]
                {
                    "Vakum pompası tamiri",
                    "Vakum pompası periyodik bakımı",
                    "Vakum pompası revizyonu",
                    "Vakum pompası yedek parça temini",
                    "Tamir süresince yedek (konsinye) vakum pompası",
                    "Sıfır ve 2. el vakum pompası temini"
                }).Select(service => new Dictionary<string, object>
            {
                ["@type"] = "Offer",
                ["itemOffered"] = new Dictionary<string, object>
                {
                    ["@type"] = "Service",
                    ["name"] = service
                }
            }).ToArray()
        };
    }
}
