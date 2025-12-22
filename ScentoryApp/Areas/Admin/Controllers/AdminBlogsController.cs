using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Cần cho SelectList
using Microsoft.EntityFrameworkCore;
using ScentoryApp.Models;

namespace ScentoryApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminBlogsController : Controller
    {
        private readonly ScentoryPtudContext _context;

        public AdminBlogsController(ScentoryPtudContext context)
        {
            _context = context;
        }

        // 1. GET: Trang danh sách
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách blog, sắp xếp mới nhất lên đầu
            var listBlogs = await _context.Blogs
                                  .OrderByDescending(b => b.ThoiGianTaoBlog)
                                  .ToListAsync();

            // Nếu bạn có bảng Danh mục Blog riêng thì lấy ra đây để đổ vào dropdown
            // Ví dụ: ViewData["DanhMuc"] = new SelectList(_context.Categories, "Id", "Name");
            // Ở đây mình giả định DanhMucBlog là string nhập tay hoặc select cứng

            return View(listBlogs);
        }

        // 2. API: Lấy chi tiết 1 Blog
        [HttpGet]
        public async Task<IActionResult> GetById(string id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null) return Json(new { success = false, message = "Không tìm thấy bài viết!" });

            // Xử lý ảnh
            string imageBase64 = null;
            if (blog.AnhBlog != null && blog.AnhBlog.Length > 0)
            {
                string base64Data = Convert.ToBase64String(blog.AnhBlog);
                imageBase64 = string.Format("data:image/jpg;base64,{0}", base64Data);
            }

            return Json(new
            {
                success = true,
                data = new
                {
                    idBlog = blog.IdBlog,
                    tenBlog = blog.TenBlog,
                    danhMucBlog = blog.DanhMucBlog,
                    noiDungNgan = blog.NoiDungNgan,
                    noiDung = blog.NoiDung,
                    tag = blog.Tag,
                    trangThai = blog.TrangThai, // 1: Hiện, 0: Ẩn
                    alias = blog.Alias,
                    metaKey = blog.MetaKey,
                    metaDesc = blog.MetaDesc,
                    views = blog.Views,
                    thoiGianTaoBlog = blog.ThoiGianTaoBlog.ToString("dd/MM/yyyy HH:mm"),
                    thoiGianCapNhatBlog = blog.ThoiGianCapNhatBlog.HasValue ? blog.ThoiGianCapNhatBlog.Value.ToString("dd/MM/yyyy HH:mm") : "",
                    anhBase64 = imageBase64
                }
            });
        }

        // 3. API: Lưu (Thêm / Sửa)
        [HttpPost]
        public async Task<IActionResult> Save(Blog model, IFormFile? ImageFile)
        {
            var existingBlog = await _context.Blogs.AsNoTracking()
                                     .FirstOrDefaultAsync(x => x.IdBlog == model.IdBlog);

            try
            {
                // Xử lý ảnh upload
                byte[]? imageBytes = null;
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await ImageFile.CopyToAsync(memoryStream);
                        imageBytes = memoryStream.ToArray();
                    }
                }

                // Auto-generate Alias nếu trống (Slug từ tiêu đề) - Giả lập đơn giản
                if (string.IsNullOrEmpty(model.Alias))
                {
                    model.Alias = model.TenBlog.ToLower().Replace(" ", "-");
                }

                if (existingBlog == null)
                {
                    // === THÊM MỚI ===
                    if (string.IsNullOrEmpty(model.IdBlog))
                    {
                        model.IdBlog = await GenerateBlogId();
                    }

                    model.ThoiGianTaoBlog = DateTime.Now;
                    model.ThoiGianCapNhatBlog = DateTime.Now;
                    model.Views = 0; // Mặc định view = 0
                    if (imageBytes != null) model.AnhBlog = imageBytes;

                    _context.Add(model);
                }
                else
                {
                    // === CẬP NHẬT ===
                    var blogToUpdate = await _context.Blogs.FindAsync(model.IdBlog);

                    blogToUpdate.TenBlog = model.TenBlog;
                    blogToUpdate.DanhMucBlog = model.DanhMucBlog;
                    blogToUpdate.NoiDungNgan = model.NoiDungNgan;
                    blogToUpdate.NoiDung = model.NoiDung;
                    blogToUpdate.Tag = model.Tag;
                    blogToUpdate.TrangThai = model.TrangThai;
                    blogToUpdate.Alias = model.Alias;
                    blogToUpdate.MetaKey = model.MetaKey;
                    blogToUpdate.MetaDesc = model.MetaDesc;
                    blogToUpdate.ThoiGianCapNhatBlog = DateTime.Now;

                    if (imageBytes != null) blogToUpdate.AnhBlog = imageBytes;

                    _context.Update(blogToUpdate);
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
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null) return Json(new { success = false, message = "Không tìm thấy bài viết!" });

            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đã xóa thành công!" });
        }
        // 5. API: Upload ảnh từ CKEditor (MỚI THÊM)
        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile upload)
        {
            if (upload != null && upload.Length > 0)
            {
                var fileName = DateTime.Now.Ticks + "_" + upload.FileName;
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/blogs", fileName);

                // Tạo thư mục nếu chưa có
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await upload.CopyToAsync(stream);
                }

                var url = "/images/blogs/" + fileName;

                // Trả về JSON theo format CKEditor yêu cầu
                return Json(new { uploaded = 1, fileName = fileName, url = url });
            }
            return Json(new { uploaded = 0, error = new { message = "Lỗi tải ảnh!" } });
        }
        private async Task<string> GenerateBlogId()
        {
            var existingIds = await _context.Blogs
                .Where(s => s.IdBlog.StartsWith("BL"))
                .Select(s => s.IdBlog)
                .ToListAsync();

            int max = 0;
            foreach (var id in existingIds)
            {
                if (id.Length > 2 && int.TryParse(id.Substring(2), out int n))
                {
                    if (n > max) max = n;
                }
            }
            return "BL" + (max + 1).ToString("D3");
        }

        [HttpGet]
        public async Task<IActionResult> GetNextId()
        {
            try
            {
                var nextId = await GenerateBlogId();
                return Json(new { success = true, data = nextId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}