using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using ScentoryApp.Models;
using System.Diagnostics;
using System.Security.Claims;
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
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (accountId == null)
                return RedirectToAction("Login", "Home");

            var khachHang = _context.KhachHangs
                .FirstOrDefault(k => k.IdTaiKhoan == accountId);

            if (khachHang == null)
                return View(new List<ChiTietGioHang>());

            var gioHang = _context.GioHangs
                .FirstOrDefault(g => g.IdKhachHang == khachHang.IdKhachHang);

            if (gioHang == null)
                return View(new List<ChiTietGioHang>());

            var chiTiet = _context.ChiTietGioHangs
                .Include(c => c.IdSanPhamNavigation)
                .Where(c => c.IdGioHang == gioHang.IdGioHang)
                .ToList();

            return View(chiTiet);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpPost]
        [Authorize]
        [IgnoreAntiforgeryToken]
        public IActionResult AddToCart([FromForm] string id, [FromForm] int quantity = 1)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (accountId == null)
                return Unauthorized();

            var khachHang = _context.KhachHangs
                .FirstOrDefault(k => k.IdTaiKhoan == accountId);

            if (khachHang == null)
                return Unauthorized();

            // 1. Lấy giỏ hàng
            var gioHang = _context.GioHangs
                .FirstOrDefault(g => g.IdKhachHang == khachHang.IdKhachHang);

            // 2. Tạo mới nếu chưa có
            if (gioHang == null)
            {
                lock (_cartIdLock)
                {
                    var lastCart = _context.GioHangs
                        .OrderByDescending(g => g.IdGioHang)
                        .FirstOrDefault();

                    int nextNumber = lastCart == null
                        ? 1
                        : int.Parse(lastCart.IdGioHang.Substring(2)) + 1;

                    gioHang = new GioHang
                    {
                        IdGioHang = "GH" + nextNumber.ToString("D3"),
                        IdKhachHang = khachHang.IdKhachHang,
                        ThoiGianTaoGh = DateTime.Now
                    };

                    _context.GioHangs.Add(gioHang);
                    _context.SaveChanges();
                }
            }

            // 3. Thêm / update chi tiết
            var chiTiet = _context.ChiTietGioHangs
                .FirstOrDefault(c => c.IdGioHang == gioHang.IdGioHang && c.IdSanPham == id);

            if (chiTiet == null)
            {
                _context.ChiTietGioHangs.Add(new ChiTietGioHang
                {
                    IdGioHang = gioHang.IdGioHang,
                    IdSanPham = id,
                    SoLuong = quantity
                });
            }
            else
            {
                chiTiet.SoLuong += quantity;
            }

            gioHang.ThoiGianCapNhatGh = DateTime.Now;
            _context.SaveChanges();

            return Json(new { success = true });
        }

        [Authorize]
        [HttpGet]
        public IActionResult MiniCart()
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (accountId == null)
                return Json(new { items = new List<object>() });

            var kh = _context.KhachHangs
                .FirstOrDefault(k => k.IdTaiKhoan == accountId);

            if (kh == null)
                return Json(new { items = new List<object>() });

            var cart = _context.GioHangs
                .Include(g => g.ChiTietGioHangs)
                    .ThenInclude(c => c.IdSanPhamNavigation)
                .FirstOrDefault(g => g.IdKhachHang == kh.IdKhachHang);

            if (cart == null)
                return Json(new { items = new List<object>(), total = 0 });

            var items = cart.ChiTietGioHangs.Select(c => new
            {
                id = c.IdSanPham,
                name = c.IdSanPhamNavigation.TenSanPham,
                price = c.IdSanPhamNavigation.GiaNiemYet,
                quantity = c.SoLuong,
                img = c.IdSanPhamNavigation.AnhSanPham != null
                    ? $"data:image/jpeg;base64,{Convert.ToBase64String(c.IdSanPhamNavigation.AnhSanPham)}"
                    : "/assets/images/placeholder-product.png"
            });

            var total = cart.ChiTietGioHangs.Sum(c => c.SoLuong * c.IdSanPhamNavigation.GiaNiemYet);

            return Json(new { items, total });
        }
        [Authorize]
        [HttpGet]
        public IActionResult GetCartItems()
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (accountId == null)
                return Json(new { items = new List<object>(), total = 0 });

            var kh = _context.KhachHangs
                .FirstOrDefault(k => k.IdTaiKhoan == accountId);

            if (kh == null)
                return Json(new { items = new List<object>(), total = 0 });

            var cart = _context.GioHangs
                .Include(g => g.ChiTietGioHangs)
                    .ThenInclude(c => c.IdSanPhamNavigation)
                .FirstOrDefault(g => g.IdKhachHang == kh.IdKhachHang);

            if (cart == null)
                return Json(new { items = new List<object>(), total = 0 });

            var items = cart.ChiTietGioHangs.Select(c => new
            {
                id = c.IdSanPham,
                name = c.IdSanPhamNavigation.TenSanPham,
                img = c.IdSanPhamNavigation.AnhSanPham != null
                    ? $"data:image/jpeg;base64,{Convert.ToBase64String(c.IdSanPhamNavigation.AnhSanPham)}"
                    : "/assets/images/placeholder-product.png",
                price = c.IdSanPhamNavigation.GiaNiemYet,
                quantity = c.SoLuong,
                subtotal = c.SoLuong * c.IdSanPhamNavigation.GiaNiemYet
            });

            var total = cart.ChiTietGioHangs.Sum(x => x.SoLuong * x.IdSanPhamNavigation.GiaNiemYet);

            return Json(new { items, total });
        }

        [HttpPost]
        [Authorize]
        [IgnoreAntiforgeryToken]
        public IActionResult UpdateQuantity(string productId, int delta)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (accountId == null) return Unauthorized();

            var kh = _context.KhachHangs.FirstOrDefault(k => k.IdTaiKhoan == accountId);
            if (kh == null) return Unauthorized();

            var cart = _context.GioHangs
                .Include(g => g.ChiTietGioHangs)
                .FirstOrDefault(g => g.IdKhachHang == kh.IdKhachHang);

            if (cart == null) return Json(new { success = true });

            var item = cart.ChiTietGioHangs.FirstOrDefault(c => c.IdSanPham == productId);
            if (item == null) return Json(new { success = true });

            item.SoLuong += delta;

            if (item.SoLuong <= 0)
            {
                _context.ChiTietGioHangs.Remove(item);
            }

            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        [Authorize]
        [IgnoreAntiforgeryToken]
        public IActionResult RemoveItem(string productId)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (accountId == null) return Unauthorized();

            var kh = _context.KhachHangs.FirstOrDefault(k => k.IdTaiKhoan == accountId);
            if (kh == null) return Unauthorized();

            var cart = _context.GioHangs
                .Include(g => g.ChiTietGioHangs)
                .FirstOrDefault(g => g.IdKhachHang == kh.IdKhachHang);

            if (cart == null) return Json(new { success = true });

            var item = cart.ChiTietGioHangs.FirstOrDefault(c => c.IdSanPham == productId);
            if (item != null)
            {
                _context.ChiTietGioHangs.Remove(item);
                _context.SaveChanges();
            }

            return Json(new { success = true });
        }
        string GenerateDonHangId()
        {
            var last = _context.DonHangs
                .OrderByDescending(d => d.IdDonHang)
                .Select(d => d.IdDonHang)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(last))
                return "DH001";

            int num = int.Parse(last.Substring(2));
            return "DH" + (num + 1).ToString("D3");
        }

        [HttpPost]
        [Authorize]
        [IgnoreAntiforgeryToken]
        public IActionResult Checkout(string bankAccount, string bankOwner, string bankName)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var kh = _context.KhachHangs
                    .FirstOrDefault(k => k.IdTaiKhoan == accountId);

                if (kh == null)
                    return Json(new { success = false });

                var cart = _context.GioHangs
                    .Include(g => g.ChiTietGioHangs)
                        .ThenInclude(c => c.IdSanPhamNavigation)
                    .FirstOrDefault(g => g.IdKhachHang == kh.IdKhachHang);

                if (cart == null || !cart.ChiTietGioHangs.Any())
                    return Json(new { success = false, message = "Giỏ hàng trống" });

                // 1. Tạo DonHang
                var donHang = new DonHang
                {
                    IdDonHang = GenerateDonHangId(),
                    IdKhachHang = kh.IdKhachHang,
                    ThoiGianDatHang = DateTime.Now,
                    NgayGiaoHangDuKien = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
                    IdDonViVanChuyen = "VC001",
                    PhiVanChuyen = 30000,
                    ThueBanHang = 0,
                    TinhTrangDonHang = "Đang chuẩn bị hàng",
                    TongTienDonHang = cart.ChiTietGioHangs.Sum(x => x.SoLuong * x.IdSanPhamNavigation.GiaNiemYet) + 30000,
                    IdMaGiamGia = "GG002"
                };

                _context.DonHangs.Add(donHang);
                _context.SaveChanges();

                // 2. ChiTietDonHang
                foreach (var item in cart.ChiTietGioHangs)
                {
                    _context.ChiTietDonHangs.Add(new ChiTietDonHang
                    {
                        IdDonHang = donHang.IdDonHang,
                        IdSanPham = item.IdSanPham,
                        DonGia = item.IdSanPhamNavigation.GiaNiemYet,
                        SoLuong = item.SoLuong,
                        ThanhTien = item.SoLuong * item.IdSanPhamNavigation.GiaNiemYet
                    });
                }

                // 3. Xóa giỏ hàng
                _context.ChiTietGioHangs.RemoveRange(cart.ChiTietGioHangs);

                _context.SaveChanges();
                transaction.Commit();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}