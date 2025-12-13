using ScentoryApp.Models;

namespace ScentoryApp.Models
{
    /// <summary>
    /// ViewModel cho filter trang Shop
    /// </summary>
    public class ShopFilterViewModel
    {
        // Tham số filter từ client
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public List<string>? CategoryIds { get; set; }
        public string? SortBy { get; set; } // "default", "price_asc", "price_desc", "newest"
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;

        // Dữ liệu trả về
        public List<SanPham>? Products { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }

        // Metadata cho filter sidebar
        public List<CategoryFilterItem>? Categories { get; set; }
        public decimal ActualMinPrice { get; set; }
        public decimal ActualMaxPrice { get; set; }
    }

    /// <summary>
    /// Item cho danh mục filter
    /// </summary>
    public class CategoryFilterItem
    {
        public string IdDanhMuc { get; set; } = null!;
        public string TenDanhMuc { get; set; } = null!;
        public int ProductCount { get; set; }
    }
}
