using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScentoryApp.Models;

namespace ScentoryApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminAccountsController : Controller
    {
        private readonly ScentoryPtudContext _context;

        public AdminAccountsController(ScentoryPtudContext context)
        {
            _context = context;
        }

        // 1. GET: Trang danh sách
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách tài khoản (có thể include KhachHang để hiển thị tên nếu muốn xem, nhưng k cần map)
            var listTaiKhoan = await _context.TaiKhoans
                                     .Include(t => t.KhachHangs)
                                     .ToListAsync();
            return View(listTaiKhoan);
        }

        // 2. API: Lấy chi tiết
        [HttpGet]
        public async Task<IActionResult> GetById(string id)
        {
            var tk = await _context.TaiKhoans.FindAsync(id);
            if (tk == null) return Json(new { success = false, message = "Không tìm thấy tài khoản!" });

            return Json(new
            {
                success = true,
                data = new
                {
                    idTaiKhoan = tk.IdTaiKhoan,
                    tenDangNhap = tk.TenDangNhap,
                    matKhau = tk.MatKhau,
                    vaiTro = tk.VaiTro
                    // Đã bỏ phần IdKhachHang
                }
            });
        }

        // 3. API: Lưu (Đã bỏ logic map khách hàng)
        [HttpPost]
        public async Task<IActionResult> Save(TaiKhoan model)
        {
            var existingAcc = await _context.TaiKhoans.AsNoTracking()
                                    .FirstOrDefaultAsync(x => x.IdTaiKhoan == model.IdTaiKhoan);
            try
            {
                if (existingAcc == null)
                {
                    // === THÊM MỚI ===
                    // Kiểm tra trùng tên đăng nhập
                    if (await _context.TaiKhoans.AnyAsync(u => u.TenDangNhap == model.TenDangNhap))
                    {
                        return Json(new { success = false, message = "Tên đăng nhập đã tồn tại!" });
                    }
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
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // 4. API: Xóa
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var tk = await _context.TaiKhoans.FindAsync(id);
            if (tk == null) return Json(new { success = false, message = "Không tìm thấy tài khoản!" });

            // Kiểm tra xem tài khoản này có đang được khách hàng nào sử dụng không (để tránh lỗi SQL)
            var isUsed = await _context.KhachHangs.AnyAsync(k => k.IdTaiKhoan == id);
            if (isUsed)
            {
                return Json(new { success = false, message = "Không thể xóa! Tài khoản này đang được sử dụng bởi một khách hàng." });
            }

            _context.TaiKhoans.Remove(tk);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đã xóa thành công!" });
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Home", new { area = "" });
        }
    }
}