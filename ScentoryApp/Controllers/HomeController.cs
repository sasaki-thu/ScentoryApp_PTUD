using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ScentoryApp.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

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

        [HttpPost]
        public async Task<IActionResult> Login(TaiKhoan model)
        {
            // This is a dummy login for demonstration.
            // In a real application, you would validate the user's credentials against a database.
            if (!string.IsNullOrEmpty(model.TenDangNhap) && !string.IsNullOrEmpty(model.MatKhau))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.TenDangNhap),
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Home");
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult Account()
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

