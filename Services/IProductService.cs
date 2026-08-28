using logistics.Models;
using logistics.Models.ViewModels;

namespace logistics.Services
{
    public interface IProductService
    {
        
        Task<(List<Product> Products, int TotalCount)> GetProductsAsync(
            string searchString = null,
            int pageNumber = 1,
            int pageSize = 10);

        
        Task<bool> CreateProductAsync(CreateProductViewModel model);
    }
}