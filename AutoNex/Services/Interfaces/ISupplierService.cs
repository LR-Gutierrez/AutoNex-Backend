using AutoNex.DTOs;
using AutoNex.DTOs.Suppliers;

namespace AutoNex.Services.Interfaces;

public interface ISupplierService
{
    Task<PagedResponse<SupplierResponse>> GetAllAsync(string? search, int? page, int? pageSize, CancellationToken cancellationToken = default);
    Task<SupplierResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<SupplierResponse> CreateAsync(CreateSupplierRequest request, CancellationToken cancellationToken = default);
    Task<SupplierResponse?> UpdateAsync(int id, UpdateSupplierRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
