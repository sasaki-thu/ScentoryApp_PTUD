using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ScentoryApp.Models;

namespace ScentoryApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Shop()
        {
            return View();
        }

        public IActionResult Perfumes()
        {
            return View();
        }
        public IActionResult Oils()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult CS_Banhang()
        {
            return View();
        }
        public IActionResult CS_Giaohang()
        {
            return View();
        }
        public IActionResult CS_Doitra()
        {
            return View();
        }
        public IActionResult CS_Baomat()
        {
            return View();
        }

        public IActionResult Blog()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
