using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace logistics.Models
{


    [Index(nameof(CustomerId))]
    [Index(nameof(Status))]
    [Index(nameof(OrderDate))]
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } 

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

      
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}

