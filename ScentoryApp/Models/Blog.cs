using System;
using System.Collections.Generic;

namespace ScentoryApp.Models;

public partial class Blog
{
    public string IdBlog { get; set; } = null!;

    public string TenBlog { get; set; } = null!;

    /// <summary>
    /// HDSD/Kiến thức/Mẹo/Sản phẩm/Thương hiệu
    /// </summary>
    public string? DanhMucBlog { get; set; }

    public string NoiDung { get; set; } = null!;

    public byte[]? AnhBlog { get; set; }

    public DateTime ThoiGianTaoBlog { get; set; }

    public DateTime? ThoiGianCapNhatBlog { get; set; }
}
