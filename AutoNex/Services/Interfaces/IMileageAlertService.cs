using AutoNex.DTOs;
using AutoNex.DTOs.MileageAlerts;

namespace AutoNex.Services.Interfaces;

public interface IMileageAlertService
{
    Task<PagedResponse<MileageAlertResponse>> GetAllAsync(bool? due, int? page, int? pageSize, CancellationToken cancellationToken = default);
    Task<MileageAlertResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<MileageAlertResponse> CreateAsync(CreateMileageAlertRequest request, CancellationToken cancellationToken = default);
    Task<MileageAlertResponse?> UpdateAsync(int id, UpdateMileageAlertRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<MileageAlertResponse?> AttendAsync(int id, CancellationToken cancellationToken = default);
    Task<List<MileageAlertResponse>> CreateOrUpdateFromOrderAsync(int orderId, CancellationToken cancellationToken = default);
}
