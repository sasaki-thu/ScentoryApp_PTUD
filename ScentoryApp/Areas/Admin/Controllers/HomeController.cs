using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ScentoryApp.Models;

namespace ScentoryApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            try
            {
                // Embedded data - using simple structure to avoid quote escaping
                // Try to read blog data from CSDL CSV file first, fall back to embedded list if unavailable
                var allBlogs = new List<Blog>();
                try
                {
                    var csvPath = Path.Combine(Directory.GetCurrentDirectory(), "CSDL", "CSDL - Blog.csv");
                    if (System.IO.File.Exists(csvPath))
                    {
                        var lines = System.IO.File.ReadAllLines(csvPath);
                        if (lines.Length > 1)
                        {
                            // header is first line
                            for (int i = 1; i < lines.Length; i++)
                            {
                                var line = lines[i];
                                if (string.IsNullOrWhiteSpace(line)) continue;
                                // simple CSV parse handling quoted fields
                                var fields = new List<string>();
                                bool inQuotes = false;
                                var sb = new System.Text.StringBuilder();
                                for (int c = 0; c < line.Length; c++)
                                {
                                    var ch = line[c];
                                    if (ch == '"')
                                    {
                                        if (inQuotes && c + 1 < line.Length && line[c + 1] == '"')
                                        {
                                            // escaped quote
                                            sb.Append('"');
                                            c++; // skip next
                                        }
                                        else
                                        {
                                            inQuotes = !inQuotes;
                                        }
                                    }
                                    else if (ch == ',' && !inQuotes)
                                    {
                                        fields.Add(sb.ToString());
                                        sb.Clear();
                                    }
                                    else
                                    {
                                        sb.Append(ch);
                                    }
                                }
                                fields.Add(sb.ToString());

                                if (fields.Count >= 4)
                                {
                                    allBlogs.Add(new Blog
                                    {
                                        IdBlog = fields[0],
                                        TenBlog = fields[1],
                                        NoiDung = fields[2],
                                        DanhMucBlog = fields[3],
                                        ThoiGianTaoBlog = DateTime.Now
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error reading blog CSV: " + ex.Message);
                }

                // fallback to embedded data if CSV not found / empty
                if (!allBlogs.Any())
                {
                    allBlogs = new List<Blog>
                    {
                        new Blog { IdBlog = "BL001", TenBlog = "Scentory: Khi mùi hương không chỉ là sản phẩm, mà là ký ức và câu chuyện cá nhân", NoiDung = "Có những mùi hương chỉ thoáng qua nhưng lại khắc sâu trong tâm trí, gợi về ký ức và cảm xúc tưởng chừng đã ngủ quên. Đó cũng chính là lý do Scentory ra đời - một thương hiệu nước hoa Việt Nam tiên phong mang triết lý \"Your scent - Your story\". Với Scentory, mùi hương không chỉ là sản phẩm, mà là câu chuyện cá nhân, là \"mùi của ký ức\" gắn liền với từng khoảnh khắc trong cuộc đời bạn. Xem thêm: Về chúng tôi", DanhMucBlog = "Thương hiệu", ThoiGianTaoBlog = DateTime.Now },
                        new Blog { IdBlog = "BL002", TenBlog = "Reminiscence - Bộ sưu tập nước hoa viết tiếp những kỷ niệm chưa bao giờ phai", NoiDung = "Bộ sưu tập Reminiscence của Scentory là lời mời gọi bạn bước vào thế giới hương thơm, nơi mỗi chai nước hoa không chỉ đơn thuần là mùi hương mà còn là ký ức, cảm xúc và câu chuyện riêng. Với sự kết hợp tinh tế giữa các tầng hương, Reminiscence Collection gợi mở miền ký ức, giúp bạn tìm thấy \"signature scent\" - nốt hương riêng biệt thể hiện cá tính và phong cách. Hãy cùng khám phá để thấy hương thơm có thể dẫn dắt bạn trở về những khoảnh khắc chưa bao giờ phai nhạt.", DanhMucBlog = "Sản phẩm", ThoiGianTaoBlog = DateTime.Now },
                        new Blog { IdBlog = "BL003", TenBlog = "Bí quyết xịt nước hoa đúng cách: Xịt đúng - Lưu lâu", NoiDung = "Nước hoa đã trở thành một trong những thứ không thể thiếu trong cuộc sống. Xịt nước hoa không chỉ tạo mùi hương dễ chịu mà còn là phương tiện để bạn thể hiện cá tính theo câu chuyện của riêng mình. Tuy nhiên, không phải ai cũng biết cách xịt nước hoa sao cho vừa đúng vừa lưu hương lâu. Chỉ cần một vài bí quyết nhỏ là bạn đã có thể tự tin với mùi hương lan tỏa tự nhiên, lưu giữ hương thơm suốt một ngày dài. Hãy cùng tìm hiểu bí quyết xịt nước hoa đúng cách cùng Scentory nhé!", DanhMucBlog = "HDSD", ThoiGianTaoBlog = DateTime.Now },
                        new Blog { IdBlog = "BL004", TenBlog = "Giải mã tầng hương: Tại sao Top - Heart - Base notes có thể tạo nên cả một hành trình cảm xúc?", NoiDung = "Khi nhắc đến nước hoa, người ta thường nghĩ ngay đến mùi hương dễ chịu hay sức hút quyến rũ. Nhưng ít ai biết rằng, đằng sau mỗi chai nước hoa lại ẩn chứa cả một cấu trúc tinh tế mang tên tầng hương nước hoa. Vậy các tầng hương có ý nghĩa như thế nào? Hãy cùng Scentory khám phá qua bài viết dưới đây nhé.", DanhMucBlog = "Kiến thức", ThoiGianTaoBlog = DateTime.Now },
                        new Blog { IdBlog = "BL005", TenBlog = "Tặng mùi hương như thế nào để chạm đến trái tim người nhận?", NoiDung = "Bạn có bao giờ nhận được một món quà mà chỉ cần thoang thoáng mùi hương là nhớ ngay tới người tặng? Scentory tin rằng mùi hương không chỉ là món quà, mà còn là \"chìa khóa\" mở ra ký ức và kết nối trái tim. Việc chọn và tặng một lọ nước hoa hay một chai tinh dầu phù hợp có thể biến khoảnh khắc bình thường thành kỷ niệm khó quên bởi hương thơm có khả năng khơi gợi cảm xúc và ký ức mạnh mẽ. Vậy làm sao để tặng mùi hương một cách tinh tế, vừa ý nghĩa mà vừa \"chạm đến trái tim\" người nhận để mọi khoảnh khắc đều có thể trở nên thật đặc biệt? Khám phá ngay bộ sưu tập nước hoa Reminiscence tại Cửa hàng Scentory. Khám phá ngay bộ sưu tập tinh dầu Seviora tại Cửa hàng Scentory", DanhMucBlog = "Mẹo", ThoiGianTaoBlog = DateTime.Now },
                        new Blog { IdBlog = "BL006", TenBlog = "Whisper - Thầm: Lời thì thầm của hoa hồng và xạ hương dành cho tâm hồn mơ mộng", NoiDung = "Whisper - Thầm là sản phẩm nước hoa đầu tiên trong bộ sưu tập Reminiscence của Scentory, mở ra hành trình khám phá hương thơm mang nhiều cảm xúc và vẻ đẹp tinh tế. Nhẹ nhàng như lời thì thầm của trái tim, hương thơm kết hợp giữa lê xanh, hoa hồng và xạ hương trắng mang đến trải nghiệm vừa trong trẻo vừa lãng mạn. Nếu bạn đang tìm kiếm một mùi hương nâng niu tâm hồn mộng mơ, hãy để Whisper - Thầm trở thành người bạn đồng hành mỗi ngày nhé - khám phá sản phẩm ngay tại đây.", DanhMucBlog = "Storytelling", ThoiGianTaoBlog = DateTime.Now }
                    };
                }

                var allProductCategories = new List<DanhMucSanPham>
                {
                    new DanhMucSanPham { IdDanhMucSanPham = "DM001", TenDanhMucSanPham = "Nước hoa" },
                    new DanhMucSanPham { IdDanhMucSanPham = "DM002", TenDanhMucSanPham = "Tinh dầu" },
                    new DanhMucSanPham { IdDanhMucSanPham = "DM003", TenDanhMucSanPham = "Box quà" }
                };

                var productCategoryLookup = allProductCategories.ToDictionary(pc => pc.IdDanhMucSanPham, pc => pc.TenDanhMucSanPham);

                var allCustomers = new List<KhachHang>
                {
                    new KhachHang { IdKhachHang = "ND004", HoTen = "Đào Ngọc Huỳnh Anh", Email = "anhdao@st.ueh.edu.vn", GioiTinh = "Nữ", NgaySinh = DateOnly.FromDateTime(DateTime.ParseExact("24/12/2005", "dd/MM/yyyy", CultureInfo.InvariantCulture)) },
                    new KhachHang { IdKhachHang = "ND005", HoTen = "Nguyễn Thị Ngọc Bích", Email = "bichnguyen@st.ueh.edu.vn", Sdt = "0847450515", DiaChi = "543 Lê Duẩn phường Buôn Ma Thuột Đắk Lắk", GioiTinh = "Nữ", NgaySinh = DateOnly.FromDateTime(DateTime.ParseExact("05/05/2005", "dd/MM/yyyy", CultureInfo.InvariantCulture)) },
                    new KhachHang { IdKhachHang = "ND006", HoTen = "Trần Lê Huy", Email = "huytran@st.ueh.edu.vn", Sdt = "0766710957", DiaChi = "Tạ Quang Bửu phường Bình Đông TP.HCM", NgaySinh = DateOnly.FromDateTime(DateTime.ParseExact("23/02/2005", "dd/MM/yyyy", CultureInfo.InvariantCulture)) },
                    new KhachHang { IdKhachHang = "ND007", HoTen = "Trịnh Thị Thảo Tâm", Email = "tamtrinh@st.ueh.edu.vn", Sdt = "0941960559", DiaChi = "Triệu Phong Quảng Trị", GioiTinh = "Nữ", NgaySinh = DateOnly.FromDateTime(DateTime.ParseExact("17/07/2005", "dd/MM/yyyy", CultureInfo.InvariantCulture)) },
                    new KhachHang { IdKhachHang = "ND008", HoTen = "Diệp Anh Thu", Email = "thudiep@st.ueh.edu.vn", Sdt = "0905865092", DiaChi = "43-45 Nguyễn Chi Thanh phường An Đông TP.HCM", GioiTinh = "Nữ", NgaySinh = DateOnly.FromDateTime(DateTime.ParseExact("19/04/2005", "dd/MM/yyyy", CultureInfo.InvariantCulture)) },
                    new KhachHang { IdKhachHang = "ND009", HoTen = "Lê Lộc Phúc Tiên", Email = "tienle@st.ueh.edu.vn", Sdt = "0835451450", DiaChi = "124/24 Nguyễn Văn Cư phường Nguyễn Cư Trinh Quận 1 TP.HCM", GioiTinh = "Nữ", NgaySinh = DateOnly.FromDateTime(DateTime.ParseExact("08/05/2005", "dd/MM/yyyy", CultureInfo.InvariantCulture)) }
                };

                var customerLookup = allCustomers.ToDictionary(c => c.IdKhachHang, c => c);

                var allOrders = new List<DonHang>
                {
                    new DonHang { IdDonHang = "DH001", IdKhachHang = "ND004", IdKhachHangNavigation = customerLookup["ND004"], ThoiGianDatHang = DateTime.ParseExact("2025-09-02 14:20:10", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), TongTienDonHang = 649000, TinhTrangDonHang = "Đã giao" },
                    new DonHang { IdDonHang = "DH002", IdKhachHang = "ND005", IdKhachHangNavigation = customerLookup["ND005"], ThoiGianDatHang = DateTime.ParseExact("2025-09-06 02:30:02", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), TongTienDonHang = 649000, TinhTrangDonHang = "Đã giao" },
                    new DonHang { IdDonHang = "DH003", IdKhachHang = "ND006", IdKhachHangNavigation = customerLookup["ND006"], ThoiGianDatHang = DateTime.ParseExact("2025-09-07 22:16:14", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), TongTienDonHang = 369000, TinhTrangDonHang = "Đã giao" },
                    new DonHang { IdDonHang = "DH004", IdKhachHang = "ND007", IdKhachHangNavigation = customerLookup["ND007"], ThoiGianDatHang = DateTime.ParseExact("2025-09-30 08:00:13", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), TongTienDonHang = 634000, TinhTrangDonHang = "Đã giao" },
                    new DonHang { IdDonHang = "DH005", IdKhachHang = "ND008", IdKhachHangNavigation = customerLookup["ND008"], ThoiGianDatHang = DateTime.ParseExact("2025-11-13 12:27:18", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), TongTienDonHang = 318000, TinhTrangDonHang = "Đang giao" },
                    new DonHang { IdDonHang = "DH006", IdKhachHang = "ND005", IdKhachHangNavigation = customerLookup["ND005"], ThoiGianDatHang = DateTime.ParseExact("2025-11-14 23:56:31", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), TongTienDonHang = 159000, TinhTrangDonHang = "Đang chuẩn bị" },
                    new DonHang { IdDonHang = "DH007", IdKhachHang = "ND009", IdKhachHangNavigation = customerLookup["ND009"], ThoiGianDatHang = DateTime.ParseExact("2025-11-16 00:49:37", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), TongTienDonHang = 349000, TinhTrangDonHang = "Đang chuẩn bị" }
                };

                var allProducts = new List<SanPham>
                {
                    new SanPham { IdSanPham = "SP001", TenSanPham = "Nước hoa Reminiscence Whisper 50ml", MoTaSanPham = "Như lời thì thầm của gió. Whisper là bản giao hưởng của sự trong trẻo...", GiaNiemYet = 699000, SoLuongTonKho = 100, TrangThaiSp = true, ThoiGianTaoSp = DateTime.Now, IdDanhMucSanPham = "DM001" },
                    new SanPham { IdSanPham = "SP002", TenSanPham = "Nước hoa Reminiscence Horizon 50ml", MoTaSanPham = "Như ánh bình minh rực rỡ. Nước hoa Horizon mở đầu với cảm màu tươi sáng...", GiaNiemYet = 699000, SoLuongTonKho = 100, TrangThaiSp = true, ThoiGianTaoSp = DateTime.Now, IdDanhMucSanPham = "DM001" },
                    new SanPham { IdSanPham = "SP003", TenSanPham = "Nước hoa Reminiscence Eclipse 50ml", MoTaSanPham = "Eclipse cuốn hút như khoảng khắc nhất thực. Hoa quyến tiếp dụng đá...", GiaNiemYet = 699000, SoLuongTonKho = 100, TrangThaiSp = true, ThoiGianTaoSp = DateTime.Now, IdDanhMucSanPham = "DM001" },
                    new SanPham { IdSanPham = "SP004", TenSanPham = "Nước hoa Reminiscence Aurora 50ml", MoTaSanPham = "Aurora mềm mại như cực quang trên bầu trời đêm. Kết hợp bergamot...", GiaNiemYet = 699000, SoLuongTonKho = 100, TrangThaiSp = true, ThoiGianTaoSp = DateTime.Now, IdDanhMucSanPham = "DM001" },
                    new SanPham { IdSanPham = "SP005", TenSanPham = "Nước hoa Reminiscence Whisper 10ml", MoTaSanPham = "Như lời thì thầm của gió. Whisper là bản giao hưởng của sự...", GiaNiemYet = 179000, SoLuongTonKho = 100, TrangThaiSp = true, ThoiGianTaoSp = DateTime.Now, IdDanhMucSanPham = "DM001" },
                    new SanPham { IdSanPham = "SP006", TenSanPham = "Nước hoa Reminiscence Horizon 10ml", MoTaSanPham = "Như ánh bình minh rực rỡ. Nước hoa Horizon mở đầu với cảm màu...", GiaNiemYet = 179000, SoLuongTonKho = 100, TrangThaiSp = true, ThoiGianTaoSp = DateTime.Now, IdDanhMucSanPham = "DM001" },
                    new SanPham { IdSanPham = "SP007", TenSanPham = "Nước hoa Reminiscence Eclipse 10ml", MoTaSanPham = "Eclipse cuốn hút như khoảng khắc nhất thực. Hoa quyến tiếp...", GiaNiemYet = 179000, SoLuongTonKho = 100, TrangThaiSp = true, ThoiGianTaoSp = DateTime.Now, IdDanhMucSanPham = "DM001" },
                    new SanPham { IdSanPham = "SP008", TenSanPham = "Nước hoa Reminiscence Aurora 10ml", MoTaSanPham = "Aurora mềm mại như cực quang. Kết hợp bergamot tươi mát...", GiaNiemYet = 179000, SoLuongTonKho = 100, TrangThaiSp = true, ThoiGianTaoSp = DateTime.Now, IdDanhMucSanPham = "DM001" },
                    new SanPham { IdSanPham = "SP009", TenSanPham = "Tinh dầu Seviora Spring 50ml", MoTaSanPham = "Tinh dầu Spring tươi mát và nhẹ nhàng. Khơi dậy ngày mới đầy năng lượng...", GiaNiemYet = 399000, SoLuongTonKho = 50, TrangThaiSp = true, ThoiGianTaoSp = DateTime.Now, IdDanhMucSanPham = "DM002" },
                    new SanPham { IdSanPham = "SP010", TenSanPham = "Tinh dầu Seviora Summer 50ml", MoTaSanPham = "Tinh dầu Summer mang hơi thở nắng vàng rực rỡ. Kết hợp hương...", GiaNiemYet = 399000, SoLuongTonKho = 50, TrangThaiSp = true, ThoiGianTaoSp = DateTime.Now, IdDanhMucSanPham = "DM002" },
                    new SanPham { IdSanPham = "SP011", TenSanPham = "Tinh dầu Seviora Autumn 50ml", MoTaSanPham = "Tinh dầu Autumn mang hương gỗ ấm áp. Đem lại cảm giác thu thái...", GiaNiemYet = 399000, SoLuongTonKho = 50, TrangThaiSp = true, ThoiGianTaoSp = DateTime.Now, IdDanhMucSanPham = "DM002" },
                    new SanPham { IdSanPham = "SP012", TenSanPham = "Tinh dầu Seviora Winter 50ml", MoTaSanPham = "Tinh dầu Winter gợi sự ấm cung giữa ngày lạnh. Hương cây nhẹ...", GiaNiemYet = 399000, SoLuongTonKho = 50, TrangThaiSp = true, ThoiGianTaoSp = DateTime.Now, IdDanhMucSanPham = "DM002" },
                    new SanPham { IdSanPham = "SP013", TenSanPham = "Box quà đặc biệt Scentory x 20/10", MoTaSanPham = "Box quà nước hoa - món quà tinh tế kết hợp nước hoa với hương...", GiaNiemYet = 799000, SoLuongTonKho = 50, TrangThaiSp = true, ThoiGianTaoSp = DateTime.Now, IdDanhMucSanPham = "DM003" }
                };

                // Populate ViewData
                ViewData["RecentOrders"] = allOrders.OrderByDescending(o => o.ThoiGianDatHang).Take(5).ToList();
                ViewData["RecentCustomers"] = allCustomers.OrderByDescending(c => c.NgaySinh).Take(5).ToList();
                ViewData["RecentBlogs"] = allBlogs.OrderByDescending(b => b.ThoiGianTaoBlog).Take(5).ToList();

                // Weekly Sales
                var weeklySalesData = new List<object>();
                for (int i = 6; i >= 0; i--)
                {
                    var date = DateTime.Today.AddDays(-i);
                    var salesForDay = allOrders
                        .Where(o => o.ThoiGianDatHang.Date == date.Date)
                        .Sum(o => o.TongTienDonHang);
                    weeklySalesData.Add(new { date = date.ToString("yyyy-MM-dd"), total = salesForDay });
                }
                ViewData["WeeklySalesData"] = weeklySalesData;

                // Product Summary - Count number of products (not inventory)
                var totalProductsCount = allProducts.Count();
                var productSummaryRaw = allProducts
                    .GroupBy(p => p.IdDanhMucSanPham)
                    .Select(g => new ProductCategorySummary
                    {
                        CategoryName = (productCategoryLookup.TryGetValue(g.Key, out var categoryName) ? categoryName : g.Key) ?? "Unknown Category",
                        Count = g.Count(),  // Count of products in this category
                        Percentage = totalProductsCount > 0 ? (double)g.Count() / totalProductsCount * 100 : 0.0
                    })
                    .OrderByDescending(p => p.Count)
                    .ToList();

                // Convert to lowercase camelCase for JSON
                var productSummaryJson = productSummaryRaw.Select(p => new
                {
                    categoryName = p.CategoryName,
                    count = p.Count,
                    percentage = p.Percentage
                }).ToList();
                ViewData["ProductSummary"] = productSummaryRaw;
                ViewData["ProductSummaryJson"] = productSummaryJson;

                // Debug output
                System.Diagnostics.Debug.WriteLine($"Products in DM001: {productSummaryRaw.FirstOrDefault(p => p.CategoryName == "Nước hoa")?.Count}");
                System.Diagnostics.Debug.WriteLine($"Products in DM002: {productSummaryRaw.FirstOrDefault(p => p.CategoryName == "Tinh dầu")?.Count}");
                System.Diagnostics.Debug.WriteLine($"Products in DM003: {productSummaryRaw.FirstOrDefault(p => p.CategoryName == "Box quà")?.Count}");

                // Order Status
                var orderStatusSummary = allOrders
                    .GroupBy(o => o.TinhTrangDonHang)
                    .Select(g => new ChartSummaryItem { Name = g.Key, Count = g.Count() })
                    .ToList();
                var orderStatusJson = orderStatusSummary.Select(s => new { name = s.Name, count = s.Count }).ToList();
                ViewData["OrderStatusSummary"] = orderStatusSummary;
                ViewData["OrderStatusJson"] = orderStatusJson;

                // Customer Gender
                var customerGenderSummary = allCustomers
                    .Where(c => !string.IsNullOrEmpty(c.GioiTinh))
                    .GroupBy(c => c.GioiTinh)
                    .Select(g => new ChartSummaryItem { Name = g.Key, Count = g.Count() })
                    .ToList();
                var customerGenderJson = customerGenderSummary.Select(g => new { name = g.Name, count = g.Count }).ToList();
                ViewData["CustomerGenderSummary"] = customerGenderSummary;
                ViewData["CustomerGenderJson"] = customerGenderJson;

                // Blog Category
                var blogCategorySummary = allBlogs
                    .Where(b => !string.IsNullOrEmpty(b.DanhMucBlog))
                    .GroupBy(b => b.DanhMucBlog)
                    .Select(g => new ChartSummaryItem { Name = g.Key, Count = g.Count() })
                    .ToList();
                var blogCategoryJson = blogCategorySummary.Select(c => new { name = c.Name, count = c.Count }).ToList();
                ViewData["BlogCategorySummary"] = blogCategorySummary;
                ViewData["BlogCategoryJson"] = blogCategoryJson;

                // Total Counts
                var totalCounts = new List<dynamic>
                {
                    new { name = "Sản phẩm", count = allProducts.Count },
                    new { name = "Đơn hàng", count = allOrders.Count },
                    new { name = "Khách hàng", count = allCustomers.Count },
                    new { name = "Blog", count = allBlogs.Count }
                };
                ViewData["TotalCounts"] = totalCounts;

                return View();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Index: {ex.Message}\n{ex.StackTrace}");
                return View();
            }
        }
    }
}
