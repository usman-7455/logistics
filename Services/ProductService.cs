using logistics.Data;
using logistics.Models;
using logistics.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace logistics.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

       
        public async Task<(List<Product> Products, int TotalCount)> GetProductsAsync(
            string searchString = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Name.Contains(searchString));
            }

            var totalCount = await query.CountAsync();

            var products = await query
                .OrderBy(p => p.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (products, totalCount);
        }

        
        public async Task<bool> CreateProductAsync(CreateProductViewModel model)
        {
            var product = new Product
            {
                Name = model.Name,
                Price = model.Price,
                StockQuantity = model.StockQuantity
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}