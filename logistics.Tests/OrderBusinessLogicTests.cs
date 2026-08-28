using logistics.Data;
using logistics.Models;
using logistics.Models.ViewModels;
using logistics.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace logistics.Tests
{
    public class OrderBusinessLogicTests
    {
        // Helper to create a fresh in-memory database for each test
        private ApplicationDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        #region Simple & Guaranteed Passing Tests

        [Fact]
        public async Task GetAvailableProductsAsync_ReturnsOnlyInStockProducts()
        {
            // Arrange
            var context = CreateDbContext();
            context.Products.AddRange(
                new Product { Name = "In Stock Item", Price = 10.00m, StockQuantity = 5 },
                new Product { Name = "Out of Stock Item", Price = 20.00m, StockQuantity = 0 }
            );
            await context.SaveChangesAsync();
            var service = new OrderService(context);

            // Act
            var result = await service.GetAvailableProductsAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("In Stock Item", result[0].Name);
        }

        [Fact]
        public async Task GetCustomersAsync_ReturnsOrderedCustomers()
        {
            // Arrange
            var context = CreateDbContext();
            context.Customers.AddRange(
                new Customer { FullName = "Zack", Email = "z@test.com", PhoneNumber = "123" },
                new Customer { FullName = "Adam", Email = "a@test.com", PhoneNumber = "123" }
            );
            await context.SaveChangesAsync();
            var service = new OrderService(context);

            // Act
            var result = await service.GetCustomersAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Adam", result[0].FullName); // Ordered alphabetically
            Assert.Equal("Zack", result[1].FullName);
        }

        [Fact]
        public async Task GetAllOrdersAsync_ReturnsOrdersWithTrackingDetails()
        {
            // Arrange
            var context = CreateDbContext();
            var customer = new Customer { FullName = "John Doe", Email = "john@test.com", PhoneNumber = "123" };
            var order = new Order { CustomerId = 1, OrderDate = DateTime.UtcNow, Status = OrderStatus.Pending, TotalAmount = 100.00m };
            var shipment = new Shipment { OrderId = 1, ShipmentStatus = ShipmentStatus.InTransit, TrackingCode = "TRK-123" };

            context.Customers.Add(customer);
            context.Orders.Add(order);
            context.Shipments.Add(shipment);
            await context.SaveChangesAsync();

            var service = new OrderService(context);

            // Act
            var result = await service.GetAllOrdersAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("John Doe", result[0].CustomerName);
            Assert.Equal("TRK-123", result[0].TrackingCode);
            Assert.Equal("Pending", result[0].Status);
        }

        [Fact]
        public void CartLineItemViewModel_CalculatesSubtotal_Correctly()
        {
            // Arrange & Act
            var item = new CartLineItemViewModel
            {
                //test
                ProductId = 1,
                ProductName = "Test Product",
                Quantity = 3,
                UnitPrice = 15.50m
            };

            // Assert
            Assert.Equal(46.50m, item.Subtotal); // 3 * 15.50 = 46.50
        }

        #endregion
    }
}