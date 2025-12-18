using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Text.Json;
using System;
using System.Collections.Generic;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class CustomerController : Controller
    {
        private readonly DB _context;

        public CustomerController(DB context)
        {
            _context = context;
        }

        // 1. 菜单页面
        public IActionResult Index()
        {
            var menuList = _context.MenuItems.ToList();
            return View(menuList);
        }

        // 2. 下单详情页 (GET)
        [HttpGet]
        public IActionResult Order(int itemId)
        {
            var item = _context.MenuItems.FirstOrDefault(m => m.Id == itemId);
            if (item == null) return RedirectToAction("Index");

            // Model 本身就是 MenuItem，裡面已經有 Category 屬性了
            return View(item);
        }

        // 3. 添加到购物车 (POST)
        [HttpPost]
        public IActionResult Order(int ItemId, int Quantity, string Size, string Ice, string Milk, string Sugar, string Packing, string Notes)
        {
            var item = _context.MenuItems.FirstOrDefault(m => m.Id == ItemId);
            if (item == null) return RedirectToAction("Index");

            var userName = HttpContext.Session.GetString("UserName") ?? "Guest";
            decimal unitPrice = item.Price;
            if (Size == "Big") unitPrice += 2.00m;
            decimal total = unitPrice * Quantity;

            var cartOrder = new Order
            {
                OrderId = "CART-" + DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                CustomerName = userName,
                OrderTime = DateTime.Now,
                Status = "InCart",
                ItemName = item.Name,
                ItemImageUrl = item.ImageUrl,
                Quantity = Quantity,
                Size = Size,
                Sugar = Sugar,
                Ice = Ice,
                Milk = Milk,
                Notes = Notes,
                UnitPrice = unitPrice,
                TotalPrice = total
            };
            _context.Orders.Add(cartOrder);
            _context.SaveChanges();
            return RedirectToAction("CartList");
        }

        // 4. 购物车列表
        public IActionResult CartList()
        {
            var userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userName)) return RedirectToAction("Index", "Login");

            var cartItems = _context.Orders
                .Where(o => o.CustomerName == userName && o.Status == "InCart")
                .ToList();

            return View(cartItems);
        }

        // 5. 结算预览 (Invoice)
        // 注意：此处的 DeliveryType 是初始默认值，最终选择是在 Invoice 页面完成的
        [HttpPost]
        public IActionResult ProceedToPayment(string DeliveryType, string UserAddress)
        {
            var userName = HttpContext.Session.GetString("UserName");
            var cartItems = _context.Orders.Where(o => o.CustomerName == userName && o.Status == "InCart").ToList();
            if (!cartItems.Any()) return RedirectToAction("CartList");

            decimal subtotal = cartItems.Sum(i => i.TotalPrice);
            decimal deliveryFee = (DeliveryType == "Delivery") ? 5.00m : 0.00m;

            var invoice = new InvoiceViewModel
            {
                CustomerName = userName,
                CartItems = cartItems,
                BaseTotalPrice = subtotal,
                DeliveryType = DeliveryType ?? "Dine In",
                DeliveryAddress = (DeliveryType == "Delivery") ? UserAddress : "Self-Pickup/Dine In",
                DeliveryFee = deliveryFee,
                TotalPrice = subtotal + deliveryFee
            };
            TempData["PaymentData"] = JsonSerializer.Serialize(invoice);
            return View("Invoice", invoice);
        }

        // 6. 支付页面 (GET) - 修复重点
        // 接收来自 Invoice 页面表单提交的最新配送数据
        public IActionResult Payment(string DeliveryType, string UserAddress)
        {
            if (TempData["PaymentData"] == null) return RedirectToAction("CartList");

            var model = JsonSerializer.Deserialize<InvoiceViewModel>(TempData["PaymentData"].ToString());

            // 更新模型：捕获用户在 Invoice 页面最终选择的配送方式
            model.DeliveryType = DeliveryType ?? "Dine In";
            model.DeliveryAddress = (model.DeliveryType == "Delivery") ? UserAddress : "Self-Pickup/Dine In";
            model.DeliveryFee = (model.DeliveryType == "Delivery") ? 5.00m : 0.00m;
            model.TotalPrice = model.BaseTotalPrice + model.DeliveryFee;

            // 将更新后的完整数据重新存入 TempData
            TempData["PaymentData"] = JsonSerializer.Serialize(model);
            TempData.Keep("PaymentData");

            return View(model);
        }

        // 7. 支付成功处理 (POST) - 修复重点
        [HttpPost]
        public IActionResult PaymentSuccess(string PaymentMethod)
        {
            var userName = HttpContext.Session.GetString("UserName");
            if (TempData["PaymentData"] == null) return RedirectToAction("Index");

            // 获取包含最新 Delivery 信息的支付数据
            var paymentData = JsonSerializer.Deserialize<InvoiceViewModel>(TempData["PaymentData"].ToString());

            var cartItems = _context.Orders.Where(o => o.CustomerName == userName && o.Status == "InCart").ToList();
            if (!cartItems.Any()) return RedirectToAction("Index");

            string finalOrderId = "ORD-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");

            foreach (var item in cartItems)
            {
                item.Status = "Paid";
                item.OrderId = finalOrderId;

                // 核心修复：将 Delivery 记录与订单主键 item.Id 准确绑定
                var delivery = new Delivery
                {
                    OrderId = item.Id,
                    DeliveryType = paymentData.DeliveryType,
                    Address = paymentData.DeliveryAddress,
                    DeliveryStatus = "Preparing"
                };
                _context.Deliveries.Add(delivery);
            }
            _context.SaveChanges();

            // 构建收据显示模型
            var receipt = new ReceiptViewModel
            {
                OrderId = finalOrderId,
                TransactionDate = DateTime.Now,
                CustomerName = userName,
                ItemName = string.Join(", ", cartItems.Select(i => i.ItemName)),
                Quantity = cartItems.Sum(i => i.Quantity),
                TotalPrice = paymentData.TotalPrice,
                PaymentMethod = PaymentMethod,
                DeliveryType = paymentData.DeliveryType,
                DeliveryAddress = paymentData.DeliveryAddress,
                Packing = "Standard"
            };
            TempData["ReceiptData"] = JsonSerializer.Serialize(receipt);
            return RedirectToAction("PaymentSuccess");
        }

        // 8. 收据显示 (GET)
        [HttpGet]
        public IActionResult PaymentSuccess()
        {
            if (TempData["ReceiptData"] is string data)
            {
                var model = JsonSerializer.Deserialize<ReceiptViewModel>(data);
                return View(model);
            }
            return RedirectToAction("Index");
        }

        // 9. 订单历史 (带关联查询)
        public IActionResult BookingHistory()
        {

            var userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userName)) return RedirectToAction("Index", "Login");

            // 使用左外连接确保即使没有 Delivery 记录也能显示订单
            var history = (from o in _context.Orders
                           join d in _context.Deliveries on o.Id equals d.OrderId into deliveryGroup
                           from d in deliveryGroup.DefaultIfEmpty()
                           where o.CustomerName == userName && o.Status == "Paid"
                           orderby o.OrderTime descending
                           select new BookingHistoryViewModel
                           {
                               OrderId = o.OrderId,
                               OrderTime = o.OrderTime,
                               ItemName = o.ItemName,
                               Size = o.Size,
                               Sugar = o.Sugar,
                               Ice = o.Ice,
                               Milk = o.Milk,
                               Quantity = o.Quantity,
                               TotalPrice = o.TotalPrice,
                               Status = o.Status,
                               DeliveryType = d != null ? d.DeliveryType : "Dine In", // 若关联失败则默认 Dine In
                               Address = d != null ? d.Address : "N/A"
                           }).ToList();
            return View(history);
        }

        // 10. 个人资料
        public IActionResult Profile()
        {
            // 核心修复：从 Session 获取唯一 Email
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Index", "Login");
            }

            // 使用 Email 进行精准查询
            var customer = _context.Customers.FirstOrDefault(c => c.Email == userEmail);

            return (customer == null) ? RedirectToAction("Index") : View(customer);
        }
        [HttpPost]
        public IActionResult UpdateProfile(Customer model)
        {
            // 1. 從 Session 獲取當前登入用戶的 Email
            var currentEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(currentEmail))
            {
                return RedirectToAction("Index", "Login");
            }

            // 2. 獲取資料庫中該用戶的原始記錄
            var customerInDb = _context.Customers.FirstOrDefault(c => c.Email == currentEmail);
            if (customerInDb == null) return RedirectToAction("Index");

            // 3. 檢查 Name 是否被其他人佔用
            // 邏輯：名字相同，但 ID 不是我自己的
            bool nameExists = _context.Customers.Any(c => c.Name == model.Name && c.Id != customerInDb.Id);
            if (nameExists)
            {
                TempData["ErrorMessage"] = "This Name is already taken by another user.";
                return View("Profile", customerInDb); // 返回頁面並帶回舊資料
            }

            // 4. 檢查 Email 是否被其他人佔用
            // 邏輯：Email 相同，但 ID 不是我自己的
            bool emailExists = _context.Customers.Any(c => c.Email == model.Email && c.Id != customerInDb.Id);
            if (emailExists)
            {
                TempData["ErrorMessage"] = "This Email is already registered by another user.";
                return View("Profile", customerInDb);
            }

            // 5. 更新資料
            customerInDb.Name = model.Name;
            customerInDb.Email = model.Email;
            customerInDb.Phone = model.Phone;

            _context.SaveChanges();

            // 6. 如果 Email 改了，記得同步更新 Session
            HttpContext.Session.SetString("UserEmail", model.Email);
            HttpContext.Session.SetString("UserName", model.Name);

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }
    }
}