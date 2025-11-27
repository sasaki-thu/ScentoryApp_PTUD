using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Cần dòng này cho SelectList
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
            // Lấy danh sách khách hàng, kèm thông tin Tài khoản (nếu cần hiển thị tên TK ra bảng)
            var listUsers = await _context.KhachHangs
                                          .Include(k => k.IdTaiKhoanNavigation) // Include bảng Tài khoản
                                          .OrderBy(u => u.HoTen)
                                          .ToListAsync();

            // --- LẤY DANH SÁCH TÀI KHOẢN ĐỂ ĐỔ VÀO DROPDOWN ---
            // Hiển thị TenDangNhap, Giá trị là IdTaiKhoan
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

                    // Trả về ID Tài khoản để Javascript chọn trong Dropdown
                    idTaiKhoan = user.IdTaiKhoan
                }
            });
        }

        // 3. API: Lưu (Thêm / Sửa)
        [HttpPost]
        public async Task<IActionResult> Save(KhachHang model)
        {
            var existingUser = await _context.KhachHangs.AsNoTracking()
                                     .FirstOrDefaultAsync(x => x.IdKhachHang == model.IdKhachHang);
            try
            {
                if (existingUser == null)
                {
                    // === THÊM MỚI ===
                    _context.Add(model);
                }
                else
                {
                    // === CẬP NHẬT ===
                    _context.Update(model);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Lưu thành công!" });
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết (nếu có InnerException) để dễ debug
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
    }
}