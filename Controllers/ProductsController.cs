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

        // GET: /Products
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllProductsAsync();
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