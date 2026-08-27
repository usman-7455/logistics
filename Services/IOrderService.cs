using logistics.Models;
using logistics.Models.ViewModels;

namespace logistics.Services
{
    public interface IOrderService
    {
        // For dropdowns
        Task<List<Customer>> GetCustomersAsync();
        Task<List<Product>> GetAvailableProductsAsync();

        // The core checkout logic
        // Returns a tuple: (Success, ErrorMessage)
        Task<(bool Success, string Message)> CreateOrderAsync(int customerId, List<CartLineItemViewModel> cartItems);
        Task<List<OrderSummaryViewModel>> GetAllOrdersAsync();
    }
}