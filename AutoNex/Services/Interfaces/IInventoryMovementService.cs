using AutoNex.DTOs;
using AutoNex.DTOs.InventoryMovements;

namespace AutoNex.Services.Interfaces;

public interface IInventoryMovementService
{
    Task<PagedResponse<InventoryMovementResponse>> GetAllAsync(int? consumableId, int? toolId, int? page, int? pageSize, CancellationToken cancellationToken = default);
    Task<InventoryMovementResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
