using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private readonly DB _context;

        public LoginController(ILogger<LoginController> logger, DB context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string Role, string UserId, string Password, string returnUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(Role) || string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(Password))
                {
                    ViewBag.ErrorMessage = "Role, Email and Password are required";
                    return View("Index");
                }

                if (Role == "customer")
                {
                    // 從 Customers 表驗證
                    var customer = _context.Customers.FirstOrDefault(u => u.Email == UserId && u.Password == Password);

                    if (customer != null)
                    {
                        HttpContext.Session.SetString("UserEmail", customer.Email);
                        HttpContext.Session.SetString("UserName", customer.Name);
                        HttpContext.Session.SetString("UserRole", "customer");

                        if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
                        return RedirectToAction("Index", "Customer");
                    }
                    ViewBag.ErrorMessage = "Invalid email or password.";
                }
                else if (Role == "admin" && UserId == "admin@test.com" && Password == "123")
                {
                    HttpContext.Session.SetString("UserRole", "admin");
                    return RedirectToAction("Index", "AdminHomePage");
                }

                return View("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error");
                ViewBag.ErrorMessage = "Database error: " + ex.Message;
                return View("Index");
            }
        }

        public IActionResult CusRegister() => View();




[HttpPost]
public IActionResult CusRegister(Customer model, string ConfirmPassword)
        {
            // 1. 验证密码长度 (至少 8 位)
            if (string.IsNullOrEmpty(model.Password) || model.Password.Length < 8)
            {
                ModelState.AddModelError("Password", "Password must be at least 8 characters long.");
                return View(model);
            }

            // 2. 验证两次密码是否一致
            if (model.Password != ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
                return View(model);
            }

            // --- 新增代码：验证 Name 是否已存在 ---
            bool nameExists = _context.Customers.Any(c => c.Name == model.Name);
            if (nameExists)
            {
                ModelState.AddModelError("Name", "This name is already taken. Please choose another one.");
                return View(model);
            }
            // ------------------------------------

            // 3. 验证 Email 是否已存在
            bool emailExists = _context.Customers.Any(c => c.Email == model.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

            // 4. 自动生成 CustomerId (CUS001, CUS002...)
            var lastCustomer = _context.Customers
                .OrderByDescending(c => c.Id)
                .FirstOrDefault();

            string newCusId = "CUS001";
            if (lastCustomer != null && !string.IsNullOrEmpty(lastCustomer.CustomerId))
            {
                string numericPart = lastCustomer.CustomerId.Substring(3);
                if (int.TryParse(numericPart, out int lastNumber))
                {
                    newCusId = "CUS" + (lastNumber + 1).ToString("D3");
                }
            }

            model.CustomerId = newCusId;

            // 5. 保存到数据库
            _context.Customers.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Index", "Login");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}