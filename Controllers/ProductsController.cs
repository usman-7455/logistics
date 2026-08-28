using logistics.Models.ViewModels;
using logistics.Services;
using Microsoft.AspNetCore.Mvc;

namespace logistics.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;

        // Inject the service (Dependency Injection)
        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: /Products (Updated with Search and Pagination)
        public async Task<IActionResult> Index(string searchString, int pageNumber = 1)
        {
            int pageSize = 10; // Show 10 products per page

            // Call the new method and deconstruct the tuple (products list, total count)
            var (products, totalCount) = await _productService.GetProductsAsync(searchString, pageNumber, pageSize);

            // Pass data to the View for the search box and pagination buttons
            ViewData["CurrentFilter"] = searchString;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewData["CurrentPage"] = pageNumber;

            return View(products);
        }

        // GET: /Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken] // Security best practice
        public async Task<IActionResult> Create(CreateProductViewModel model)
        {
            // Server-side validation check
            if (ModelState.IsValid)
            {
                await _productService.CreateProductAsync(model);
                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
    }
}