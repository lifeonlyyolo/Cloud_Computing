using System.Diagnostics;
using Cloud_Computing.Models; // 請確保與你的專案命名空間一致
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace Cloud_Computing.Controllers
{
    public class HomeController : Controller
    {
        private readonly DB _context;

        // 透過建構函式注入資料庫上下文
        public HomeController(DB context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // 從資料庫獲取所有菜單項並傳遞給 View
            var menuList = _context.MenuItems.ToList();
            return View(menuList);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}