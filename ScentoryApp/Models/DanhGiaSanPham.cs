using System;
using System.Collections.Generic;

namespace ScentoryApp.Models;

public partial class DanhGiaSanPham
{
    public string IdDanhGia { get; set; } = null!;

    public string IdSanPham { get; set; } = null!;

    public string IdKhachHang { get; set; } = null!;

    public int SoSao { get; set; }

    public string NoiDung { get; set; } = null!;

    public DateTime ThoiGianDanhGia { get; set; }

    public virtual KhachHang IdKhachHangNavigation { get; set; } = null!;

    public virtual SanPham IdSanPhamNavigation { get; set; } = null!;
}
