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

        public async Task<List<PendingShipmentViewModel>> GetPendingShipmentsAsync()
        {
            // Fetch shipments that are currently 'InTransit' (waiting for driver assignment/dispatch)
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

            // 1. Generate Tracking Code: TRK-YYYYMMDD-XXXX
            string datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            int randomPart = _random.Next(1000, 9999);
            shipment.TrackingCode = $"TRK-{datePart}-{randomPart}";

            // 2. Assign Driver and Update Status
            shipment.DriverName = model.DriverName;
            shipment.EstimatedDeliveryTime = model.EstimatedDeliveryTime;
            shipment.ShipmentStatus = ShipmentStatus.OutForDelivery; // Move to next stage

            // 3. Update the Order status to Shipped as well
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

                // Safe navigation to prevent NullReferenceException
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
                return false; // Already delivered or not found
            }

            // Update Shipment Status
            shipment.ShipmentStatus = ShipmentStatus.Delivered;

            // Update the related Order Status as well
            if (shipment.Order != null)
            {
                shipment.Order.Status = OrderStatus.Delivered;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}