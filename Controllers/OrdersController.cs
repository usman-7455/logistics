using System.Text.Json;
using logistics.Models.ViewModels;
using logistics.Services;
using Microsoft.AspNetCore.Mvc;

namespace logistics.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private const string CartSessionKey = "CartSessionKey";

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        // GET: /Orders
        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return View(orders);
        }
        // Helper methods to manage the Session Cart
        private List<CartLineItemViewModel> GetCart()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            return cartJson == null ? new List<CartLineItemViewModel>() : JsonSerializer.Deserialize<List<CartLineItemViewModel>>(cartJson);
        }

        private void SaveCart(List<CartLineItemViewModel> cart)
        {
            HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
        }

        // GET: /Orders/Create
        public async Task<IActionResult> Create()
        {
            var vm = new OrderCreateViewModel
            {
                CartItems = GetCart()
            };

            // Populate dropdowns
            ViewBag.Customers = await _orderService.GetCustomersAsync();
            ViewBag.Products = await _orderService.GetAvailableProductsAsync();

            return View(vm);
        }

        // POST: /Orders/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(AddToCartViewModel model)
        {
            if (ModelState.IsValid)
            {
                var products = await _orderService.GetAvailableProductsAsync();
                var product = products.FirstOrDefault(p => p.Id == model.ProductId);

                if (product != null)
                {
                    var cart = GetCart();
                    var existingItem = cart.FirstOrDefault(i => i.ProductId == model.ProductId);

                    if (existingItem != null)
                    {
                        existingItem.Quantity += model.Quantity;
                    }
                    else
                    {
                        cart.Add(new CartLineItemViewModel
                        {
                            ProductId = product.Id,
                            ProductName = product.Name,
                            Quantity = model.Quantity,
                            UnitPrice = product.Price
                        });
                    }
                    SaveCart(cart);
                }
            }
            return RedirectToAction(nameof(Create));
        }

        // POST: /Orders/RemoveFromCart
        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = GetCart();
            cart.RemoveAll(i => i.ProductId == productId);
            SaveCart(cart);
            return RedirectToAction(nameof(Create));
        }

        // POST: /Orders/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(OrderCreateViewModel model)
        {
            var cart = GetCart();
            if (cart == null || !cart.Any())
            {
                ModelState.AddModelError("", "Your cart is empty. Please add items before checking out.");
                return RedirectToAction(nameof(Create));
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Customers = await _orderService.GetCustomersAsync();
                ViewBag.Products = await _orderService.GetAvailableProductsAsync();
                model.CartItems = cart;
                return View("Create", model);
            }

            // Call the service with the atomic transaction
            var (success, message) = await _orderService.CreateOrderAsync(model.CustomerId, cart);

            if (success)
            {
                HttpContext.Session.Remove(CartSessionKey); // Clear cart
                TempData["SuccessMessage"] = message;
                return RedirectToAction("Index", "Products");
            }
            else
            {
                // Display the stock error message from the service
                ModelState.AddModelError("", message);
                ViewBag.Customers = await _orderService.GetCustomersAsync();
                ViewBag.Products = await _orderService.GetAvailableProductsAsync();
                model.CartItems = cart;
                return View("Create", model);
            }
        }
    }
}