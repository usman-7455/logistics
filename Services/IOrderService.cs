using logistics.Models;
using logistics.Models.ViewModels;

namespace logistics.Services
{
    public interface IOrderService
    {
      
        Task<List<Customer>> GetCustomersAsync();
        Task<List<Product>> GetAvailableProductsAsync();

        //  checkout logic
        
        Task<(bool Success, string Message)> CreateOrderAsync(int customerId, List<CartLineItemViewModel> cartItems);
        Task<List<OrderSummaryViewModel>> GetAllOrdersAsync();
    }
}