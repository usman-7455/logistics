using logistics.Data;
using logistics.Models;
using logistics.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace logistics.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        // Constructor Injection (Industry Standard for Dependency Injection)
        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductViewModel>> GetAllProductsAsync()
        {
            // We project the Entity directly to the ViewModel using LINQ.
            // This is highly efficient because it only fetches the columns we need from SQL Server.
            return await _context.Products
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity
                    // Note: IsLowStock is calculated automatically in the ViewModel 
                    // based on StockQuantity < 5, satisfying the PDF requirement.
                })
                .ToListAsync();
        }

        public async Task<bool> CreateProductAsync(CreateProductViewModel model)
        {
            // Map the ViewModel back to the Domain Entity
            var product = new Product
            {
                Name = model.Name,
                Price = model.Price,
                StockQuantity = model.StockQuantity
            };

            _context.Products.Add(product);

            // Save changes to the database
            await _context.SaveChangesAsync();

            return true;
        }
    }
}