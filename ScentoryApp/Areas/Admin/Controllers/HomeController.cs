using Microsoft.AspNetCore.Mvc;

namespace ScentoryApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Products()
        {
            return View();
        }

        public IActionResult Categories()
    {
        return View();
    }

    public IActionResult Orders()
    {
        return View();
    }

    public IActionResult Customers()
    {
        return View();
    }

    public IActionResult Coupons()
    {
        return View();
    }

    public IActionResult Accounts()
    {
        return View();
    }
    }
}
