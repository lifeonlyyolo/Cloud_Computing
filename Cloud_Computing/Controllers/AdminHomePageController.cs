using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class AdminHomePageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
