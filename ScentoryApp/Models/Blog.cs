using System;
using System.Collections.Generic;

namespace ScentoryApp.Models;

public partial class Blog
{
    public string IdBlog { get; set; } = null!;

    public string TenBlog { get; set; } = null!;

    public string? DanhMucBlog { get; set; }

    public string NoiDungNgan { get; set; } = null!;

    public string NoiDung { get; set; } = null!;

    public string Tag { get; set; } = null!;

    public int TrangThai { get; set; }

    public string Alias { get; set; } = null!;

    public byte[]? AnhBlog { get; set; }

    public string? MetaKey { get; set; }

    public string? MetaDesc { get; set; }

    public int Views { get; set; }

    public DateTime ThoiGianTaoBlog { get; set; }

    public DateTime? ThoiGianCapNhatBlog { get; set; }
}
