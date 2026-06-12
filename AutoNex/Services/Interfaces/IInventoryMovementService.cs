using AutoNex.DTOs.InventoryMovements;

namespace AutoNex.Services.Interfaces;

public interface IInventoryMovementService
{
    Task<List<InventoryMovementResponse>> GetAllAsync(int? consumableId, int? toolId, int? page, int? pageSize);
    Task<InventoryMovementResponse?> GetByIdAsync(int id);
}
