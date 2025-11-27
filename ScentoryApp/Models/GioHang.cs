using System;
using System.Collections.Generic;

namespace ScentoryApp.Models;

public partial class GioHang
{
    public string IdGioHang { get; set; } = null!;

    public DateTime ThoiGianTaoGh { get; set; }

    public DateTime? ThoiGianCapNhatGh { get; set; }

    public string IdKhachHang { get; set; } = null!;

    public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();

    public virtual KhachHang IdKhachHangNavigation { get; set; } = null!;
}
