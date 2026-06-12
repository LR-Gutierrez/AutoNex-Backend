using AutoNex.DTOs.MileageAlerts;

namespace AutoNex.Services.Interfaces;

public interface IMileageAlertService
{
    Task<List<MileageAlertResponse>> GetAllAsync(bool? due);
    Task<MileageAlertResponse?> GetByIdAsync(int id);
    Task<MileageAlertResponse> CreateAsync(CreateMileageAlertRequest request);
    Task<MileageAlertResponse?> UpdateAsync(int id, UpdateMileageAlertRequest request);
    Task<bool> DeleteAsync(int id);
    Task UpdateAlertFromServiceOrderAsync(int vehicleId, int currentKm, List<int> orderItemIds);
}
