using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TurkishVacuum.Web.Models;

namespace TurkishVacuum.Web.Controllers
{
    // Sayfalar arama motorları için anlamlı adreslerde yayınlanır: Türkçe (ör. /yedek-parca)
    // ve İngilizce (ör. /en/spare-parts). Adresler Models/SiteRoutes.cs içinde tanımlıdır.
    // Eski /Home/... adresleri Program.cs içinde 301 ile Türkçe adreslere yönlendirilir.
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [Route(SiteRoutes.IndexTr)]
        [Route(SiteRoutes.IndexEn)]
        public IActionResult Index()
        {
            return View();
        }

        [Route(SiteRoutes.YedekParcaTr)]
        [Route(SiteRoutes.YedekParcaEn)]
        public IActionResult Yedek_Parca()
        {
            return View();
        }

        [Route(SiteRoutes.DestekTr)]
        [Route(SiteRoutes.DestekEn)]
        public IActionResult Destek()
        {
            return View();
        }

        [Route(SiteRoutes.IletisimTr)]
        [Route(SiteRoutes.IletisimEn)]
        public IActionResult Iletisim()
        {
            return View();
        }

        [Route(SiteRoutes.HakkimizdaTr)]
        [Route(SiteRoutes.HakkimizdaEn)]
        public IActionResult Hakkimizda()
        {
            return View();
        }

        [Route("hata/{statusCode:int?}")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? statusCode)
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = statusCode
            });
        }
    }
}
