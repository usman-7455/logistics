using logistics.Models;
using logistics.Models.ViewModels;

namespace logistics.Services
{
    public interface IShipmentService
    {
        Task<List<PendingShipmentViewModel>> GetPendingShipmentsAsync();
        Task<bool> AssignDriverAsync(AssignDriverViewModel model);
        Task<TrackingResultViewModel> GetTrackingInfoAsync(string trackingCode);
        Task<bool> MarkAsDeliveredAsync(string trackingCode);


        Task<(List<Shipment> Shipments, int TotalCount)> GetShipmentsAsync(
    string searchString = null,
    string statusFilter = null,
    int pageNumber = 1,
    int pageSize = 10);
    }
}