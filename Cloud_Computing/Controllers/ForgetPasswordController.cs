using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    public class ForgetPasswordController : Controller
    {
        private readonly DB _context;
        private readonly IEmailService _emailService;

        public ForgetPasswordController(DB context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // 1. 顯示輸入郵箱的頁面
        public IActionResult Index() => View();

        // 2. 處理發送連結請求
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendResetLink(string email)
        {
            var customer = _context.Customers.FirstOrDefault(u => u.Email == email);
            if (customer == null)
            {
                ViewBag.ErrorMessage = "The email address was not found.";
                return View("Index");
            }

            // 生成唯一 Token 並存入 MySQL
            string token = Guid.NewGuid().ToString();
            var resetToken = new PasswordResetToken
            {
                Email = email,
                Token = token,
                ExpiryTime = DateTime.Now.AddHours(1), // 1小時有效
                IsUsed = false
            };

            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();

            // 構建重設連結
            string resetLink = Url.Action("ResetPassword", "ForgetPassword", new { token = token }, Request.Scheme);

            // 發送郵件
            string subject = "Reset Your Wave Cafe Password";
            string body = $"<p>You requested a password reset.</p><p>Please <a href='{resetLink}'>click here</a> to set a new password.</p><p>This link expires in 1 hour.</p>";

            await _emailService.SendEmailAsync(email, subject, body);

            ViewBag.SuccessMessage = "Check your email! A reset link has been sent.";
            return View("Index");
        }

        // 3. 顯示修改密碼頁面 (當用戶點擊郵件連結時)
        public IActionResult ResetPassword(string token)
        {
            var tokenRecord = _context.PasswordResetTokens
                .FirstOrDefault(t => t.Token == token && t.ExpiryTime > DateTime.Now && !t.IsUsed);

            if (tokenRecord == null) return View("ResetPasswordError");

            ViewBag.Token = token;
            return View();
        }

        // 4. 執行修改密碼
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessReset(string token, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ViewBag.ErrorMessage = "Passwords do not match.";
                ViewBag.Token = token;
                return View("ResetPassword");
            }

            var tokenRecord = _context.PasswordResetTokens
                .FirstOrDefault(t => t.Token == token && t.ExpiryTime > DateTime.Now && !t.IsUsed);

            if (tokenRecord == null) return View("ResetPasswordError");

            var customer = _context.Customers.FirstOrDefault(c => c.Email == tokenRecord.Email);
            if (customer != null)
            {
                customer.Password = newPassword; // 更新資料庫密碼
                tokenRecord.IsUsed = true; // 標記 Token 已失效
                await _context.SaveChangesAsync();
                return View("ResetPasswordSuccess");
            }

            return View("ResetPasswordError");
        }
    }
}