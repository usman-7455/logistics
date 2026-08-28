using logistics.Data;
using logistics.Models;
using logistics.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace logistics.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Customer>> GetCustomersAsync()
        {
            return await _context.Customers
                .AsNoTracking() // Always fetch fresh data
                .OrderBy(c => c.FullName)
                .ToListAsync();
        }

        public async Task<List<Product>> GetAvailableProductsAsync()
        {
            return await _context.Products
                .AsNoTracking() // Always fetch fresh data
                .Where(p => p.StockQuantity > 0)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> CreateOrderAsync(int customerId, List<CartLineItemViewModel> cartItems)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Validate Stock Availability
                foreach (var item in cartItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null || product.StockQuantity < item.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return (false, $"Insufficient stock for {item.ProductName}. Available: {product?.StockQuantity ?? 0}");
                    }
                }

                // 2. Create the Order Entity
                var order = new Order
                {
                    CustomerId = customerId,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    TotalAmount = cartItems.Sum(i => i.Subtotal)
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // 3. Create Order Items and Deduct Stock
                foreach (var item in cartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice // Note: Ensure your model has UnitPrice, not Price
                    };
                    _context.OrderItems.Add(orderItem);

                    var product = await _context.Products.FindAsync(item.ProductId);
                    product.StockQuantity -= item.Quantity;
                }

                await _context.SaveChangesAsync(); // Save OrderItems and Stock changes

                // 4. Create the Pending Shipment
                var shipment = new Shipment
                {
                    OrderId = order.Id,
                    ShipmentStatus = ShipmentStatus.InTransit
                    // TrackingCode and DriverName are intentionally left null for now
                };

                _context.Shipments.Add(shipment);
                await _context.SaveChangesAsync(); // Save Shipment

                await transaction.CommitAsync();
                return (true, "Order created successfully!");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"An error occurred: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        public async Task<List<OrderSummaryViewModel>> GetAllOrdersAsync()
        {
            // MAGIC FIX: .AsNoTracking() forces EF Core to bypass its memory cache 
            // and always pull the absolute latest data from the SQL database.
            var orders = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var result = new List<OrderSummaryViewModel>();

            foreach (var order in orders)
            {
                // Also use AsNoTracking() here to ensure the shipment lookup is fresh
                var trackingCode = await _context.Shipments
                    .AsNoTracking()
                    .Where(s => s.OrderId == order.Id)
                    .Select(s => s.TrackingCode)
                    .FirstOrDefaultAsync();

                result.Add(new OrderSummaryViewModel
                {
                    OrderId = order.Id,
                    CustomerName = order.Customer?.FullName ?? "Unknown",
                    CustomerEmail = order.Customer?.Email ?? "Unknown",
                    TotalAmount = order.TotalAmount,
                    OrderDate = order.OrderDate,
                    Status = order.Status.ToString(),
                    TrackingCode = trackingCode
                });
            }

            return result;
        }
    }
}