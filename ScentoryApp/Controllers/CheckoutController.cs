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
            if (!ids.Any()) return Json(new { success = false, message = "Không có sản phẩm để thanh toán" });

            // 1. Lấy Đơn vị vận chuyển
            var dvvc = _context.DonViVanChuyens.FirstOrDefault();
            if (dvvc == null)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: Chưa cấu hình đơn vị vận chuyển." });
            }

            using var tx = _context.Database.BeginTransaction();
            try
            {
                // 2. Lấy giỏ hàng
                var cart = _context.GioHangs
                    .Include(g => g.ChiTietGioHangs)
                        .ThenInclude(c => c.IdSanPhamNavigation)
                    .FirstOrDefault(g => g.IdKhachHang == kh.IdKhachHang);

                if (cart == null) return Json(new { success = false, message = "Giỏ hàng không tồn tại" });

                var items = cart.ChiTietGioHangs
                    .Where(c => ids.Contains(c.IdSanPham))
                    .ToList();

                if (!items.Any()) return Json(new { success = false, message = "Sản phẩm không tồn tại trong giỏ" });

                // 3. Tính toán tiền
                decimal tienHang = items.Sum(i => i.SoLuong * i.IdSanPhamNavigation.GiaNiemYet);
                decimal ship = 30000;
                decimal giamGia = 0;

                // 4. Xử lý Mã giảm giá
                if (!string.IsNullOrEmpty(req.DiscountId))
                {
                    var voucher = _context.MaGiamGia.FirstOrDefault(m => m.IdMaGiamGia == req.DiscountId);

                    // Kiểm tra điều kiện:
                    if (voucher != null &&
                        voucher.ThoiGianKetThuc >= DateTime.Now &&
                        tienHang >= voucher.GiaTriToiThieu)
                    {
                        if (voucher.LoaiGiam == "%")
                        {
                            giamGia = tienHang * (voucher.GiaTriGiam / 100m);
                            if (voucher.GiaGiamToiDa.HasValue)
                            {
                                giamGia = Math.Min(giamGia, voucher.GiaGiamToiDa.Value);
                            }
                        }
                        else // Giảm tiền mặt ("VND")
                        {
                            giamGia = voucher.GiaTriGiam;
                        }
                    }
                    else
                    {
                        req.DiscountId = null; // Voucher không hợp lệ -> Hủy áp dụng
                    }
                }

                decimal tongTien = (tienHang + ship) - giamGia;
                if (tongTien < 0) tongTien = 0;

                // 5. Tạo Đơn hàng
                var donHang = new DonHang
                {
                    IdDonHang = GenerateDonHangId(),
                    IdKhachHang = kh.IdKhachHang,

                    ThoiGianDatHang = DateTime.Now,
                    NgayGiaoHangDuKien = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),

                    // Lấy ID thật từ DB
                    IdDonViVanChuyen = dvvc.IdDonViVanChuyen,

                    DiaChiNhanHang = req.Address,
                    PhiVanChuyen = ship,
                    ThueBanHang = 0,
                    TongTienDonHang = tongTien,

                    TinhTrangDonHang = "Đang chuẩn bị hàng",

                    ThoiGianCapNhat = null,
                    ThoiGianHoanTatDonHang = null,
                    IdMaGiamGia = (!string.IsNullOrEmpty(req.DiscountId) && giamGia > 0) ? req.DiscountId : null
                };

                _context.DonHangs.Add(donHang);
                _context.SaveChanges();

                // 6. Tạo Chi tiết đơn hàng
                foreach (var i in items)
                {
                    var ctdh = new ChiTietDonHang
                    {
                        IdDonHang = donHang.IdDonHang,
                        IdSanPham = i.IdSanPham,
                        SoLuong = i.SoLuong,
                        DonGia = i.IdSanPhamNavigation.GiaNiemYet,
                        ThanhTien = i.SoLuong * i.IdSanPhamNavigation.GiaNiemYet
                    };
                    _context.ChiTietDonHangs.Add(ctdh);
                }

                // 7. Xóa khỏi giỏ hàng
                _context.ChiTietGioHangs.RemoveRange(items);

                _context.SaveChanges();
                tx.Commit();

                // Xóa session
                HttpContext.Session.Remove(CHECKOUT_SESSION_KEY);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                tx.Rollback();
                return Json(new { success = false, message = "Lỗi xử lý: " + ex.Message });
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
            // Lấy ID lớn nhất hiện tại để tăng tự động: DH001 -> DH002
            var lastId = _context.DonHangs
                .OrderByDescending(d => d.IdDonHang)
                .Select(d => d.IdDonHang)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(lastId)) return "DH001";

            // Giả sử format luôn là DHxxx
            if (lastId.Length > 2 && int.TryParse(lastId.Substring(2), out int n))
            {
                return $"DH{(n + 1):D3}";
            }

            // Fallback nếu ID cũ không đúng định dạng
            return $"DH{DateTime.Now.Ticks % 1000:D3}";
        }
    }

    public class PlaceOrderRequest
    {
        public string Address { get; set; } = "";
        public string PaymentMethod { get; set; } = "";
        public string? DiscountId { get; set; }
    }
}