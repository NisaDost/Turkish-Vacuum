using System.Globalization;
using Microsoft.AspNetCore.Mvc.Razor;
using TurkishVacuum.Web.Models;

namespace TurkishVacuum.Web.Localization
{
    // Tüm görünümlerin temel sınıfı (_ViewImports.cshtml). Metinler görünümde iki dilli yazılır:
    //   @T("Ana Sayfa", "Home")
    // Dil, adresin /en ile başlayıp başlamadığına göre Program.cs'te belirlenir.
    public abstract class LocalizedRazorPage<TModel> : RazorPage<TModel>
    {
        public bool IsEnglish => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "en";

        public string Lang => IsEnglish ? "en" : "tr";

        public string T(string turkish, string english) => IsEnglish ? english : turkish;

        // Telefon numarası yabancı ziyaretçiler için uluslararası biçimde gösterilir.
        public string PhoneText => IsEnglish ? SiteInfo.PhoneDisplayInternational : SiteInfo.PhoneDisplay;

        public string WhatsAppLink => SiteInfo.WhatsAppLink(T(SiteInfo.DefaultWhatsAppMessage, SiteInfo.DefaultWhatsAppMessageEn));

        public string LoanerPumpWhatsAppLink => SiteInfo.WhatsAppLink(T(SiteInfo.LoanerPumpMessage, SiteInfo.LoanerPumpMessageEn));

        // Geçerli dildeki sayfa adresi, ör. PageUrl("Destek") → "/destek" veya "/en/support"
        public string PageUrl(string action) => Context.Request.PathBase + SiteRoutes.PathFor(action, IsEnglish);
    }
}
