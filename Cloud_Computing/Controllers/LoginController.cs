using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using WebApplication1.Models;
using WebApplication1.Filters;

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
                    var customer = _context.Customers.FirstOrDefault(u => u.Email == UserId && u.Password == Password);

                    if (customer != null)
                    {
                        HttpContext.Session.SetString("UserEmail", customer.Email);
                        HttpContext.Session.SetString("UserName", customer.Name);
                        HttpContext.Session.SetString("UserRole", "customer");

                        if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
                        return RedirectToAction("Index", "Customer");
                    }
                }
                else if (Role == "admin")
                {
                    var admin = _context.Admins.FirstOrDefault(u => u.Email == UserId && u.Password == Password);

                    if (admin != null)
                    {
                        HttpContext.Session.SetString("UserEmail", admin.Email);
                        HttpContext.Session.SetString("UserName", admin.Name);
                        HttpContext.Session.SetString("UserRole", "admin");

                        if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
                        return RedirectToAction("Index", "AdminHomePage");
                    }
                }

                ViewBag.ErrorMessage = "Invalid email or password.";
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
            if (string.IsNullOrEmpty(model.Password) || model.Password.Length < 8)
            {
                ModelState.AddModelError("Password", "Password must be at least 8 characters long.");
                return View(model);
            }

            if (model.Password != ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
                return View(model);
            }

            bool nameExists = _context.Customers.Any(c => c.Name == model.Name);
            if (nameExists)
            {
                ModelState.AddModelError("Name", "This name is already taken. Please choose another one.");
                return View(model);
            }
            bool emailExists = _context.Customers.Any(c => c.Email == model.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

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