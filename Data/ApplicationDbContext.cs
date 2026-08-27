using logistics.Models; // Updated to match your project name
using Microsoft.EntityFrameworkCore;

namespace logistics.Data
{
    public class ApplicationDbContext : DbContext
    {
        // The constructor is required for Dependency Injection
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Define a DbSet for each of your models. 
        // These will become the tables in your SQL Server database.
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Shipment> Shipments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Explicitly define decimal precision to avoid warnings (18 digits total, 2 decimal places)
            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(p => p.Price).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(o => o.TotalAmount).HasPrecision(18, 2);
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.Property(oi => oi.UnitPrice).HasPrecision(18, 2);
            });

            // Define the cascade delete for OrderItems
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}