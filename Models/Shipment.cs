using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace logistics.Models
{

    [Index(nameof(TrackingCode), IsUnique = true)]
    [Index(nameof(OrderId))]
    [Index(nameof(ShipmentStatus))]
    public class Shipment
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }
        public Order Order { get; set; } // Navigation property

        public string? TrackingCode { get; set; } // Will be auto-generated later

        public string? DriverName { get; set; }

        public ShipmentStatus ShipmentStatus { get; set; } = ShipmentStatus.InTransit;

        public DateTime? EstimatedDeliveryTime { get; set; }
    }
}
