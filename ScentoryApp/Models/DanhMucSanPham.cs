using System;
using System.Collections.Generic;

namespace ScentoryApp.Models;

public partial class DanhMucSanPham
{
    public string IdDanhMucSanPham { get; set; } = null!;

    public string TenDanhMucSanPham { get; set; } = null!;

    public string MoTaDanhMuc { get; set; } = null!;

    public bool TrangThaiDanhMuc { get; set; }

    public DateTime ThoiGianTaoDm { get; set; }

    public DateTime? ThoiGianCapNhatDm { get; set; }

    public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
}
