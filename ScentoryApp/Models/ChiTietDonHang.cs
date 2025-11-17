using System;
using System.Collections.Generic;

namespace ScentoryApp.Models;

public partial class ChiTietDonHang
{
    public string IdDonHang { get; set; } = null!;

    public string IdSanPham { get; set; } = null!;

    public decimal DonGia { get; set; }

    public int SoLuong { get; set; }

    public decimal ThanhTien { get; set; }

    public virtual DonHang IdDonHangNavigation { get; set; } = null!;

    public virtual SanPham IdSanPhamNavigation { get; set; } = null!;
}
