using AutoNex.DTOs;
using AutoNex.DTOs.Services;

namespace AutoNex.Services.Interfaces;

public interface IServiceCatalogService
{
    Task<PagedResponse<ServiceResponse>> GetAllAsync(int? page, int? pageSize, CancellationToken cancellationToken = default);
    Task<ServiceResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ServiceResponse> CreateAsync(CreateServiceRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResponse?> UpdateAsync(int id, UpdateServiceRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
