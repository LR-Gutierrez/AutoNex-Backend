using AutoNex.DTOs;
using AutoNex.DTOs.Services;

namespace AutoNex.Services.Interfaces;

public interface IServiceCatalogService
{
    Task<PagedResponse<ServiceResponse>> GetAllAsync(int? page, int? pageSize);
    Task<ServiceResponse?> GetByIdAsync(int id);
    Task<ServiceResponse> CreateAsync(CreateServiceRequest request);
    Task<ServiceResponse?> UpdateAsync(int id, UpdateServiceRequest request);
    Task<bool> DeleteAsync(int id);

    // Service Variants
    Task<List<ServiceVariantResponse>> GetVariantsAsync(int serviceId);
    Task<ServiceVariantResponse?> GetVariantByIdAsync(int id);
    Task<ServiceVariantResponse> CreateVariantAsync(int serviceId, CreateServiceVariantRequest request);
    Task<ServiceVariantResponse?> UpdateVariantAsync(int id, UpdateServiceVariantRequest request);
    Task<bool> DeleteVariantAsync(int id);
}
