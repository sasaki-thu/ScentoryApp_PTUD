using System;
using System.Collections.Generic;

namespace ScentoryApp.Models;

public partial class SanPham
{
    public string IdSanPham { get; set; } = null!;

    public string TenSanPham { get; set; } = null!;

    public string MoTaSanPham { get; set; } = null!;

    public decimal GiaNiemYet { get; set; }

    public int SoLuongTonKho { get; set; }

    public bool TrangThaiSp { get; set; }

    public DateTime ThoiGianTaoSp { get; set; }

    public DateTime? ThoiGianCapNhat { get; set; }

    public string IdDanhMucSanPham { get; set; } = null!;

    public byte[]? AnhSanPham { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();

    public virtual ICollection<DanhGiaSanPham> DanhGiaSanPhams { get; set; } = new List<DanhGiaSanPham>();

    public virtual DanhMucSanPham IdDanhMucSanPhamNavigation { get; set; } = null!;
}
