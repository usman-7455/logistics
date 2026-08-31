using logistics.Models.ViewModels;
using logistics.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace logistics.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;

        // (Dependency Injection)
        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: /Products (Updated with Search and Pagination)
        public async Task<IActionResult> Index(string searchString, int pageNumber = 1)
        {
            int pageSize = 10; 

            
            var (products, totalCount) = await _productService.GetProductsAsync(searchString, pageNumber, pageSize);

            
            ViewData["CurrentFilter"] = searchString;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewData["CurrentPage"] = pageNumber;

            return View(products);
        }
        public async Task<IActionResult> SearchProducts(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            // 1. Fetch a generous batch of products (e.g., 1000) to filter in memory.
            // This is lightning fast in C# and avoids complex SQL translation issues.
            var (products, _) = await _productService.GetProductsAsync(null, pageNumber: 1, pageSize: 1000);

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                // 2. Clean the search query: remove ALL spaces and special characters, keep only letters/numbers
                var cleanSearch = Regex.Replace(searchString.ToLower(), @"[^a-z0-9]", "");

                // 3. Filter in memory: clean the product name the same way, then check if it StartsWith the search
                products = products.Where(p =>
                {
                    var cleanName = Regex.Replace(p.Name.ToLower(), @"[^a-z0-9]", "");
                    return cleanName.StartsWith(cleanSearch);
                }).ToList();
            }

            // Return only the partial view containing the table
            return PartialView("_ProductTable", products);
        }


        // GET: /Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Create(CreateProductViewModel model)
        {
            // Server-side validation check
            if (ModelState.IsValid)
            {
                await _productService.CreateProductAsync(model);
                return RedirectToAction(nameof(Index));
            }

           
            return View(model);
        }
    }
}