using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScentoryApp.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace ScentoryApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ScentoryPtudContext _context;

        public HomeController(ILogger<HomeController> logger, ScentoryPtudContext context)
        {
            _logger = logger;
            _context = context;
        }
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult About()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Shop(int page = 1)
        {
            const int pageSize = 10;

            if (page < 1) page = 1;

            var query = _context.SanPhams
                .Include(p => p.IdDanhMucSanPhamNavigation)
                .Where(p => p.TrangThaiSp)                      // chỉ lấy sp đang active
                .OrderByDescending(p => p.ThoiGianTaoSp);       // mới nhất lên trước

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var vm = new ShopViewModel
            {
                Products = products,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(vm);
        }

        [Authorize]
        public IActionResult Perfumes()
        {
            return View();
        }
        [Authorize]
        public IActionResult Oils()
        {
            return View();
        }
        [Authorize]
        public IActionResult Contact()
        {
            return View();
        }
        [Authorize]
        public IActionResult CS_Banhang()
        {
            return View();
        }
        [Authorize]
        public IActionResult CS_Giaohang()
        {
            return View();
        }
        [Authorize]
        public IActionResult CS_Doitra()
        {
            return View();
        }
        [Authorize]
        public IActionResult CS_Baomat()
        {
            return View();
        }
        [Authorize]
        public IActionResult Blog()
        {
            return View();
        }
        [Authorize]
        public async Task<IActionResult> ProductDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var product = await _context.SanPhams
                .Include(p => p.IdDanhMucSanPhamNavigation)
                .FirstOrDefaultAsync(p => p.IdSanPham == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }


        [AllowAnonymous] // ==== cho phép truy cập không cần login ====
        public IActionResult Login(string returnUrl = "/")
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        public async Task<IActionResult> LoginApi([FromBody] LoginRequest req)
        {
            // Giả lập validate (sau này bạn thay bằng DB)
            if (string.IsNullOrEmpty(req.Username) || string.IsNullOrEmpty(req.Password))
            {
                return Json(new { success = false, message = "Thông tin không hợp lệ." });
            }

            // ===== TẠO COOKIE LOGIN =====
            Console.WriteLine("123");
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, req.Username),
                new Claim("Role", req.Role)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            // ======= PHÂN QUYỀN REDIRECT =========
            string redirectUrl;

            if (req.Role == "Admin")
            {
                redirectUrl = "/Admin/Home/Index";   // luôn về Admin
            }
            else
            {
                // User → đi theo ReturnUrl nếu hợp lệ
                if (!string.IsNullOrEmpty(req.ReturnUrl) && Url.IsLocalUrl(req.ReturnUrl))
                    redirectUrl = req.ReturnUrl;
                else
                    redirectUrl = "/";  // về Home
            }

            return Json(new { success = true, returnUrl = redirectUrl });
        }

        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string Role { get; set; }
            public string ReturnUrl { get; set; }
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
