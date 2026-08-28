using logistics.Data;
using logistics.Models.ViewModels;
using logistics.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace logistics.Controllers
{
    public class ShipmentsController : Controller
    {
        private readonly IShipmentService _shipmentService;
        private readonly ApplicationDbContext _context;

        public ShipmentsController(IShipmentService shipmentService, ApplicationDbContext context)
        {
            _shipmentService = shipmentService;
            _context = context;
        }

        // GET: /Shipments (List pending shipments with Search)
        public async Task<IActionResult> Index(string searchString = null)
        {
            var shipments = await _shipmentService.GetPendingShipmentsAsync();

            if (!string.IsNullOrEmpty(searchString))
            {
                shipments = shipments.Where(s =>
                    s.CustomerName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    s.OrderId.ToString().Contains(searchString)
                ).ToList();
            }

            return View(shipments);
        }

        // GET: /Shipments/SearchShipments (Dynamic AJAX Endpoint)
        public async Task<IActionResult> SearchShipments(string searchString)
        {
            var shipments = await _shipmentService.GetPendingShipmentsAsync();
            ViewData["CurrentFilter"] = searchString;

            if (!string.IsNullOrEmpty(searchString))
            {
                var cleanSearch = searchString.Replace(" ", "").ToLower();

                shipments = shipments.Where(s =>
                    s.CustomerName.Replace(" ", "").ToLower().StartsWith(cleanSearch) ||
                    s.OrderId.ToString().StartsWith(cleanSearch)
                ).ToList();
            }

            return PartialView("_PendingShipmentTable", shipments);
        }

        // GET: /Shipments/AssignDriver/5
        public async Task<IActionResult> AssignDriver(int id)
        {
            // Fetch the shipment and its related order to get the actual OrderDate
            var shipment = await _context.Shipments
                .Include(s => s.Order)
                .FirstOrDefaultAsync(s => s.Id == id);

            // Calculate minimum delivery date: Order Date + 1 Day. 
            // Fallback to tomorrow if shipment/order isn't found for safety.
            var minDate = shipment?.Order?.OrderDate.AddDays(1) ?? DateTime.Now.AddDays(1);

            var model = new AssignDriverViewModel
            {
                ShipmentId = id,
                MinDeliveryDate = minDate,
                EstimatedDeliveryTime = minDate // Sets a logical default value so the picker isn't empty
            };

            return View(model);
        }

        // POST: /Shipments/AssignDriver
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDriver(AssignDriverViewModel model)
        {
            if (ModelState.IsValid)
            {
                var success = await _shipmentService.AssignDriverAsync(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "Driver assigned successfully! Tracking code generated.";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(model);
        }

        // GET: /Shipments/Track 
        public async Task<IActionResult> Track(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return View();
            }

            var result = await _shipmentService.GetTrackingInfoAsync(code);

            if (result == null)
            {
                ViewBag.Error = "Tracking code not found. Please check the code and try again.";
                return View();
            }

            return View(result);
        }

        // POST: /Shipments/MarkAsDelivered
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsDelivered(string code)
        {
            if (!string.IsNullOrWhiteSpace(code))
            {
                var success = await _shipmentService.MarkAsDeliveredAsync(code);
                if (success)
                {
                    TempData["SuccessMessage"] = "Shipment marked as Delivered successfully!";
                }
            }
            return RedirectToAction(nameof(Track), new { code = code });
        }
    }
}