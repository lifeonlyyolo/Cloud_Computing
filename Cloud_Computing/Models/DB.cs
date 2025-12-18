using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models
{
    // 这个类继承自 DbContext，用于管理数据库连接
    public class DB : DbContext
    {
        public DB(DbContextOptions<DB> options) : base(options)

        {
        }

        // === 这里将你的 C# 模型映射到数据库的表格 ===
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<OrderPayment> OrderPayments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderPayment>()
                .HasIndex(op => new { op.OrderId, op.PaymentId })
                .IsUnique();

            modelBuilder.Entity<OrderPayment>()
                .HasOne(op => op.Order)
                .WithMany(o => o.OrderPayments)
                .HasForeignKey(op => op.OrderId);

            modelBuilder.Entity<OrderPayment>()
                .HasOne(op => op.Payment)
                .WithMany(p => p.OrderPayments)
                .HasForeignKey(op => op.PaymentId);
        }



    }


}