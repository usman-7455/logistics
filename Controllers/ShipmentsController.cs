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

        // GET: /Shipments (List pending shipments)
        public async Task<IActionResult> Index()
        {
            var shipments = await _shipmentService.GetPendingShipmentsAsync();
            return View(shipments);
        }

        // GET: /Shipments/AssignDriver/5
        public IActionResult AssignDriver(int id)
        {
            // Pre-fill the ShipmentId so the form knows which one to update
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
        // This single method handles BOTH the empty search form AND the search results
        public async Task<IActionResult> Track(string code)
        {
            // 1. If no code is provided, just show the empty search form (Model will be null)
            if (string.IsNullOrWhiteSpace(code))
            {
                return View();
            }

            // 2. If a code IS provided, search for it in the database
            var result = await _shipmentService.GetTrackingInfoAsync(code);

            // 3. If not found, show the form again with an error message
            if (result == null)
            {
                ViewBag.Error = "Tracking code not found. Please check the code and try again.";
                return View();
            }

            // 4. Success! Pass the result to the view to display the tracking details
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
            // Redirect back to the tracking page to show the updated status
            return RedirectToAction(nameof(Track), new { code = code });
        }
    }
}