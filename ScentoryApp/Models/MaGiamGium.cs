using System;
using System.Collections.Generic;

namespace ScentoryApp.Models;

public partial class MaGiamGium
{
    public string IdMaGiamGia { get; set; } = null!;

    public string MoTa { get; set; } = null!;

    public string LoaiGiam { get; set; } = null!;

    public decimal GiaTriGiam { get; set; }

    public decimal GiaTriToiThieu { get; set; }

    public decimal? GiaGiamToiDa { get; set; }

    public DateTime ThoiGianBatDau { get; set; }

    public DateTime ThoiGianKetThuc { get; set; }

    public int GioiHanSuDung { get; set; }

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();
}
