using System;
using System.Collections.Generic;

namespace ScentoryApp.Models;

public partial class DonHang
{
    public string IdDonHang { get; set; } = null!;

    public string IdKhachHang { get; set; } = null!;

    public DateTime ThoiGianDatHang { get; set; }

    public DateOnly NgayGiaoHangDuKien { get; set; }

    public string IdDonViVanChuyen { get; set; } = null!;

    public decimal TongTienDonHang { get; set; }

    public decimal PhiVanChuyen { get; set; }

    public decimal ThueBanHang { get; set; }

    public DateTime? ThoiGianCapNhat { get; set; }

    public string TinhTrangDonHang { get; set; } = null!;

    public DateTime? ThoiGianHoanTatDonHang { get; set; }

    public string IdMaGiamGia { get; set; } = null!;

    public string DiaChiNhanHang { get; set; } = null!;

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual DonViVanChuyen IdDonViVanChuyenNavigation { get; set; } = null!;

    public virtual KhachHang IdKhachHangNavigation { get; set; } = null!;

    public virtual MaGiamGium IdMaGiamGiaNavigation { get; set; } = null!;
}
