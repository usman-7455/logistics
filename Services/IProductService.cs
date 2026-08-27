using logistics.Models.ViewModels;

namespace logistics.Services
{
    public interface IProductService
    {
        Task<List<ProductViewModel>> GetAllProductsAsync();
        Task<bool> CreateProductAsync(CreateProductViewModel model);
    }
}