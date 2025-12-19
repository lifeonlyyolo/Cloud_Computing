using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Text.Json;
using System;
using System.Collections.Generic;
using WebApplication1.Filters;
using WebApplication1.Models;


namespace WebApplication1.Controllers
{
    [TypeFilter(typeof(CustomerCheckFilter))]
    public class CustomerController : Controller
    {
        private readonly DB _context;

        public CustomerController(DB context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var menuList = _context.MenuItems.ToList();
            return View(menuList);
        }

        [HttpGet]
        public IActionResult Order(int itemId)
        {
            var item = _context.MenuItems.FirstOrDefault(m => m.Id == itemId);
            if (item == null) return RedirectToAction("Index");

            return View(item);
        }

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

        public IActionResult CartList()
        {
            var userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userName)) return RedirectToAction("Index", "Login");

            var cartItems = _context.Orders
                .Where(o => o.CustomerName == userName && o.Status == "InCart")
                .ToList();

            return View(cartItems);
        }


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


        public IActionResult Payment(string DeliveryType, string UserAddress)
        {
            if (TempData["PaymentData"] == null) return RedirectToAction("CartList");

            var model = JsonSerializer.Deserialize<InvoiceViewModel>(TempData["PaymentData"].ToString());

            model.DeliveryType = DeliveryType ?? "Dine In";
            model.DeliveryAddress = (model.DeliveryType == "Delivery") ? UserAddress : "Self-Pickup/Dine In";
            model.DeliveryFee = (model.DeliveryType == "Delivery") ? 5.00m : 0.00m;
            model.TotalPrice = model.BaseTotalPrice + model.DeliveryFee;

            TempData["PaymentData"] = JsonSerializer.Serialize(model);
            TempData.Keep("PaymentData");

            return View(model);
        }

        [HttpPost]
        public IActionResult PaymentSuccess(string PaymentMethod)
        {
            var userName = HttpContext.Session.GetString("UserName");
            if (TempData["PaymentData"] == null) return RedirectToAction("Index");

            var paymentData = JsonSerializer.Deserialize<InvoiceViewModel>(TempData["PaymentData"].ToString());

            var cartItems = _context.Orders.Where(o => o.CustomerName == userName && o.Status == "InCart").ToList();
            if (!cartItems.Any()) return RedirectToAction("Index");

            string finalOrderId = "ORD-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");

            foreach (var item in cartItems)
            {
                item.Status = "Paid";
                item.OrderId = finalOrderId;

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

        public IActionResult BookingHistory()
        {

            var userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userName)) return RedirectToAction("Index", "Login");

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
                               DeliveryType = d != null ? d.DeliveryType : "Dine In",
                               Address = d != null ? d.Address : "N/A"
                           }).ToList();
            return View(history);
        }

        public IActionResult Profile()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Index", "Login");
            }

            var customer = _context.Customers.FirstOrDefault(c => c.Email == userEmail);

            return (customer == null) ? RedirectToAction("Index") : View(customer);
        }
        [HttpPost]
        public IActionResult UpdateProfile(Customer model)
        {
            var currentEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(currentEmail))
            {
                return RedirectToAction("Index", "Login");
            }

            var customerInDb = _context.Customers.FirstOrDefault(c => c.Email == currentEmail);
            if (customerInDb == null) return RedirectToAction("Index");


            bool nameExists = _context.Customers.Any(c => c.Name == model.Name && c.Id != customerInDb.Id);
            if (nameExists)
            {
                TempData["ErrorMessage"] = "This Name is already taken by another user.";
                return View("Profile", customerInDb);
            }


            bool emailExists = _context.Customers.Any(c => c.Email == model.Email && c.Id != customerInDb.Id);
            if (emailExists)
            {
                TempData["ErrorMessage"] = "This Email is already registered by another user.";
                return View("Profile", customerInDb);
            }

            customerInDb.Name = model.Name;
            customerInDb.Email = model.Email;
            customerInDb.Phone = model.Phone;

            _context.SaveChanges();

            HttpContext.Session.SetString("UserEmail", model.Email);
            HttpContext.Session.SetString("UserName", model.Name);

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }
    }
}