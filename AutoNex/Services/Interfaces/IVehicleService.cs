using AutoNex.DTOs;
using AutoNex.DTOs.Vehicles;

namespace AutoNex.Services.Interfaces;

public interface IVehicleService
{
    Task<PagedResponse<VehicleResponse>> GetAllAsync(string? search, int? page, int? pageSize, CancellationToken cancellationToken = default);
    Task<VehicleResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<VehicleResponse> CreateAsync(CreateVehicleRequest request, CancellationToken cancellationToken = default);
    Task<VehicleResponse?> UpdateAsync(int id, UpdateVehicleRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
