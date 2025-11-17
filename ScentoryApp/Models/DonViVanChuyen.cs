using System;
using System.Collections.Generic;

namespace ScentoryApp.Models;

public partial class DonViVanChuyen
{
    public string IdDonViVanChuyen { get; set; } = null!;

    public string TenDonViVanChuyen { get; set; } = null!;

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();
}
