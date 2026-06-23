using AutoNex.DTOs;
using AutoNex.DTOs.Consumables;

namespace AutoNex.Services.Interfaces;

public interface IConsumableService
{
    Task<PagedResponse<ConsumableResponse>> GetAllAsync(string? search, string? category, int? page, int? pageSize, CancellationToken cancellationToken = default);
    Task<List<ConsumableResponse>> GetLowStockAsync(CancellationToken cancellationToken = default);
    Task<ConsumableResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ConsumableResponse> CreateAsync(CreateConsumableRequest request, CancellationToken cancellationToken = default);
    Task<ConsumableResponse?> UpdateAsync(int id, UpdateConsumableRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
