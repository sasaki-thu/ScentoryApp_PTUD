using ScentoryApp.Models;

namespace ScentoryApp.Areas.Admin.Models
{
    public class DashboardViewModel
    {
        // 1. Thẻ chỉ số tổng quan (KPIs)
        public decimal DoanhThuThangNay { get; set; }
        public decimal DoanhThuHomNay { get; set; }
        public int DonHangMoi { get; set; } // Đơn chờ xử lý
        public int TongKhachHang { get; set; }

        // 2. Dữ liệu biểu đồ (Chart Data)
        public List<string> LabelsBieuDo { get; set; } = new List<string>();
        public List<decimal> DataBieuDoDoanhThu { get; set; } = new List<decimal>();
        public List<int> DataBieuDoDonHang { get; set; } = new List<int>();

        // 3. Thống kê tỷ trọng (Pie Chart)
        public List<ChartData> TyLeTrangThaiDonHang { get; set; } = new List<ChartData>();

        // 4. Danh sách hiển thị
        public List<DonHang> DonHangGanDay { get; set; } = new List<DonHang>();
        public List<TopProduct> SanPhamBanChay { get; set; } = new List<TopProduct>();
        public DateTime StartDateSelected { get; set; }
    }

    public class ChartData
    {
        public string Label { get; set; }
        public int Value { get; set; }
    }

    public class TopProduct
    {
        public string TenSanPham { get; set; }
        public string HinhAnh { get; set; }
        public int SoLuongBan { get; set; }
        public decimal TongTien { get; set; }
    }
}