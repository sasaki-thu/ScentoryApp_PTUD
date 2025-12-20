using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScentoryApp.Areas.Admin.Models;
using ScentoryApp.Models;
using System.Globalization;

namespace ScentoryApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ScentoryPtudContext _context;

        public HomeController(ScentoryPtudContext context)
        {
            _context = context;
        }

        public IActionResult Index(string date = null)
        {
            var model = new DashboardViewModel();
            var today = DateTime.Today;
            DateTime startDate;

            // 1. XỬ LÝ NGÀY BẮT ĐẦU (Tìm ngày Thứ 2 đầu tuần)
            if (string.IsNullOrEmpty(date))
            {
                // Nếu không chọn ngày -> Lấy tuần hiện tại
                // Công thức tìm ngày Thứ 2 của tuần này:
                int diff = today.DayOfWeek - DayOfWeek.Monday;
                if (diff < 0) diff += 7; // Nếu là Chủ Nhật thì lùi về Thứ 2 tuần trước
                startDate = today.AddDays(-diff).Date;
            }
            else
            {
                // Nếu chọn ngày (bấm nút) -> Parse ngày đó ra
                if (!DateTime.TryParse(date, out startDate))
                {
                    startDate = today; // Fallback nếu lỗi
                }
            }

            // Lưu lại để View dùng làm nút Next/Prev
            model.StartDateSelected = startDate;
            DateTime endDate = startDate.AddDays(6); // Chủ Nhật

            // --- KPI TỔNG QUAN (Giữ nguyên tính theo thời điểm thực tế) ---
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            model.DoanhThuThangNay = _context.DonHangs.Where(d => d.ThoiGianDatHang >= firstDayOfMonth && d.TinhTrangDonHang != "Đã hủy").Sum(d => d.TongTienDonHang);
            model.DoanhThuHomNay = _context.DonHangs.Where(d => d.ThoiGianDatHang.Date == today && d.TinhTrangDonHang != "Đã hủy").Sum(d => d.TongTienDonHang);
            model.DonHangMoi = _context.DonHangs.Count(d => d.TinhTrangDonHang == "Đang chuẩn bị hàng" || d.TinhTrangDonHang == "Đang chờ xử lý");
            model.TongKhachHang = _context.KhachHangs.Count();

            // --- 2. XỬ LÝ BIỂU ĐỒ THEO TUẦN ---
            var rawData = _context.DonHangs
                .Where(d => d.ThoiGianDatHang >= startDate
                         && d.ThoiGianDatHang <= endDate.AddDays(1) // +1 để lấy hết ngày cuối
                         && d.TinhTrangDonHang != "Đã hủy")
                .Select(d => new { d.ThoiGianDatHang, d.TongTienDonHang })z
                .ToList();

            model.LabelsBieuDo = new List<string>();
            model.DataBieuDoDoanhThu = new List<decimal>();
            model.DataBieuDoDonHang = new List<int>();

            // Chạy từ Thứ 2 (0) đến CN (6)
            for (int i = 0; i <= 6; i++)
            {
                var currentDay = startDate.AddDays(i);

                // Label hiển thị: T2 (16/12)
                string dayName = currentDay.DayOfWeek == DayOfWeek.Sunday ? "CN" : "T" + ((int)currentDay.DayOfWeek + 1);
                model.LabelsBieuDo.Add($"{dayName} ({currentDay:dd/MM})");

                var dailyData = rawData.Where(x => x.ThoiGianDatHang.Date == currentDay).ToList();

                model.DataBieuDoDoanhThu.Add(dailyData.Sum(x => x.TongTienDonHang));
                model.DataBieuDoDonHang.Add(dailyData.Count);
            }

            // --- CÁC PHẦN DƯỚI GIỮ NGUYÊN ---
            model.TyLeTrangThaiDonHang = _context.DonHangs.GroupBy(d => d.TinhTrangDonHang).Select(g => new ChartData { Label = g.Key, Value = g.Count() }).ToList();

            model.SanPhamBanChay = _context.ChiTietDonHangs
                .Include(ct => ct.IdSanPhamNavigation)
                .GroupBy(ct => new { ct.IdSanPham, ct.IdSanPhamNavigation.TenSanPham, ct.IdSanPhamNavigation.AnhSanPham })
                .Select(g => new TopProduct
                {
                    TenSanPham = g.Key.TenSanPham,
                    HinhAnh = g.Key.AnhSanPham != null ? Convert.ToBase64String(g.Key.AnhSanPham) : null,
                    SoLuongBan = g.Sum(x => x.SoLuong),
                    TongTien = g.Sum(x => x.ThanhTien)
                }).OrderByDescending(p => p.SoLuongBan).Take(5).ToList();

            model.DonHangGanDay = _context.DonHangs.Include(d => d.IdKhachHangNavigation).OrderByDescending(d => d.ThoiGianDatHang).Take(6).ToList();

            return View(model);
        }
    }
}