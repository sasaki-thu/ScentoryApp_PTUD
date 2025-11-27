using System;
using System.Collections.Generic;

namespace ScentoryApp.Models;

public partial class TaiKhoan
{
    public string IdTaiKhoan { get; set; } = null!;

    public string TenDangNhap { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string VaiTro { get; set; } = null!;

    public virtual ICollection<KhachHang> KhachHangs { get; set; } = new List<KhachHang>();
}
