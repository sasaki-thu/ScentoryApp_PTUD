using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ScentoryApp.Models;

namespace ScentoryApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminUsersController : Controller
    {
        private readonly ScentoryPtudContext _context;

        public AdminUsersController(ScentoryPtudContext context)
        {
            _context = context;
        }

        // 1. GET: Trang danh sách
        public async Task<IActionResult> Index()
        {
            var listUsers = await _context.KhachHangs
                                          .Include(k => k.IdTaiKhoanNavigation)
                                          .OrderBy(u => u.HoTen)
                                          .ToListAsync();

            ViewData["ListTaiKhoan"] = new SelectList(_context.TaiKhoans, "IdTaiKhoan", "TenDangNhap");

            return View(listUsers);
        }


        // 2. API: Lấy chi tiết 1 khách hàng
        [HttpGet]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _context.KhachHangs.FindAsync(id);
            if (user == null) return Json(new { success = false, message = "Không tìm thấy khách hàng!" });

            return Json(new
            {
                success = true,
                data = new
                {
                    idKhachHang = user.IdKhachHang,
                    hoTen = user.HoTen,
                    email = user.Email,
                    sdt = user.Sdt,
                    diaChi = user.DiaChi,
                    gioiTinh = user.GioiTinh,
                    ngaySinh = user.NgaySinh.ToString("yyyy-MM-dd"),
                    idTaiKhoan = user.IdTaiKhoan
                }
            });
        }

        // 3. API: Lưu (Thêm / Sửa)
        [HttpPost]
        public async Task<IActionResult> Save(KhachHang model)
        {
            try
            {
                var exists = await _context.KhachHangs.AsNoTracking()
                                           .AnyAsync(x => x.IdKhachHang == model.IdKhachHang);

                if (exists)
                {
                    _context.Update(model);
                }
                else
                {
                    if (string.IsNullOrEmpty(model.IdKhachHang))
                    {
                        model.IdKhachHang = await GenerateKhachHangId();
                    }
                    _context.Add(model);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Lưu thành công!" });
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { success = false, message = "Lỗi hệ thống: " + msg });
            }
        }

        // 4. API: Xóa
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _context.KhachHangs.FindAsync(id);
            if (user == null) return Json(new { success = false, message = "Không tìm thấy khách hàng!" });

            // Kiểm tra ràng buộc khóa ngoại
            var hasOrder = await _context.DonHangs.AnyAsync(d => d.IdKhachHang == id);

            if (hasOrder)
            {
                return Json(new { success = false, message = "Không thể xóa! Khách hàng này đã có đơn hàng." });
            }

            _context.KhachHangs.Remove(user);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đã xóa thành công!" });
        }

        private async Task<string> GenerateKhachHangId()
        {
            var existingIds = await _context.KhachHangs
                .Where(k => k.IdKhachHang.StartsWith("KH"))
                .Select(k => k.IdKhachHang)
                .ToListAsync();
            int max = 0;
            foreach (var id in existingIds)
            {
                if (id.Length > 2 && int.TryParse(id.Substring(2), out int n))
                {
                    if (n > max) max = n;
                }
            }
            return "KH" + (max + 1).ToString("D3");
        }
        [HttpGet]
        public async Task<IActionResult> GetNextId()
        {
            try
            {
                var nextId = await GenerateKhachHangId();
                return Json(new { success = true, data = nextId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}