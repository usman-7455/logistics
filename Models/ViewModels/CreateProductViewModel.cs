using System.ComponentModel.DataAnnotations;

namespace logistics.Models.ViewModels
{
    public class CreateProductViewModel
    {
        [Required(ErrorMessage = "Product Name is required")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Range(0.01, 10000.00)]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }
    }
}