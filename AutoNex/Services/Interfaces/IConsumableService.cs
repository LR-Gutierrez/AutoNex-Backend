using AutoNex.DTOs.Consumables;

namespace AutoNex.Services.Interfaces;

public interface IConsumableService
{
    Task<List<ConsumableResponse>> GetAllAsync(string? category);
    Task<List<ConsumableResponse>> GetLowStockAsync();
    Task<ConsumableResponse?> GetByIdAsync(int id);
    Task<ConsumableResponse> CreateAsync(CreateConsumableRequest request);
    Task<ConsumableResponse?> UpdateAsync(int id, UpdateConsumableRequest request);
    Task<bool> DeleteAsync(int id);
}
