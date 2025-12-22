using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScentoryApp.Models;

namespace ScentoryApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminCouponsController : Controller
    {
        private readonly ScentoryPtudContext _context;

        public AdminCouponsController(ScentoryPtudContext context)
        {
            _context = context;
        }

        // 1. GET: Hiển thị trang danh sách
        public async Task<IActionResult> Index()
        {
            var coupons = await _context.MaGiamGia
                                .OrderByDescending(c => c.ThoiGianBatDau) 
                                .ToListAsync();
            return View(coupons);
        }

        // 2. API: Lấy chi tiết
        [HttpGet]
        public async Task<IActionResult> GetById(string id)
        {
            var coupon = await _context.MaGiamGia.FindAsync(id);
            if (coupon == null) return Json(new { success = false, message = "Không tìm thấy mã giảm giá!" });

            return Json(new
            {
                success = true,
                data = new
                {
                    idMaGiamGia = coupon.IdMaGiamGia,
                    moTa = coupon.MoTa,
                    loaiGiam = coupon.LoaiGiam,
                    giaTriGiam = coupon.GiaTriGiam,
                    giaTriToiThieu = coupon.GiaTriToiThieu,
                    giaGiamToiDa = coupon.GiaGiamToiDa, // Có thể null
                    gioiHanSuDung = coupon.GioiHanSuDung,
                    thoiGianBatDau = coupon.ThoiGianBatDau.ToString("dd/MM/yyyy HH:mm"),
                    thoiGianKetThuc = coupon.ThoiGianKetThuc.ToString("dd/MM/yyyy HH:mm")
                }
            });
        }

        // 3. API: Lưu (Thêm / Sửa)
        [HttpPost]
        public async Task<IActionResult> Save(MaGiamGium model)
        {
            var existingCoupon = await _context.MaGiamGia.AsNoTracking()
                                       .FirstOrDefaultAsync(x => x.IdMaGiamGia == model.IdMaGiamGia);

            try
            {
                if (existingCoupon == null)
                {
                    if (string.IsNullOrEmpty(model.IdMaGiamGia))
                    {
                        model.IdMaGiamGia = await GenerateMaGiamGiaId();
                    }
                    // Thêm mới
                    _context.Add(model);
                }
                else
                {
                    // Cập nhật
                    _context.Update(model);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Lưu thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // 4. API: Xóa
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var coupon = await _context.MaGiamGia.FindAsync(id);
            if (coupon == null) return Json(new { success = false, message = "Không tìm thấy mã!" });

            // Check ràng buộc khóa ngoại trước khi xóa
            var hasOrder = await _context.DonHangs.AnyAsync(d => d.IdMaGiamGia == id);
            if (hasOrder) return Json(new { success = false, message = "Mã này đã được sử dụng, không thể xóa!" });

            _context.MaGiamGia.Remove(coupon);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đã xóa thành công!" });
        }
        private async Task<string> GenerateMaGiamGiaId()
        {
            var existingIds = await _context.MaGiamGia
                .Where(s => s.IdMaGiamGia.StartsWith("GG"))
                .Select(s => s.IdMaGiamGia)
                .ToListAsync();

            int max = 0;
            foreach (var id in existingIds)
            {
                if (id.Length > 2 && int.TryParse(id.Substring(2), out int n))
                {
                    if (n > max) max = n;
                }
            }
            return "GG" + (max + 1).ToString("D3");
        }

        [HttpGet]
        public async Task<IActionResult> GetNextId()
        {
            try
            {
                var nextId = await GenerateMaGiamGiaId();
                return Json(new { success = true, data = nextId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}