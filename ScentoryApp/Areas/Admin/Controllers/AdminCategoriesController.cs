using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScentoryApp.Models;

namespace ScentoryApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminCategoriesController : Controller
    {
        private readonly ScentoryPtudContext _context;

        public AdminCategoriesController(ScentoryPtudContext context)
        {
            _context = context;
        }

        // 1. GET: Hiển thị trang danh sách
        public async Task<IActionResult> Index()
        {
            var listDanhMuc = await _context.DanhMucSanPhams
                                    .OrderByDescending(d => d.ThoiGianTaoDm)
                                    .ToListAsync();
            return View(listDanhMuc);
        }

        // 2. API: Lấy chi tiết 1 danh mục (Cho nút Edit / Details)
        [HttpGet]
        public async Task<IActionResult> GetById(string id)
        {
            var dm = await _context.DanhMucSanPhams.FindAsync(id);
            if (dm == null) return Json(new { success = false, message = "Không tìm thấy danh mục!" });

            // Xử lý ảnh (nếu có)
            string imageBase64 = null;
            if (dm.AnhDanhMuc != null && dm.AnhDanhMuc.Length > 0)
            {
                string base64Data = Convert.ToBase64String(dm.AnhDanhMuc);
                imageBase64 = string.Format("data:image/jpg;base64,{0}", base64Data);
            }

            return Json(new
            {
                success = true,
                data = new
                {
                    idDanhMucSanPham = dm.IdDanhMucSanPham,
                    tenDanhMucSanPham = dm.TenDanhMucSanPham,
                    moTaDanhMuc = dm.MoTaDanhMuc,
                    trangThaiDanhMuc = dm.TrangThaiDanhMuc,

                    // Format ngày tháng
                    thoiGianTaoDm = dm.ThoiGianTaoDm.ToString("dd/MM/yyyy HH:mm"),
                    thoiGianCapNhatDm = dm.ThoiGianCapNhatDm.HasValue ? dm.ThoiGianCapNhatDm.Value.ToString("dd/MM/yyyy HH:mm") : "",

                    anhBase64 = imageBase64
                }
            });
        }

        // 3. API: Xóa danh mục
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var dm = await _context.DanhMucSanPhams.FindAsync(id);
            if (dm == null) return Json(new { success = false, message = "Không tìm thấy danh mục!" });

            // Kiểm tra ràng buộc khóa ngoại (Có sản phẩm nào đang dùng danh mục này không?)
            var hasProduct = await _context.SanPhams.AnyAsync(s => s.IdDanhMucSanPham == id);
            if (hasProduct)
            {
                return Json(new { success = false, message = "Không thể xóa! Danh mục này đang chứa sản phẩm." });
            }

            _context.DanhMucSanPhams.Remove(dm);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đã xóa thành công!" });
        }

        // 4. API: Lưu danh mục (Thêm mới / Cập nhật)
        [HttpPost]
        public async Task<IActionResult> Save(DanhMucSanPham model, IFormFile? ImageFile)
        {
            // Kiểm tra xem ID đã tồn tại chưa
            var existingCategory = await _context.DanhMucSanPhams.AsNoTracking()
                                         .FirstOrDefaultAsync(x => x.IdDanhMucSanPham == model.IdDanhMucSanPham);

            try
            {
                // Xử lý file ảnh
                byte[]? imageBytes = null;
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await ImageFile.CopyToAsync(memoryStream);
                        imageBytes = memoryStream.ToArray();
                    }
                }

                if (existingCategory == null)
                {
                    // === THÊM MỚI ===
                    model.ThoiGianTaoDm = DateTime.Now;
                    model.ThoiGianCapNhatDm = DateTime.Now;
                    if (imageBytes != null) model.AnhDanhMuc = imageBytes;

                    _context.Add(model);
                }
                else
                {
                    // === CẬP NHẬT ===
                    var categoryToUpdate = await _context.DanhMucSanPhams.FindAsync(model.IdDanhMucSanPham);

                    categoryToUpdate.TenDanhMucSanPham = model.TenDanhMucSanPham;
                    categoryToUpdate.MoTaDanhMuc = model.MoTaDanhMuc;
                    categoryToUpdate.TrangThaiDanhMuc = model.TrangThaiDanhMuc;
                    categoryToUpdate.ThoiGianCapNhatDm = DateTime.Now;

                    if (imageBytes != null)
                    {
                        categoryToUpdate.AnhDanhMuc = imageBytes;
                    }

                    _context.Update(categoryToUpdate);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Lưu thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}