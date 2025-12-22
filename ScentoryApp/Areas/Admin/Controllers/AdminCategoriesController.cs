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
                    if (string.IsNullOrEmpty(model.IdDanhMucSanPham))
                    {
                        model.IdDanhMucSanPham = await GenerateDanhMucId();
                    }
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
        private async Task<string> GenerateDanhMucId()
        {
            var existingIds = await _context.DanhMucSanPhams
                .Where(s => s.IdDanhMucSanPham.StartsWith("DM"))
                .Select(s => s.IdDanhMucSanPham)
                .ToListAsync();

            int max = 0;
            foreach (var id in existingIds)
            {
                if (id.Length > 2 && int.TryParse(id.Substring(2), out int n))
                {
                    if (n > max) max = n;
                }
            }
            return "DM" + (max + 1).ToString("D3");
        }

        [HttpGet]
        public async Task<IActionResult> GetNextId()
        {
            try
            {
                var nextId = await GenerateDanhMucId();
                return Json(new { success = true, data = nextId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}