using System.ComponentModel.DataAnnotations;

namespace logistics.Models.ViewModels
{
    
    public class PendingShipmentViewModel
    {
        public int ShipmentId { get; set; }
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string CurrentStatus { get; set; }
    }

   
    public class AssignDriverViewModel
    {
        public int ShipmentId { get; set; }

        [Required(ErrorMessage = "Driver name is required")]
        public string DriverName { get; set; }

        [Required(ErrorMessage = "Estimated delivery time is required")]
        [DataType(DataType.DateTime)]
        public DateTime EstimatedDeliveryTime { get; set; }

        public DateTime MinDeliveryDate { get; set; }
    }

   
    public class TrackingResultViewModel
    {
        public string TrackingCode { get; set; }
        public string Status { get; set; }
        public string DriverName { get; set; }
        public DateTime? EstimatedDeliveryTime { get; set; }

        
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }

       
        public List<string> OrderItemsSummary { get; set; } = new List<string>();
    }
}