using logistics.Models.ViewModels;

namespace logistics.Services
{
    public interface IShipmentService
    {
        Task<List<PendingShipmentViewModel>> GetPendingShipmentsAsync();
        Task<bool> AssignDriverAsync(AssignDriverViewModel model);
        Task<TrackingResultViewModel> GetTrackingInfoAsync(string trackingCode);
        Task<bool> MarkAsDeliveredAsync(string trackingCode);
    }
}