using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    // === 数据库实体 (对应 Tables) ===
    public class MenuItem
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public class Order
    {
        public int Id { get; set; }
        public string? OrderId { get; set; }      // 加 ? 允许空值
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; } // 加 ?
        public DateTime OrderTime { get; set; }
        public string? Status { get; set; }       // 加 ?

        public string? ItemName { get; set; }     // 加 ?
        public string? ItemImageUrl { get; set; } // 加 ?
        public int Quantity { get; set; }

        public string? Size { get; set; }         // 加 ?
        public string? Ice { get; set; }          // 加 ?
        public string? Milk { get; set; }         // 加 ?
        public string? Sugar { get; set; }        // 加 ?
        public string? Notes { get; set; }        // 加 ?

        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        public ICollection<OrderPayment>? OrderPayments { get; set; }
    }
    public class Delivery
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string? DeliveryType { get; set; }
        public string? Address { get; set; }
        public string? DeliveryStatus { get; set; }
    }

    public class Payment
    {
        [Key]
        [Column("PaymentId")]
        public int Id { get; set; }
        public int? AdminId { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public DateTime PaymentDate { get; set; }

        public ICollection<OrderPayment>? OrderPayments { get; set; }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string? CustomerId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Phone { get; set; }
    }

    public class Admin
    {
        public int Id { get; set; }
        public string AdminId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public double Salary { get; set; }
        public string? Description { get; set; }

    }

    // === 视图模型 (用于页面传值) ===
    // 在 Entities.cs 中找到 InvoiceViewModel 并修改如下：
    public class InvoiceViewModel
    {
        public string? CustomerName { get; set; }
        public List<Order> CartItems { get; set; } = new List<Order>();
        public decimal BaseTotalPrice { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal TotalPrice { get; set; } // 确保这里是 TotalPrice 而不是 FinalTotal
        public string DeliveryType { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
    }

    public class ReceiptViewModel
    {
        public string OrderId { get; set; }
        public DateTime TransactionDate { get; set; } // 确保是 TransactionDate 而不是 OrderTime
        public string CustomerName { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public string Size { get; set; }
        public string Sugar { get; set; }
        public string Ice { get; set; }
        public string Milk { get; set; }
        public string Packing { get; set; }
        public string DeliveryType { get; set; }
        public string DeliveryAddress { get; set; }
        public decimal TotalPrice { get; set; }
        public string PaymentMethod { get; set; }
    }
    public class BookingHistoryViewModel
    {
        public string? OrderId { get; set; }
        public DateTime OrderTime { get; set; }
        public string? ItemName { get; set; }
        public string? Size { get; set; }
        public string? Sugar { get; set; }
        public string? Ice { get; set; }
        public string? Milk { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Status { get; set; }
        public string? DeliveryType { get; set; } // 来自 Deliveries 表 [cite: 1]
        public string? Address { get; set; }      // 来自 Deliveries 表 [cite: 1]
    }

    public class OrderPayment
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int PaymentId { get; set; }
        public Payment? Payment { get; set; }

        // Optional but VERY useful
        public decimal AppliedAmount { get; set; }
        public DateTime LinkedAt { get; set; }
    }


}