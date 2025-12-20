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
                                       .Include(d => d.IdKhachHangNavigation)
                                       .Include(d => d.IdDonViVanChuyenNavigation)
                                       .OrderByDescending(d => d.ThoiGianDatHang)
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
                    khachHang = order.IdKhachHang + " - " + order.IdKhachHangNavigation.HoTen,

                    // Format ngày tháng cho input datetime-local hoặc hiển thị
                    thoiGianDatHang = order.ThoiGianDatHang.ToString("dd/MM/yyyy HH:mm:ss"),

                    // DateOnly convert sang string yyyy-MM-dd để input date hiểu
                    ngayGiaoHangDuKien = order.NgayGiaoHangDuKien.ToString("yyyy-MM-dd"),

                    idDonViVanChuyen = order.IdDonViVanChuyen,
                    tongTien = order.TongTienDonHang,
                    phiShip = order.PhiVanChuyen,
                    thue = order.ThueBanHang,
                    trangThai = order.TinhTrangDonHang,
                    maGiamGia = order.IdMaGiamGia,

                    // Lấy địa chỉ: Ưu tiên địa chỉ đơn hàng, nếu ko có thì lấy địa chỉ khách
                    diaChiGiao = !string.IsNullOrEmpty(order.DiaChiNhanHang) ? order.DiaChiNhanHang : order.IdKhachHangNavigation.DiaChi,

                    thoiGianHoanTat = order.ThoiGianHoanTatDonHang.HasValue ? order.ThoiGianHoanTatDonHang.Value.ToString("dd/MM/yyyy HH:mm") : ""
                }
            });
        }

        // 3. API: Cập nhật Đơn hàng
        [HttpPost]
        public async Task<IActionResult> Update(string IdDonHang, string TinhTrangDonHang, DateOnly NgayGiaoHangDuKien)
        {
            var order = await _context.DonHangs.FindAsync(IdDonHang);
            if (order == null) return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });

            try
            {
                // 1. Kiểm tra trạng thái hợp lệ
                string[] validStatus = { "Đang chuẩn bị hàng", "Đang giao", "Đã giao", "Đã hủy" };
                if (!validStatus.Contains(TinhTrangDonHang))
                {
                    return Json(new { success = false, message = "Trạng thái không hợp lệ!" });
                }

                // 2. Cập nhật thông tin
                order.TinhTrangDonHang = TinhTrangDonHang;

                // Chỉ cập nhật ngày giao dự kiến nếu đơn KHÔNG PHẢI là "Đã hủy"
                if (TinhTrangDonHang != "Đã hủy")
                {
                    order.NgayGiaoHangDuKien = NgayGiaoHangDuKien;
                }

                order.ThoiGianCapNhat = DateTime.Now;

                // 3. Logic Hoàn tất đơn hàng
                if (TinhTrangDonHang == "Đã giao")
                {
                    // Nếu chuyển sang Đã giao -> Cập nhật thời gian hoàn tất
                    if (order.ThoiGianHoanTatDonHang == null)
                        order.ThoiGianHoanTatDonHang = DateTime.Now;
                }
                else
                {
                    // Nếu chuyển sang trạng thái khác (kể cả Đã hủy) -> Xóa thời gian hoàn tất
                    order.ThoiGianHoanTatDonHang = null;
                }

                _context.Update(order);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật thành công!" });
            }
            catch (DbUpdateException dbEx)
            {
                // BẮT LỖI SQL/TRIGGER CỤ THỂ
                // Lấy lỗi bên trong cùng (InnerException) để biết Trigger nào hoặc Constraint nào chặn
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                return Json(new { success = false, message = "Lỗi Database: " + innerMessage });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // 4. API: Xóa Vĩnh Viễn (Dùng cho nút Xóa Đơn)
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var order = await _context.DonHangs.FindAsync(id);
            if (order == null) return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });

            try
            {
                // Xóa chi tiết trước để tránh lỗi khóa ngoại
                var details = await _context.ChiTietDonHangs.Where(c => c.IdDonHang == id).ToListAsync();
                _context.ChiTietDonHangs.RemoveRange(details);

                _context.DonHangs.Remove(order);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã xóa đơn hàng vĩnh viễn!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa: " + ex.Message });
            }
        }
    }
}