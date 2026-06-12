using AutoNex.DTOs;
using AutoNex.DTOs.Vehicles;

namespace AutoNex.Services.Interfaces;

public interface IVehicleService
{
    Task<PagedResponse<VehicleResponse>> GetAllAsync(string? search, int? page, int? pageSize);
    Task<VehicleResponse?> GetByIdAsync(int id);
    Task<VehicleResponse> CreateAsync(CreateVehicleRequest request);
    Task<VehicleResponse?> UpdateAsync(int id, UpdateVehicleRequest request);
    Task<bool> DeleteAsync(int id);
}
