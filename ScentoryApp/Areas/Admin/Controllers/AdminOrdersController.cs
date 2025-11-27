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
    public class AdminOrdersController : Controller
    {
        private readonly ScentoryPtudContext _context;

        public AdminOrdersController(ScentoryPtudContext context)
        {
            _context = context;
        }

        // 1. GET: Trang danh sách
        public async Task<IActionResult> Index()
        {
            var orders = await _context.DonHangs
                                       .Include(d => d.IdKhachHangNavigation) // Lấy tên khách
                                       .Include(d => d.IdDonViVanChuyenNavigation) // Lấy tên Shipper
                                       .OrderByDescending(d => d.ThoiGianDatHang) // Đơn mới nhất lên đầu
                                       .ToListAsync();
            return View(orders);
        }

        // 2. API: Lấy chi tiết đơn hàng
        [HttpGet]
        public async Task<IActionResult> GetById(string id)
        {
            var order = await _context.DonHangs
                                      .Include(d => d.IdKhachHangNavigation)
                                      .FirstOrDefaultAsync(x => x.IdDonHang == id);

            if (order == null) return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });

            return Json(new
            {
                success = true,
                data = new
                {
                    idDonHang = order.IdDonHang,
                    khachHang = order.IdKhachHang + " - " + order.IdKhachHangNavigation.HoTen, // Hiển thị cả Mã và Tên

                    // Format ngày tháng chuẩn ISO cho input datetime-local
                    thoiGianDatHang = order.ThoiGianDatHang.ToString("dd/MM/yyyy HH:mm:ss"),
                    ngayGiaoHangDuKien = order.NgayGiaoHangDuKien.ToString("yyyy-MM-dd"), // DateOnly dùng format này

                    idDonViVanChuyen = order.IdDonViVanChuyen,

                    tongTien = order.TongTienDonHang,
                    phiShip = order.PhiVanChuyen,
                    thue = order.ThueBanHang,

                    trangThai = order.TinhTrangDonHang,
                    maGiamGia = order.IdMaGiamGia,

                    // Lấy địa chỉ giao hàng từ bảng Khách hàng (hoặc từ bảng Đơn hàng nếu có cột địa chỉ riêng)
                    // Ở đây mình lấy tạm từ bảng Khách hàng vì Model DonHang của bạn k thấy cột DiaChiGiaoHang
                    diaChiGiao = order.IdKhachHangNavigation.DiaChi,

                    thoiGianHoanTat = order.ThoiGianHoanTatDonHang.HasValue ? order.ThoiGianHoanTatDonHang.Value.ToString("dd/MM/yyyy HH:mm") : ""
                }
            });
        }

        // 3. API: Cập nhật Đơn hàng (Chủ yếu sửa Trạng thái, Ngày giao)
        [HttpPost]
        public async Task<IActionResult> Update(string IdDonHang, string TinhTrangDonHang, DateOnly NgayGiaoHangDuKien)
        {
            var order = await _context.DonHangs.FindAsync(IdDonHang);
            if (order == null) return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });

            try
            {
                order.TinhTrangDonHang = TinhTrangDonHang;
                order.NgayGiaoHangDuKien = NgayGiaoHangDuKien;
                order.ThoiGianCapNhat = DateTime.Now;

                // Nếu trạng thái là "Đã giao" hoặc "Hoàn tất" -> Cập nhật thời gian hoàn tất
                if (TinhTrangDonHang == "Đã giao" || TinhTrangDonHang == "Hoàn tất")
                {
                    if (order.ThoiGianHoanTatDonHang == null)
                        order.ThoiGianHoanTatDonHang = DateTime.Now;
                }

                _context.Update(order);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // 4. API: Hủy đơn hàng (Xóa)
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var order = await _context.DonHangs.FindAsync(id);
            if (order == null) return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });

            // Lưu ý: Thường đơn hàng không nên xóa hẳn (Hard Delete) mà chỉ chuyển trạng thái "Đã hủy" (Soft Delete)
            // Nhưng nếu bạn muốn xóa thật thì dùng code này:

            // Cần xóa chi tiết đơn hàng trước (nếu có) để tránh lỗi khóa ngoại
            var details = await _context.ChiTietDonHangs.Where(c => c.IdDonHang == id).ToListAsync();
            _context.ChiTietDonHangs.RemoveRange(details);

            _context.DonHangs.Remove(order);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xóa đơn hàng thành công!" });
        }
    }
}