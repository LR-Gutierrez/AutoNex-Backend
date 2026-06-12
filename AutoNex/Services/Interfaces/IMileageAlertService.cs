using AutoNex.DTOs;
using AutoNex.DTOs.MileageAlerts;

namespace AutoNex.Services.Interfaces;

public interface IMileageAlertService
{
    Task<PagedResponse<MileageAlertResponse>> GetAllAsync(bool? due, int? page, int? pageSize);
    Task<MileageAlertResponse?> GetByIdAsync(int id);
    Task<MileageAlertResponse> CreateAsync(CreateMileageAlertRequest request);
    Task<MileageAlertResponse?> UpdateAsync(int id, UpdateMileageAlertRequest request);
    Task<bool> DeleteAsync(int id);
    Task UpdateAlertFromServiceOrderAsync(int vehicleId, int currentKm, List<int> orderItemIds);
}
