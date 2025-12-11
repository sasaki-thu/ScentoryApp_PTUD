using System.Collections.Generic;
using ScentoryApp.Models;

namespace ScentoryApp.Models
{
    public class BlogViewModel
    {
        public IEnumerable<Blog> Blogs { get; set; } = new List<Blog>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}