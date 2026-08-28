using logistics.Data;
using logistics.Models;
using logistics.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace logistics.Services
{
    public class ShipmentService : IShipmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly Random _random = new Random();

        public ShipmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- NEW: Search and Filter Method ---
        public async Task<(List<Shipment> Shipments, int TotalCount)> GetShipmentsAsync(
            string searchString = null,
            string statusFilter = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var query = _context.Shipments
                .Include(s => s.Order)
                .ThenInclude(o => o.Customer)
                .AsQueryable();

            // Search by Tracking Code
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.TrackingCode != null && s.TrackingCode.Contains(searchString));
            }

            // Filter by Status
            if (!string.IsNullOrEmpty(statusFilter))
            {
                var status = Enum.Parse<ShipmentStatus>(statusFilter);
                query = query.Where(s => s.ShipmentStatus == status);
            }

            var totalCount = await query.CountAsync();

            var shipments = await query
                .OrderByDescending(s => s.Order.OrderDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (shipments, totalCount);
        }
        // ---------------------------------------

        public async Task<List<PendingShipmentViewModel>> GetPendingShipmentsAsync()
        {
            return await _context.Shipments
                .Include(s => s.Order)
                .ThenInclude(o => o.Customer)
                .Where(s => s.ShipmentStatus == ShipmentStatus.InTransit)
                .Select(s => new PendingShipmentViewModel
                {
                    ShipmentId = s.Id,
                    OrderId = s.OrderId,
                    CustomerName = s.Order.Customer.FullName,
                    OrderDate = s.Order.OrderDate,
                    TotalAmount = s.Order.TotalAmount,
                    CurrentStatus = s.ShipmentStatus.ToString()
                })
                .ToListAsync();
        }

        public async Task<bool> AssignDriverAsync(AssignDriverViewModel model)
        {
            var shipment = await _context.Shipments.FindAsync(model.ShipmentId);
            if (shipment == null) return false;

            string datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            int randomPart = _random.Next(1000, 9999);
            shipment.TrackingCode = $"TRK-{datePart}-{randomPart}";

            shipment.DriverName = model.DriverName;
            shipment.EstimatedDeliveryTime = model.EstimatedDeliveryTime;
            shipment.ShipmentStatus = ShipmentStatus.OutForDelivery;

            var order = await _context.Orders.FindAsync(shipment.OrderId);
            if (order != null)
            {
                order.Status = OrderStatus.Shipped;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<TrackingResultViewModel> GetTrackingInfoAsync(string trackingCode)
        {
            var shipment = await _context.Shipments
                .Include(s => s.Order)
                .ThenInclude(o => o.Customer)
                .Include(s => s.Order.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(s => s.TrackingCode == trackingCode);

            if (shipment == null) return null;

            return new TrackingResultViewModel
            {
                TrackingCode = shipment.TrackingCode ?? "Not Generated Yet",
                Status = shipment.ShipmentStatus.ToString(),
                DriverName = shipment.DriverName ?? "Not Assigned",
                EstimatedDeliveryTime = shipment.EstimatedDeliveryTime,
                CustomerName = shipment.Order?.Customer?.FullName ?? "Unknown",
                CustomerEmail = shipment.Order?.Customer?.Email ?? "Unknown",
                OrderItemsSummary = shipment.Order?.OrderItems?
                    .Select(oi => $"{oi.Quantity}x {oi.Product?.Name ?? "Unknown Product"}")
                    .ToList() ?? new List<string>()
            };
        }

        public async Task<bool> MarkAsDeliveredAsync(string trackingCode)
        {
            var shipment = await _context.Shipments
                .Include(s => s.Order)
                .FirstOrDefaultAsync(s => s.TrackingCode == trackingCode);

            if (shipment == null || shipment.ShipmentStatus == ShipmentStatus.Delivered)
            {
                return false;
            }

            shipment.ShipmentStatus = ShipmentStatus.Delivered;

            if (shipment.Order != null)
            {
                shipment.Order.Status = OrderStatus.Delivered;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}