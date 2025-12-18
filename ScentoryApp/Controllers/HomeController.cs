using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using ScentoryApp.Models;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.IO;

namespace ScentoryApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ScentoryPtudContext _context;

        public HomeController(ILogger<HomeController> logger, ScentoryPtudContext context)
        {
            _logger = logger;
            _context = context;
        }
        // Map legacy role values to display role
        private string MapRole(string? role)
        {
            if (string.IsNullOrWhiteSpace(role)) return "Khách hàng";
            if (role.Equals("User", StringComparison.OrdinalIgnoreCase)) return "Khách hàng";
            return role;
        }
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult About()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Shop(int page = 1)
        {
            const int pageSize = 12;
            if (page < 1) page = 1;

            // Lấy tất cả sản phẩm active
            var query = _context.SanPhams
                .Include(p => p.IdDanhMucSanPhamNavigation)
                .Where(p => p.TrangThaiSp)
                .OrderByDescending(p => p.ThoiGianTaoSp); // Mặc định sort theo thời gian tạo

            // Tính toán pagination
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Lấy danh sách danh mục và đếm số sản phẩm
            var categories = await _context.DanhMucSanPhams
                .Select(c => new CategoryFilterItem
                {
                    IdDanhMuc = c.IdDanhMucSanPham,
                    TenDanhMuc = c.TenDanhMucSanPham,
                    ProductCount = _context.SanPhams.Count(p => p.IdDanhMucSanPham == c.IdDanhMucSanPham && p.TrangThaiSp)
                })
                .Where(c => c.ProductCount > 0)
                .ToListAsync();

            // Lấy giá min/max thực tế từ DB
            var prices = await _context.SanPhams
                .Where(p => p.TrangThaiSp)
                .Select(p => p.GiaNiemYet)
                .ToListAsync();

            var vm = new ShopFilterViewModel
            {
                Products = products,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalItems = totalItems,
                Categories = categories,
                ActualMinPrice = prices.Any() ? prices.Min() : 0,
                ActualMaxPrice = prices.Any() ? prices.Max() : 5000000
            };

            return View(vm);
        }

        [AllowAnonymous]
        public IActionResult Contact()
        {
            return View();
        }
        [AllowAnonymous]
        public IActionResult CS_Banhang()
        {
            return View();
        }
        [AllowAnonymous]
        public IActionResult CS_Giaohang()
        {
            return View();
        }
        [AllowAnonymous]
        public IActionResult CS_Doitra()
        {
            return View();
        }
        [AllowAnonymous]
        public IActionResult CS_Baomat()
        {
            return View();
        }
        [AllowAnonymous]
        public async Task<IActionResult> Blog(int page = 1)
        {
            int pageSize = 6; // Số bài viết mỗi trang
            if (page < 1) page = 1;

            // 1. Tạo Query từ bảng Blogs trong Database
            var query = _context.Blogs.AsNoTracking();

            // Nếu bạn muốn lọc bài viết đang ẩn/hiện thì bỏ comment dòng dưới:
            // query = query.Where(b => b.TrangThai == 1); 

            // Sắp xếp bài mới nhất lên đầu
            query = query.OrderByDescending(b => b.ThoiGianTaoBlog);

            // 2. Tính toán phân trang
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            if (totalPages > 0 && page > totalPages) page = totalPages;

            // 3. Lấy dữ liệu theo trang
            var pagedBlogs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var vm = new BlogViewModel
            {
                Blogs = pagedBlogs,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(vm);
        }
        [AllowAnonymous]
        public async Task<IActionResult> BlogDetails(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            // 1. Lấy bài viết chính
            var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.IdBlog == id);
            if (blog == null) return NotFound();

            // Tăng lượt xem
            blog.Views += 1;

            // 2. Lấy danh sách bài viết liên quan (Lấy 3 bài mới nhất, trừ bài hiện tại)
            var relatedBlogs = await _context.Blogs
                .Where(b => b.IdBlog != id) // Loại trừ bài đang xem
                .OrderByDescending(b => b.ThoiGianTaoBlog)
                .Take(3)
                .ToListAsync();

            // Gửi danh sách này qua ViewBag
            ViewBag.RelatedBlogs = relatedBlogs;

            await _context.SaveChangesAsync();

            return View(blog);
        }
        [AllowAnonymous]
        public async Task<IActionResult> ProductDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var product = await _context.SanPhams
                .Include(p => p.IdDanhMucSanPhamNavigation)
                .FirstOrDefaultAsync(p => p.IdSanPham == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }


        [AllowAnonymous] // ==== cho phép truy cập không cần login ====
        public IActionResult Login(string returnUrl = "/")
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LoginApi([FromBody] LoginRequest req)
        {
            if (string.IsNullOrEmpty(req.Username) || string.IsNullOrEmpty(req.Password))
            {
                return Json(new { success = false, message = "Thông tin không hợp lệ." });
            }

            // kiểm tra trong DB
            var account = await _context.TaiKhoans
                .FirstOrDefaultAsync(t => t.TenDangNhap == req.Username && t.MatKhau == req.Password);

            if (account == null)
            {
                return Json(new { success = false, message = "Tên đăng nhập hoặc mật khẩu không đúng." });
            }

            // Determine whether the client is attempting to sign in to Admin area or User area.
            // Priority: explicit `req.Role` -> infer from `req.ReturnUrl` -> default to User area.
            var selectedRole = req.Role?.Trim();
            bool requestedAdminArea = false;
            if (!string.IsNullOrEmpty(selectedRole))
            {
                requestedAdminArea = selectedRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);
            }
            else if (!string.IsNullOrEmpty(req.ReturnUrl))
            {
                // If returnUrl targets admin area, treat it as admin login attempt
                requestedAdminArea = req.ReturnUrl.IndexOf("/Admin/", StringComparison.OrdinalIgnoreCase) >= 0
                                     || req.ReturnUrl.Equals("/Admin", StringComparison.OrdinalIgnoreCase);
            }

            // Detect whether the account in DB is an admin account. Support common variants (Admin, contains 'admin', or Vietnamese 'quản').
            bool isAdminAccount = false;
            if (!string.IsNullOrEmpty(account.VaiTro))
            {
                var vt = account.VaiTro.Trim();
                isAdminAccount = vt.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                                 || vt.IndexOf("admin", StringComparison.OrdinalIgnoreCase) >= 0
                                 || vt.IndexOf("quản", StringComparison.OrdinalIgnoreCase) >= 0;
            }

            // Enforce separation: admin pages require admin accounts; user pages reject admin accounts.
            if (requestedAdminArea && !isAdminAccount)
            {
                return Json(new { success = false, message = "Tài khoản không có quyền truy cập quản trị." });
            }

            if (!requestedAdminArea && isAdminAccount)
            {
                return Json(new { success = false, message = "Tài khoản quản trị không thể đăng nhập ở khu vực khách hàng." });
            }

            // tạo cookie với role từ DB
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, account.TenDangNhap),
                new Claim(ClaimTypes.NameIdentifier, account.IdTaiKhoan),
                new Claim(ClaimTypes.Role, MapRole(account.VaiTro))
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            string redirectUrl = "/";
            if (account.VaiTro != null && account.VaiTro.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                redirectUrl = "/Admin/Home/Index";
            }
            else
            {
                if (!string.IsNullOrEmpty(req.ReturnUrl) && Url.IsLocalUrl(req.ReturnUrl))
                    redirectUrl = req.ReturnUrl;
            }

            return Json(new { success = true, returnUrl = redirectUrl });
        }

        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string Role { get; set; }
            public string ReturnUrl { get; set; }
        }

        public class RegisterRequest
        {
            public string FullName { get; set; }
            public string Gender { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string BirthDate { get; set; }
            public string Address { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Role { get; set; }
        }

        public class AccountUpdateRequest
        {
            public string FullName { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string Address { get; set; }
            public string BirthDate { get; set; }
            public string Gender { get; set; }
        }

        public class ChangePasswordRequest
        {
            public string CurrentPassword { get; set; }
            public string NewPassword { get; set; }
            public string ConfirmNewPassword { get; set; }
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Home");
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterApi([FromBody] RegisterRequest req)
        {
            if (req == null || string.IsNullOrEmpty(req.Username) || string.IsNullOrEmpty(req.Password) || string.IsNullOrEmpty(req.Email))
            {
                return Json(new { success = false, message = "Dữ liệu đăng ký không hợp lệ." });
            }

            // check username/email unique
            var exists = await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == req.Username);
            if (exists)
                return Json(new { success = false, message = "Tên đăng nhập đã tồn tại." });

            var emailExists = await _context.KhachHangs.AnyAsync(k => k.Email == req.Email);
            if (emailExists)
                return Json(new { success = false, message = "Email đã được sử dụng." });

            // tạo Id cho tài khoản theo định dạng TK### (ví dụ: TK001)
            async Task<string> GenerateTaiKhoanIdAsync()
            {
                var existing = await _context.TaiKhoans
                    .Where(t => t.IdTaiKhoan.StartsWith("TK"))
                    .Select(t => t.IdTaiKhoan.Substring(2))
                    .ToListAsync();

                int max = 0;
                foreach (var s in existing)
                {
                    if (int.TryParse(s, out var n))
                        max = Math.Max(max, n);
                }

                return "TK" + (max + 1).ToString("D3");
            }

            // tạo Id ngắn 5 ký tự cho KhachHang (giữ cách tạo ngẫu nhiên trước đó)
            string genId() => Guid.NewGuid().ToString("N").Substring(0, 5).ToUpper();

            var newAccount = new TaiKhoan
            {
                IdTaiKhoan = await GenerateTaiKhoanIdAsync(),
                TenDangNhap = req.Username,
                MatKhau = req.Password,
                VaiTro = "Khách hàng"
            };

            var newCustomer = new KhachHang
            {
                IdKhachHang = genId(),
                HoTen = req.FullName ?? req.Username,
                Email = req.Email,
                Sdt = req.Phone ?? string.Empty,
                DiaChi = req.Address ?? string.Empty,
                GioiTinh = req.Gender ?? string.Empty,
                NgaySinh = string.IsNullOrEmpty(req.BirthDate) ? DateOnly.FromDateTime(DateTime.MinValue) : DateOnly.Parse(req.BirthDate),
                IdTaiKhoan = newAccount.IdTaiKhoan
            };

            try
            {
                await _context.TaiKhoans.AddAsync(newAccount);
                await _context.KhachHangs.AddAsync(newCustomer);
                await _context.SaveChangesAsync();

                // Registration succeeded. Do NOT auto sign-in; redirect user to Login page to sign in.
                var loginUrl = Url.Action("Login", "Home") ?? "/Home/Login";
                return Json(new { success = true, returnUrl = loginUrl, message = "Đăng ký thành công. Vui lòng đăng nhập." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        public async Task<IActionResult> Account()
        {
            // Try to find user by NameIdentifier claim (preferred). If not available,
            // fall back to the username (ClaimTypes.Name / User.Identity.Name).
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            KhachHang? kh = null;

            if (!string.IsNullOrEmpty(accountId))
            {
                kh = await _context.KhachHangs
                    .Include(k => k.IdTaiKhoanNavigation)
                    .FirstOrDefaultAsync(k => k.IdTaiKhoan == accountId);
            }

            if (kh == null)
            {
                // fallback: try username
                var username = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name;
                if (!string.IsNullOrEmpty(username))
                {
                    var acct = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.TenDangNhap == username);
                    if (acct != null)
                    {
                        kh = await _context.KhachHangs
                            .Include(k => k.IdTaiKhoanNavigation)
                            .FirstOrDefaultAsync(k => k.IdTaiKhoan == acct.IdTaiKhoan);
                    }
                }
            }

            if (kh == null)
            {
                // nothing found — show view with no model and an informational message
                TempData["AccountInfoMessage"] = "Không tìm thấy thông tin khách hàng cho tài khoản hiện tại.";
                return View();
            }

            return View(kh);
        }
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AccountUpdate(AccountUpdateRequest req)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Challenge();

            var kh = await _context.KhachHangs
                .Include(k => k.IdTaiKhoanNavigation)
                .FirstOrDefaultAsync(k => k.IdTaiKhoan == accountId);

            if (kh == null)
                return NotFound();

            // Validate email trùng
            if (!string.Equals(kh.Email, req.Email, StringComparison.OrdinalIgnoreCase))
            {
                var emailExists = await _context.KhachHangs
                    .AnyAsync(k => k.Email == req.Email && k.IdKhachHang != kh.IdKhachHang);

                if (emailExists)
                {
                    TempData["AccountInfoMessage"] = "Email đã được sử dụng.";
                    return RedirectToAction("Account");
                }
            }

            // UPDATE DATA
            kh.HoTen = req.FullName;
            kh.Email = req.Email;
            kh.Sdt = req.Phone;
            kh.DiaChi = req.Address;

            await _context.SaveChangesAsync();

            // THÔNG BÁO THÀNH CÔNG
            TempData["AccountInfoMessage"] = "Cập nhật thông tin thành công!";

            // LOAD LẠI DATA MỚI
            return RedirectToAction("Account");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest req)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Challenge();

            var taiKhoan = await _context.TaiKhoans
                .FirstOrDefaultAsync(t => t.IdTaiKhoan == accountId);

            if (taiKhoan == null)
                return NotFound();

            // Kiểm tra nhập đủ
            if (string.IsNullOrEmpty(req.CurrentPassword) ||
                string.IsNullOrEmpty(req.NewPassword) ||
                string.IsNullOrEmpty(req.ConfirmNewPassword))
            {
                TempData["PasswordMessage"] = "Vui lòng nhập đầy đủ thông tin.";
                return RedirectToAction("Account");
            }

            // Kiểm tra mật khẩu hiện tại
            if (taiKhoan.MatKhau != req.CurrentPassword)
            {
                TempData["PasswordMessage"] = "Mật khẩu hiện tại không đúng.";
                return RedirectToAction("Account");
            }

            // Kiểm tra xác nhận mật khẩu
            if (req.NewPassword != req.ConfirmNewPassword)
            {
                TempData["PasswordMessage"] = "Mật khẩu xác nhận không khớp.";
                return RedirectToAction("Account");
            }

            // Update mật khẩu mới
            taiKhoan.MatKhau = req.NewPassword;

            await _context.SaveChangesAsync();

            TempData["PasswordMessage"] = "Đổi mật khẩu thành công!";

            return RedirectToAction("Account");
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetMyOrders()
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var kh = _context.KhachHangs
                .FirstOrDefault(k => k.IdTaiKhoan == accountId);

            if (kh == null)
                return Unauthorized();

            var orders = _context.DonHangs
                .Where(dh => dh.IdKhachHang == kh.IdKhachHang)
                .OrderByDescending(dh => dh.ThoiGianDatHang)
                .Select(dh => new
                {
                    id = dh.IdDonHang,
                    date = dh.ThoiGianDatHang,
                    status = dh.TinhTrangDonHang,
                    total = dh.TongTienDonHang
                })
                .ToList();

            return Json(orders);
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetOrderDetail(string orderId)
        {
            var order = _context.DonHangs
                .Where(d => d.IdDonHang == orderId)
                .Select(d => new
                {
                    id = d.IdDonHang,

                    products = d.ChiTietDonHangs.Select(ct => new
                    {
                        name = ct.IdSanPhamNavigation.TenSanPham,
                        quantity = ct.SoLuong,
                        price = ct.DonGia
                    }).ToList(),

                    tongTienSanPham = d.ChiTietDonHangs
                        .Sum(ct => ct.SoLuong * ct.DonGia),
                    shippingFee = d.PhiVanChuyen,
                    total = d.TongTienDonHang,
                    discountAmount = (d.ChiTietDonHangs.Sum(ct => ct.SoLuong * ct.DonGia)+ d.PhiVanChuyen)- d.TongTienDonHang,
                    discountCode = d.IdMaGiamGia,
                    shippingInfo = new
                    {
                        address = d.IdKhachHangNavigation.DiaChi,
                        phone = d.IdKhachHangNavigation.Sdt
                    }
                })
                .FirstOrDefault();

            if (order == null)
                return NotFound();

            return Json(order);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// API AJAX để filter sản phẩm real-time - VERSION FIXED
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> FilterProducts([FromBody] ShopFilterViewModel filter)
        {
            try
            {
                _logger?.LogInformation("🔍 FilterProducts called with: {@Filter}", filter);

                if (filter == null)
                {
                    _logger?.LogWarning("⚠️ Filter is null");
                    return Json(new { success = false, message = "Invalid filter data" });
                }

                // Khởi tạo giá trị mặc định
                if (filter.Page < 1) filter.Page = 1;
                if (filter.PageSize < 1) filter.PageSize = 12;

                // Query cơ bản
                var query = _context.SanPhams
                    .Include(p => p.IdDanhMucSanPhamNavigation)
                    .Where(p => p.TrangThaiSp)
                    .AsQueryable();

                _logger?.LogInformation("📦 Initial query count: {Count}", await query.CountAsync());

                // ============ FILTER THEO GIÁ ============
                if (filter.MinPrice.HasValue && filter.MinPrice.Value > 0)
                {
                    query = query.Where(p => p.GiaNiemYet >= filter.MinPrice.Value);
                    _logger?.LogInformation("💰 Applied min price filter: {MinPrice}", filter.MinPrice.Value);
                }

                if (filter.MaxPrice.HasValue && filter.MaxPrice.Value > 0)
                {
                    query = query.Where(p => p.GiaNiemYet <= filter.MaxPrice.Value);
                    _logger?.LogInformation("💰 Applied max price filter: {MaxPrice}", filter.MaxPrice.Value);
                }

                // ============ FILTER THEO DANH MỤC ============
                // FIX: Kiểm tra null và Any() trước khi dùng Contains
                if (filter.CategoryIds != null && filter.CategoryIds.Any())
                {
                    _logger?.LogInformation("📂 Filtering by categories: {Categories}", string.Join(", ", filter.CategoryIds));

                    // FIX: Trim whitespace và uppercase để đảm bảo khớp
                    var normalizedCategories = filter.CategoryIds
                        .Select(c => c?.Trim().ToUpper())
                        .Where(c => !string.IsNullOrEmpty(c))
                        .ToList();

                    if (normalizedCategories.Any())
                    {
                        query = query.Where(p => normalizedCategories.Contains(p.IdDanhMucSanPham.ToUpper()));
                        _logger?.LogInformation("📂 After category filter count: {Count}", await query.CountAsync());
                    }
                }
                else
                {
                    _logger?.LogInformation("📂 No category filter applied");
                }

                // ============ SẮP XẾP ============
                // FIX: Sắp xếp theo ThoiGianCapNhat cho "newest", theo GiaNiemYet cho price
                _logger?.LogInformation("🔄 Sorting by: {SortBy}", filter.SortBy ?? "default");

                query = filter.SortBy switch
                {
                    "price_asc" => query.OrderBy(p => p.GiaNiemYet),
                    "price_desc" => query.OrderByDescending(p => p.GiaNiemYet),
                    "newest" => query.OrderByDescending(p => p.ThoiGianCapNhat ?? p.ThoiGianTaoSp), // FIX: Dùng ThoiGianCapNhat, fallback ThoiGianTaoSp
                    _ => query.OrderByDescending(p => p.ThoiGianTaoSp) // default: mới tạo nhất
                };

                // ============ PAGINATION ============
                var totalItems = await query.CountAsync();
                _logger?.LogInformation("📊 Total items after filter: {Count}", totalItems);

                var totalPages = (int)Math.Ceiling(totalItems / (double)filter.PageSize);
                if (totalPages == 0) totalPages = 1;
                if (filter.Page > totalPages) filter.Page = totalPages;

                var products = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                _logger?.LogInformation("📦 Products returned: {Count}", products.Count);

                // ============ RENDER PARTIAL VIEW ============
                var html = await this.RenderViewAsync("_ProductList", products, true);

                return Json(new
                {
                    success = true,
                    html = html,
                    totalItems = totalItems,
                    totalPages = totalPages,
                    currentPage = filter.Page
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Error in FilterProducts");
                return Json(new
                {
                    success = false,
                    message = "Có lỗi xảy ra: " + ex.Message
                });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string returnUrl = "/", string role = "User")
        {
            // Chỉ cho khách hàng (User) dùng social login
            if (!string.Equals(role, "User", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            if (provider != "Google" && provider != "Facebook")
                return BadRequest("Provider không hợp lệ.");

            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                returnUrl = "/";

            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(ExternalLoginCallback), new { returnUrl, role })
            };

            return Challenge(props, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = "/", string role = "User", string? remoteError = null)
        {
            if (!string.Equals(role, "User", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            if (!string.IsNullOrEmpty(remoteError))
            {
                TempData["message"] = "Đăng nhập mạng xã hội bị lỗi: " + remoteError;
                return RedirectToAction("Login", new { returnUrl });
            }

            var extResult = await HttpContext.AuthenticateAsync("External");
            if (!extResult.Succeeded || extResult.Principal == null)
            {
                TempData["message"] = "Không lấy được thông tin đăng nhập từ Google/Facebook.";
                return RedirectToAction("Login", new { returnUrl });
            }

            var principal = extResult.Principal;

            var email = principal.FindFirstValue(ClaimTypes.Email);
            var fullName = principal.FindFirstValue(ClaimTypes.Name) ?? email;

            if (string.IsNullOrWhiteSpace(email))
            {
                // Facebook đôi khi không trả email nếu account không có/không cấp quyền email
                TempData["message"] = "Tài khoản Facebook/Google không cung cấp Email. Vui lòng dùng tài khoản khác hoặc đăng ký thủ công.";
                await HttpContext.SignOutAsync("External");
                return RedirectToAction("Login", new { returnUrl });
            }

            // 1) Tìm khách hàng theo Email
            var existingCustomer = await _context.KhachHangs.FirstOrDefaultAsync(k => k.Email == email);

            TaiKhoan account;
            if (existingCustomer != null && !string.IsNullOrEmpty(existingCustomer.IdTaiKhoan))
            {
                // đã có customer + đã link tài khoản
                account = await _context.TaiKhoans.FirstAsync(t => t.IdTaiKhoan == existingCustomer.IdTaiKhoan);
            }
            else
            {
                // 2) Chưa có -> tạo mới TaiKhoan + KhachHang, hoặc tạo TaiKhoan rồi gắn vào KhachHang đã có
                var newAccountId = await GenerateTaiKhoanIdAsync();
                var username = await GenerateUniqueUsernameFromEmailAsync(email);

                account = new TaiKhoan
                {
                    IdTaiKhoan = newAccountId,
                    TenDangNhap = username,
                    MatKhau = Guid.NewGuid().ToString("N"), // mật khẩu random vì login bằng social
                    VaiTro = "Khách hàng"
                };

                await _context.TaiKhoans.AddAsync(account);

                if (existingCustomer == null)
                {
                    var newCustomer = new KhachHang
                    {
                        IdKhachHang = GenerateCustomerId(),
                        HoTen = string.IsNullOrWhiteSpace(fullName) ? username : fullName,
                        Email = email,
                        Sdt = "",
                        DiaChi = "",
                        GioiTinh = "",
                        NgaySinh = DateOnly.FromDateTime(DateTime.MinValue),
                        IdTaiKhoan = account.IdTaiKhoan
                    };
                    await _context.KhachHangs.AddAsync(newCustomer);
                }
                else
                {
                    // đã có customer theo email nhưng chưa link tài khoản
                    existingCustomer.IdTaiKhoan = account.IdTaiKhoan;
                    if (string.IsNullOrWhiteSpace(existingCustomer.HoTen))
                        existingCustomer.HoTen = string.IsNullOrWhiteSpace(fullName) ? username : fullName;
                }

                await _context.SaveChangesAsync();
            }

            // 3) Sign in bằng cookie của hệ thống giống LoginApi của bạn
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, account.TenDangNhap),
                    new Claim(ClaimTypes.NameIdentifier, account.IdTaiKhoan),
                    new Claim(ClaimTypes.Role, MapRole(account.VaiTro)),
                    new Claim(ClaimTypes.Email, email)
                };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity)
            );

            // dọn cookie tạm
            await HttpContext.SignOutAsync("External");

            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                returnUrl = "/";

            return Redirect(returnUrl);
        }

        // ==== helpers ====

        private async Task<string> GenerateTaiKhoanIdAsync()
        {
            var existing = await _context.TaiKhoans
                .Where(t => t.IdTaiKhoan.StartsWith("TK"))
                .Select(t => t.IdTaiKhoan.Substring(2))
                .ToListAsync();

            int max = 0;
            foreach (var s in existing)
                if (int.TryParse(s, out var n)) max = Math.Max(max, n);

            return "TK" + (max + 1).ToString("D3");
        }

        private string GenerateCustomerId()
            => Guid.NewGuid().ToString("N").Substring(0, 5).ToUpper();

        private async Task<string> GenerateUniqueUsernameFromEmailAsync(string email)
        {
            var baseName = email.Split('@')[0].Trim();
            if (string.IsNullOrWhiteSpace(baseName)) baseName = "user";

            // chỉ giữ ký tự an toàn
            baseName = new string(baseName.Where(ch => char.IsLetterOrDigit(ch) || ch == '_' || ch == '.').ToArray());
            if (string.IsNullOrWhiteSpace(baseName)) baseName = "user";

            var username = baseName;
            var i = 0;

            while (await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == username))
            {
                i++;
                username = $"{baseName}{i}";
            }

            return username;
        }
    }



    /// <summary>
    /// Extension methods cho Controller
    /// </summary>
    public static class ControllerExtensions
    {
        /// <summary>
        /// Render partial view thành HTML string
        /// </summary>
        public static async Task<string> RenderViewAsync<TModel>(this Controller controller, string viewName, TModel model, bool partial = false)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = controller.ControllerContext.ActionDescriptor.ActionName;
            }

            controller.ViewData.Model = model;

            using (var writer = new StringWriter())
            {
                IViewEngine viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;

                if (viewEngine == null)
                {
                    throw new InvalidOperationException("Could not resolve ICompositeViewEngine");
                }

                ViewEngineResult viewResult = viewEngine.FindView(controller.ControllerContext, viewName, !partial);

                if (viewResult.Success == false)
                {
                    var searchedLocations = string.Join(", ", viewResult.SearchedLocations);
                    return $"ERROR: A view with the name '{viewName}' could not be found. Searched locations: {searchedLocations}";
                }

                ViewContext viewContext = new ViewContext(
                    controller.ControllerContext,
                    viewResult.View,
                    controller.ViewData,
                    controller.TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);

                return writer.GetStringBuilder().ToString();
            }
        }
    }
}
