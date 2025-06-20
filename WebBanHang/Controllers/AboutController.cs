using Microsoft.AspNetCore.Mvc;

namespace WebBanHang.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
