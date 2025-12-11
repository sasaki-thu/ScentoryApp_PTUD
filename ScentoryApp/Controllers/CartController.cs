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

        public CartController(ILogger<CartController> logger, ScentoryPtudContext context)
        {
            _logger = logger;
            _context = context;
        }
        
        [Authorize]
        public IActionResult Index()
        {
            return View();
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
            // Lấy id user đang đăng nhập
            var userId = User.Identity.Name;

            // Tìm giỏ hàng theo user
            var gioHang = _context.GioHangs.FirstOrDefault(g => g.IdKhachHang == userId);

            // Nếu chưa có giỏ → tạo mới
            if (gioHang == null)
            {
                gioHang = new GioHang
                {
                    IdGioHang = Guid.NewGuid().ToString(),
                    IdKhachHang = userId,
                    ThoiGianTaoGh = DateTime.Now
                };

                _context.GioHangs.Add(gioHang);
                _context.SaveChanges();
            }

            // Kiểm tra sản phẩm có trong giỏ chưa
            var chiTiet = _context.ChiTietGioHangs
                .FirstOrDefault(c => c.IdGioHang == gioHang.IdGioHang && c.IdSanPham == id);

            if (chiTiet == null)
            {
                // thêm mới vào giỏ
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
                // Nếu đã có thì tăng số lượng
                chiTiet.SoLuong++;
            }

            _context.SaveChanges();

            // Quay lại trang chủ hoặc giỏ hàng
            return RedirectToAction("Index", "Cart");
        }

    }
}