using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScentoryApp.Models;
using System.Diagnostics;
namespace ScentoryApp.Controllers
{
    public class CartController : Controller
    {
        private readonly ILogger<CartController> _logger;
        private readonly ScentoryPtudContext _context;
        private static readonly object _cartIdLock = new object();
        public CartController(ILogger<CartController> logger, ScentoryPtudContext context)
        {
            _logger = logger;
            _context = context;
        }
        
        [Authorize]
        public IActionResult Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // Lấy giỏ hàng của khách
            var gioHang = _context.GioHangs
                .FirstOrDefault(g => g.IdKhachHang == userId);

            if (gioHang == null)
                return View(new List<ChiTietGioHang>()); // Giỏ trống

            // Lấy danh sách sản phẩm trong giỏ, include sản phẩm
            var chiTiet = _context.ChiTietGioHangs
                .Include(c => c.IdSanPhamNavigation) // chỉ load sản phẩm
                .Where(c => c.IdGioHang == gioHang.IdGioHang)
                .ToList();

            return View(chiTiet);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpGet]
        [Authorize]
        public IActionResult AddToCart(string id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // Lấy giỏ hàng của khách
            var gioHang = _context.GioHangs
                .FirstOrDefault(g => g.IdKhachHang == userId);

            // Nếu chưa có giỏ → tạo mới
            if (gioHang == null)
            {
                string newId;

                lock (_cartIdLock) // đảm bảo không trùng ID
                {
                    var lastCart = _context.GioHangs
                        .OrderByDescending(g => g.IdGioHang)
                        .FirstOrDefault();

                    if (lastCart == null)
                        newId = "GH001";
                    else
                    {
                        int number = int.Parse(lastCart.IdGioHang.Substring(2));
                        newId = "GH" + (number + 1).ToString("D3");
                    }

                    gioHang = new GioHang
                    {
                        IdGioHang = newId,
                        IdKhachHang = userId,
                        ThoiGianTaoGh = DateTime.Now
                    };

                    _context.GioHangs.Add(gioHang);
                    _context.SaveChanges();
                }
            }

            // Kiểm tra sản phẩm trong giỏ
            var chiTiet = _context.ChiTietGioHangs
                .FirstOrDefault(c => c.IdGioHang == gioHang.IdGioHang && c.IdSanPham == id);

            if (chiTiet == null)
            {
                chiTiet = new ChiTietGioHang
                {
                    IdGioHang = gioHang.IdGioHang,
                    IdSanPham = id,
                    SoLuong = 1
                };
                _context.ChiTietGioHangs.Add(chiTiet);
            }
            else
            {
                chiTiet.SoLuong++;
            }

            _context.SaveChanges();

            return RedirectToAction("Index", "Cart");
        }
    }
}