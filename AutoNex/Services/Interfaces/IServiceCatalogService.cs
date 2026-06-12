using AutoNex.DTOs.Services;

namespace AutoNex.Services.Interfaces;

public interface IServiceCatalogService
{
    Task<List<ServiceResponse>> GetAllAsync();
    Task<ServiceResponse?> GetByIdAsync(int id);
    Task<ServiceResponse> CreateAsync(CreateServiceRequest request);
    Task<ServiceResponse?> UpdateAsync(int id, UpdateServiceRequest request);
    Task<bool> DeleteAsync(int id);
}
