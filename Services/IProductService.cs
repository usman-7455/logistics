using logistics.Models;
using logistics.Models.ViewModels;

namespace logistics.Services
{
    public interface IProductService
    {
        // Updated to support search and pagination (returns a tuple of list and total count)
        Task<(List<Product> Products, int TotalCount)> GetProductsAsync(
            string searchString = null,
            int pageNumber = 1,
            int pageSize = 10);

        // Fixed the name to match standard conventions (removed "All")
        Task<bool> CreateProductAsync(CreateProductViewModel model);
    }
}