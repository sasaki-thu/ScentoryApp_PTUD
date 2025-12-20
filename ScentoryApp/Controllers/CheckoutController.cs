using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScentoryApp.Models;
using System.Security.Claims;
using System.Text.Json;

namespace ScentoryApp.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ScentoryPtudContext _context;
        private const string CHECKOUT_SESSION_KEY = "CHECKOUT_ITEMS";

        public CheckoutController(ScentoryPtudContext context)
        {
            _context = context;
        }

        public IActionResult Index() => View();

        [HttpPost]
        public IActionResult Prepare([FromBody] List<string> productIds)
        {
            HttpContext.Session.SetString(
                CHECKOUT_SESSION_KEY,
                JsonSerializer.Serialize(productIds)
            );
            return Ok();
        }

        [HttpGet]
        public IActionResult GetCheckoutItems()
        {
            var kh = GetKhachHang();
            if (kh == null) return Unauthorized();

            var ids = GetSelectedIds();
            if (!ids.Any())
                return Json(new { empty = true });

            var cart = _context.GioHangs
                .Include(g => g.ChiTietGioHangs)
                    .ThenInclude(c => c.IdSanPhamNavigation)
                .FirstOrDefault(g => g.IdKhachHang == kh.IdKhachHang);

            var items = cart?.ChiTietGioHangs
                .Where(c => ids.Contains(c.IdSanPham))
                .Select(c => new
                {
                    id = c.IdSanPham,
                    name = c.IdSanPhamNavigation.TenSanPham,
                    quantity = c.SoLuong,
                    subtotal = c.SoLuong * c.IdSanPhamNavigation.GiaNiemYet,
                    img = c.IdSanPhamNavigation.AnhSanPham != null
                        ? $"data:image/jpeg;base64,{Convert.ToBase64String(c.IdSanPhamNavigation.AnhSanPham)}"
                        : "/assets/images/placeholder-product.png"
                }).ToList();

            return Json(new { items });
        }

        [HttpPost]
        public IActionResult PlaceOrder([FromBody] PlaceOrderRequest req)
        {
            var kh = GetKhachHang();
            if (kh == null) return Unauthorized();

            var ids = GetSelectedIds();
            if (!ids.Any())
                return Json(new { success = false, message = "Không có sản phẩm" });

            using var tx = _context.Database.BeginTransaction();

            try
            {
                var cart = _context.GioHangs
                    .Include(g => g.ChiTietGioHangs)
                        .ThenInclude(c => c.IdSanPhamNavigation)
                    .First(g => g.IdKhachHang == kh.IdKhachHang);

                var items = cart.ChiTietGioHangs
                    .Where(c => ids.Contains(c.IdSanPham))
                    .ToList();

                decimal tienHang = items.Sum(i => i.SoLuong * i.IdSanPhamNavigation.GiaNiemYet);
                decimal ship = 30000;
                decimal tong = tienHang + ship;

                var donHang = new DonHang
                {
                    IdDonHang = GenerateDonHangId(),
                    IdKhachHang = kh.IdKhachHang,
                    ThoiGianDatHang = DateTime.Now,
                    DiaChiNhanHang = req.Address,
                    NgayGiaoHangDuKien = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
                    IdDonViVanChuyen = "VC001",
                    PhiVanChuyen = ship,
                    ThueBanHang = 0,
                    TinhTrangDonHang = "Đang chuẩn bị hàng",
                    TongTienDonHang = tong,
                    IdMaGiamGia = req.DiscountId ?? null
                };

                _context.DonHangs.Add(donHang);
                _context.SaveChanges();

                foreach (var i in items)
                {
                    _context.ChiTietDonHangs.Add(new ChiTietDonHang
                    {
                        IdDonHang = donHang.IdDonHang,
                        IdSanPham = i.IdSanPham,
                        SoLuong = i.SoLuong,
                        DonGia = i.IdSanPhamNavigation.GiaNiemYet,
                        ThanhTien = i.SoLuong * i.IdSanPhamNavigation.GiaNiemYet
                    });
                }

                _context.ChiTietGioHangs.RemoveRange(items);
                _context.SaveChanges();
                tx.Commit();

                HttpContext.Session.Remove(CHECKOUT_SESSION_KEY);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                tx.Rollback();
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ===== Helpers =====
        private KhachHang? GetKhachHang()
        {
            var accId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return _context.KhachHangs.FirstOrDefault(k => k.IdTaiKhoan == accId);
        }

        private List<string> GetSelectedIds()
        {
            var s = HttpContext.Session.GetString(CHECKOUT_SESSION_KEY);
            return string.IsNullOrEmpty(s)
                ? new()
                : JsonSerializer.Deserialize<List<string>>(s)!;
        }

        private string GenerateDonHangId()
        {
            var last = _context.DonHangs
                .OrderByDescending(d => d.IdDonHang)
                .Select(d => d.IdDonHang)
                .FirstOrDefault();

            int n = last == null ? 0 : int.Parse(last[2..]);
            return $"DH{(n + 1):D3}";
        }
    }

    public class PlaceOrderRequest
    {
        public string Address { get; set; } = "";
        public string PaymentMethod { get; set; } = ""; // COD | ONLINE
        public string? DiscountId { get; set; }
    }

}