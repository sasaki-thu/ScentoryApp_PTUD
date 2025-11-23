using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ScentoryApp.Models;

namespace ScentoryApp.Areas_Admin_Controllers
{
    [Area("Admin")]
    public class AdminProductsController : Controller
    {
        private readonly ScentoryPtudContext _context;

        public AdminProductsController(ScentoryPtudContext context)
        {
            _context = context;
        }

        // GET: AdminProducts (Chỉ hiển thị giao diện và danh sách ban đầu)
        public async Task<IActionResult> Index()
        {
            var listSanPham = await _context.SanPhams
                                    .Include(s => s.IdDanhMucSanPhamNavigation)
                                    .OrderByDescending(s => s.ThoiGianTaoSp)
                                    .ToListAsync();
            ViewData["IdDanhMucSanPham"] = new SelectList(_context.DanhMucSanPhams, "IdDanhMucSanPham", "TenDanhMucSanPham");
            return View(listSanPham);
        }

        // API: Lấy chi tiết 1 sản phẩm (Cho nút Edit / Details)
        [HttpGet]
        public async Task<IActionResult> GetById(string id)
        {
            var sp = await _context.SanPhams.FindAsync(id);
            if (sp == null) return Json(new { success = false, message = "Không tìm thấy sản phẩm" });

            // Chuyển đổi ảnh byte[] sang Base64 string để hiển thị ở client
            string imageBase64 = null;
            if (sp.AnhSanPham != null && sp.AnhSanPham.Length > 0)
            {
                string base64Data = Convert.ToBase64String(sp.AnhSanPham);
                imageBase64 = string.Format("data:image/jpg;base64,{0}", base64Data);
            }

            // Trả về đối tượng Anonymous để tránh lỗi vòng lặp JSON (Circular Reference)
            return Json(new
            {
                success = true,
                data = new
                {
                    idSanPham = sp.IdSanPham,
                    tenSanPham = sp.TenSanPham,
                    moTaSanPham = sp.MoTaSanPham,
                    giaNiemYet = sp.GiaNiemYet,
                    soLuongTonKho = sp.SoLuongTonKho,
                    trangThaiSp = sp.TrangThaiSp,
                    idDanhMucSanPham = sp.IdDanhMucSanPham,
                    anhBase64 = imageBase64, // Trả về chuỗi ảnh để hiện preview
                    thoiGianTaoSp = sp.ThoiGianTaoSp.ToString("dd/MM/yyyy HH:mm"),
                    thoiGianCapNhat = sp.ThoiGianCapNhat.HasValue ? sp.ThoiGianCapNhat.Value.ToString("dd/MM/yyyy HH:mm") : "Chưa cập nhật"
                }
            });
        }

        // API: Xóa sản phẩm
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var sp = await _context.SanPhams.FindAsync(id);
            if (sp != null)
            {
                _context.SanPhams.Remove(sp);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đã xóa thành công!" });
            }
            return Json(new { success = false, message = "Lỗi: Không tìm thấy sản phẩm!" });
        }

        // API: Lưu sản phẩm (Xử lý cả Thêm Mới và Cập Nhật)
        [HttpPost]
        public async Task<IActionResult> Save(SanPham model, IFormFile? ImageFile)
        {
            // Vì ID là string người dùng nhập, ta kiểm tra xem ID đã có chưa để biết là Thêm hay Sửa
            // Lưu ý: Logic này giả định người dùng không sửa ID khi bấm Edit (Input ID set readonly ở View)
            var existingProduct = await _context.SanPhams.AsNoTracking().FirstOrDefaultAsync(x => x.IdSanPham == model.IdSanPham);

            try
            {
                // Xử lý file ảnh: Chuyển IFormFile -> byte[]
                byte[]? imageBytes = null;
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await ImageFile.CopyToAsync(memoryStream);
                        imageBytes = memoryStream.ToArray();
                    }
                }

                if (existingProduct == null)
                {
                    // === THÊM MỚI ===
                    model.ThoiGianTaoSp = DateTime.Now;
                    model.ThoiGianCapNhat = DateTime.Now;
                    if (imageBytes != null) model.AnhSanPham = imageBytes;

                    _context.Add(model);
                }
                else
                {
                    // === CẬP NHẬT ===
                    // Phải load lại thực thể để update (để tránh lỗi tracking)
                    var productToUpdate = await _context.SanPhams.FindAsync(model.IdSanPham);

                    productToUpdate.TenSanPham = model.TenSanPham;
                    productToUpdate.MoTaSanPham = model.MoTaSanPham;
                    productToUpdate.GiaNiemYet = model.GiaNiemYet;
                    productToUpdate.SoLuongTonKho = model.SoLuongTonKho;
                    productToUpdate.TrangThaiSp = model.TrangThaiSp;
                    productToUpdate.IdDanhMucSanPham = model.IdDanhMucSanPham;
                    productToUpdate.ThoiGianCapNhat = DateTime.Now;

                    // Chỉ cập nhật ảnh nếu người dùng có chọn file mới
                    if (imageBytes != null)
                    {
                        productToUpdate.AnhSanPham = imageBytes;
                    }

                    _context.Update(productToUpdate);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Lưu dữ liệu thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}