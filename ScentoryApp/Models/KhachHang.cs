using System;
using System.Collections.Generic;

namespace ScentoryApp.Models;

public partial class KhachHang
{
    public string IdKhachHang { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Sdt { get; set; } = null!;

    public string DiaChi { get; set; } = null!;

    public string GioiTinh { get; set; } = null!;

    public DateOnly NgaySinh { get; set; }

    public string IdTaiKhoan { get; set; } = null!;

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    public virtual ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();

    public virtual TaiKhoan IdTaiKhoanNavigation { get; set; } = null!;
}
