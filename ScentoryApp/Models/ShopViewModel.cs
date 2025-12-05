using System.Collections.Generic;

namespace ScentoryApp.Models
{
    public class ShopViewModel
    {
        public IEnumerable<SanPham> Products { get; set; } = new List<SanPham>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
