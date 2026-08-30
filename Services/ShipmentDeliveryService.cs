using logistics.Data;
using logistics.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace logistics.Services.Background
{
    public class ShipmentDeliveryService : BackgroundService
    {
        private readonly ILogger<ShipmentDeliveryService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); 

        public ShipmentDeliveryService(ILogger<ShipmentDeliveryService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(" Shipment Delivery Auto-Complete Service is starting...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation(" Checking for shipments to auto-complete...");
                    await ProcessCompletedShipmentsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, " Error occurred while processing completed shipments");
                }

               
                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation(" Shipment Delivery Auto-Complete Service is stopping.");
        }

        private async Task ProcessCompletedShipmentsAsync(CancellationToken stoppingToken)
        {
            // Create a scoped service provider to access DbContext
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Find all shipments that are OutForDelivery AND estimated time has passed
            var currentTime = DateTime.Now;

            var shipmentsToComplete = await context.Shipments
                .Include(s => s.Order)
                .Where(s =>
                    s.ShipmentStatus == ShipmentStatus.OutForDelivery &&
                    s.EstimatedDeliveryTime <= currentTime)
                .ToListAsync(stoppingToken);

            if (!shipmentsToComplete.Any())
            {
                _logger.LogInformation(" No shipments ready for auto-completion at {Time}", currentTime);
                return;
            }

            _logger.LogInformation(" Found {Count} shipments to mark as delivered", shipmentsToComplete.Count);

            foreach (var shipment in shipmentsToComplete)
            {
                try
                {
                    // Mark shipment as delivered
                    shipment.ShipmentStatus = ShipmentStatus.Delivered;

                    // Update the related order status
                    if (shipment.Order != null)
                    {
                        shipment.Order.Status = OrderStatus.Delivered;
                        _logger.LogInformation(" Order #{OrderId} marked as Delivered", shipment.OrderId);
                    }

                    _logger.LogInformation("Shipment {TrackingCode} auto-marked as Delivered (was due at {ETA})",
                        shipment.TrackingCode ?? $"#{shipment.Id}",
                        shipment.EstimatedDeliveryTime);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, " Failed to auto-complete shipment {ShipmentId}", shipment.Id);
                }
            }

            // Save all changes to the database
            var recordsAffected = await context.SaveChangesAsync(stoppingToken);
            _logger.LogInformation(" Successfully auto-completed {Count} shipments ({Records} records updated)",
                shipmentsToComplete.Count, recordsAffected);
        }
    }
}