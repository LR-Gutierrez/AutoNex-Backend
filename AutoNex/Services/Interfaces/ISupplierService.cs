using AutoNex.DTOs.Suppliers;

namespace AutoNex.Services.Interfaces;

public interface ISupplierService
{
    Task<List<SupplierResponse>> GetAllAsync();
    Task<SupplierResponse?> GetByIdAsync(int id);
    Task<SupplierResponse> CreateAsync(CreateSupplierRequest request);
    Task<SupplierResponse?> UpdateAsync(int id, UpdateSupplierRequest request);
    Task<bool> DeleteAsync(int id);
}
