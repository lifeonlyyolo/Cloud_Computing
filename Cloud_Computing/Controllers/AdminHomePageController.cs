using Microsoft.AspNetCore.Mvc;
using WebApplication1.Filters;

namespace WebApplication1.Controllers
{
    public class AdminHomePageController : Controller
    {
        [TypeFilter(typeof(AdminCheckFilter))]

        public IActionResult Index()
        {
            return View();
        }
    }
}
