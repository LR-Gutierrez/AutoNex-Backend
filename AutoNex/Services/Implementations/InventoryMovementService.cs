using AutoNex.Data;
using AutoNex.DTOs.InventoryMovements;
using AutoNex.Helpers;
using AutoNex.Models;
using AutoNex.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoNex.Services.Implementations;

public class InventoryMovementService : IInventoryMovementService
{
    private readonly AppDbContext _context;

    public InventoryMovementService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<InventoryMovementResponse>> GetAllAsync(int? consumableId, int? toolId, int? page, int? pageSize)
    {
        var query = _context.InventoryMovements
            .Include(m => m.Consumable)
            .Include(m => m.Tool)
            .AsQueryable();

        if (consumableId.HasValue)
            query = query.Where(m => m.ConsumableId == consumableId.Value);
        if (toolId.HasValue)
            query = query.Where(m => m.ToolId == toolId.Value);

        var movements = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip(((page ?? 1) - 1) * (pageSize ?? 20))
            .Take(pageSize ?? 20)
            .ToListAsync();

        return movements.Select(m => m.ToResponse()).ToList();
    }

    public async Task<InventoryMovementResponse?> GetByIdAsync(int id)
    {
        var movement = await _context.InventoryMovements
            .Include(m => m.Consumable)
            .Include(m => m.Tool)
            .FirstOrDefaultAsync(m => m.Id == id);

        return movement?.ToResponse();
    }
}
