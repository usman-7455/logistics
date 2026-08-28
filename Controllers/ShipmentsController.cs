using logistics.Models.ViewModels;
using logistics.Services;
using Microsoft.AspNetCore.Mvc;

namespace logistics.Controllers
{
    public class ShipmentsController : Controller
    {
        private readonly IShipmentService _shipmentService;

        public ShipmentsController(IShipmentService shipmentService)
        {
            _shipmentService = shipmentService;
        }

        // GET: /Shipments (List pending shipments with Search)
        public async Task<IActionResult> Index(string searchString = null)
        {
            // 1. Fetch the pending shipments (Returns List<PendingShipmentViewModel>)
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
                // Remove spaces and make lowercase for accurate "StartsWith" matching
                var cleanSearch = searchString.Replace(" ", "").ToLower();

                shipments = shipments.Where(s =>
                    s.CustomerName.Replace(" ", "").ToLower().StartsWith(cleanSearch) ||
                    s.OrderId.ToString().StartsWith(cleanSearch)
                ).ToList();
            }

            // Return only the partial view containing the table
            return PartialView("_PendingShipmentTable", shipments);
        }

        // GET: /Shipments/AssignDriver/5
        public IActionResult AssignDriver(int id)
        {
            
            return View(new AssignDriverViewModel { ShipmentId = id });
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