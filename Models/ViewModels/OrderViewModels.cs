using System.ComponentModel.DataAnnotations;

namespace logistics.Models.ViewModels
{
    // 1. The main ViewModel for the Create Order page
    public class OrderCreateViewModel
    {
        [Required(ErrorMessage = "Please select a customer")]
        public int CustomerId { get; set; }

        // This will hold the items in our temporary cart
        public List<CartLineItemViewModel> CartItems { get; set; } = new List<CartLineItemViewModel>();

        // Calculated total
        public decimal OrderTotal => CartItems.Sum(item => item.Subtotal);
    }

    // 2. Represents a single row in our temporary cart table
    public class CartLineItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        // Calculated on the fly
        public decimal Subtotal => Quantity * UnitPrice;
    }

    // 3. Used strictly for the "Add to Cart" form submission
    public class AddToCartViewModel
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Please enter a valid quantity")]
        public int Quantity { get; set; }
    }
    // For the All Orders summary list
    public class OrderSummaryViewModel
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public string TrackingCode { get; set; }
    }
}