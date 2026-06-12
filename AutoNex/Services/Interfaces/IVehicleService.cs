using AutoNex.DTOs.Vehicles;

namespace AutoNex.Services.Interfaces;

public interface IVehicleService
{
    Task<List<VehicleResponse>> GetAllAsync(string? search);
    Task<VehicleResponse?> GetByIdAsync(int id);
    Task<VehicleResponse> CreateAsync(CreateVehicleRequest request);
    Task<VehicleResponse?> UpdateAsync(int id, UpdateVehicleRequest request);
    Task<bool> DeleteAsync(int id);
}
