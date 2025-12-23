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
                                      .Include(d => d.ChiTietDonHangs)
                                          .ThenInclude(ct => ct.IdSanPhamNavigation)
                                      .FirstOrDefaultAsync(x => x.IdDonHang == id);

            if (order == null) return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });

            decimal tongTienHang = order.ChiTietDonHangs.Sum(ct => ct.ThanhTien);
            decimal giamGia = (tongTienHang + order.PhiVanChuyen + order.ThueBanHang) - order.TongTienDonHang;

            if (giamGia < 0) giamGia = 0;
            var listProducts = order.ChiTietDonHangs.Select(ct => new
            {
                tenSanPham = ct.IdSanPhamNavigation.TenSanPham,
                anhSanPham = ct.IdSanPhamNavigation.AnhSanPham != null
                             ? "data:image/jpg;base64," + Convert.ToBase64String(ct.IdSanPhamNavigation.AnhSanPham)
                             : null,
                soLuong = ct.SoLuong,
                donGia = ct.DonGia,
                thanhTien = ct.ThanhTien
            }).ToList();

            return Json(new
            {
                success = true,
                data = new
                {
                    idDonHang = order.IdDonHang,
                    khachHang = order.IdKhachHang + " - " + order.IdKhachHangNavigation.HoTen,
                    thoiGianDatHang = order.ThoiGianDatHang.ToString("dd/MM/yyyy HH:mm:ss"),
                    ngayGiaoHangDuKien = order.NgayGiaoHangDuKien.ToString("yyyy-MM-dd"),

                    idDonViVanChuyen = order.IdDonViVanChuyen,
                    trangThai = order.TinhTrangDonHang,
                    maGiamGia = order.IdMaGiamGia ?? "",

                    diaChiGiao = !string.IsNullOrEmpty(order.DiaChiNhanHang) ? order.DiaChiNhanHang : order.IdKhachHangNavigation.DiaChi,
                    thoiGianHoanTat = order.ThoiGianHoanTatDonHang.HasValue ? order.ThoiGianHoanTatDonHang.Value.ToString("dd/MM/yyyy HH:mm") : "",
                    tongTienHang = tongTienHang,
                    phiShip = order.PhiVanChuyen,
                    giamGia = giamGia,
                    tongCong = order.TongTienDonHang,
                    chiTietSanPham = listProducts
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
                string[] validStatus = { "Đang chuẩn bị hàng", "Đang giao", "Đã giao", "Đã hủy" };
                if (!validStatus.Contains(TinhTrangDonHang))
                {
                    return Json(new { success = false, message = "Trạng thái không hợp lệ!" });
                }

                order.TinhTrangDonHang = TinhTrangDonHang;

                if (TinhTrangDonHang != "Đã hủy")
                {
                    order.NgayGiaoHangDuKien = NgayGiaoHangDuKien;
                }

                order.ThoiGianCapNhat = DateTime.Now;

                if (TinhTrangDonHang == "Đã giao")
                {
                    if (order.ThoiGianHoanTatDonHang == null)
                        order.ThoiGianHoanTatDonHang = DateTime.Now;
                }
                else
                {
                    order.ThoiGianHoanTatDonHang = null;
                }

                _context.Update(order);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật thành công!" });
            }
            catch (DbUpdateException dbEx)
            {
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                return Json(new { success = false, message = "Lỗi Database: " + innerMessage });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // 4. API: Xóa Vĩnh Viễn
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var order = await _context.DonHangs.FindAsync(id);
            if (order == null) return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });

            try
            {
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