namespace logistics.Models.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        // Helper property for low stock highlighting (requirement: < 5)
        public bool IsLowStock => StockQuantity < 5;
    }
}