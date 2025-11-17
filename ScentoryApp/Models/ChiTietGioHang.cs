using System;
using System.Collections.Generic;

namespace ScentoryApp.Models;

public partial class ChiTietGioHang
{
    public string IdGioHang { get; set; } = null!;

    public string IdSanPham { get; set; } = null!;

    public int SoLuong { get; set; }

    public virtual GioHang IdGioHangNavigation { get; set; } = null!;

    public virtual SanPham IdSanPhamNavigation { get; set; } = null!;
}
