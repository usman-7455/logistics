using logistics.Models;
using Microsoft.EntityFrameworkCore;

namespace logistics.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            
            await context.Database.MigrateAsync();

            
            if (!await context.Customers.AnyAsync())
            {
                var customers = new List<Customer>
                {
                    new Customer { FullName = "John Doe", Email = "john.doe@example.com", PhoneNumber = "123-456-7890", CreatedAt = DateTime.UtcNow },
                    new Customer { FullName = "Jane Smith", Email = "jane.smith@example.com", PhoneNumber = "987-654-3210", CreatedAt = DateTime.UtcNow },
                    new Customer { FullName = "Alice Johnson", Email = "alice.j@example.com", PhoneNumber = "555-019-2837", CreatedAt = DateTime.UtcNow }
                };

                context.Customers.AddRange(customers);
                await context.SaveChangesAsync();
            }

           
            if (!await context.Products.AnyAsync())
            {
                var products = new List<Product>
                {
                    new Product { Name = "Wireless Mouse", Price = 25.99m, StockQuantity = 50 },
                    new Product { Name = "Mechanical Keyboard", Price = 89.50m, StockQuantity = 3 }, // Low stock!
                    new Product { Name = "HD Monitor 24\"", Price = 150.00m, StockQuantity = 12 },
                    new Product { Name = "USB-C Hub", Price = 35.00m, StockQuantity = 2 }, // Low stock!
                    new Product { Name = "Webcam 1080p", Price = 60.00m, StockQuantity = 15 }
                };

                context.Products.AddRange(products);
                await context.SaveChangesAsync();
            }
        }
    }
}