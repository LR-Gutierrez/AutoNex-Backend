using AutoNex.Data;
using AutoNex.DTOs.Consumables;
using AutoNex.Enums;
using AutoNex.Helpers;
using AutoNex.Models;
using AutoNex.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoNex.Services.Implementations;

public class ConsumableService : IConsumableService
{
    private readonly AppDbContext _context;

    public ConsumableService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ConsumableResponse>> GetAllAsync(string? category)
    {
        var query = _context.Consumables
            .Include(c => c.Supplier)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(category) && Enum.TryParse<ConsumableCategory>(category, true, out var cat))
            query = query.Where(c => c.Category == cat);

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return items.Select(c => c.ToResponse()).ToList();
    }

    public async Task<List<ConsumableResponse>> GetLowStockAsync()
    {
        var items = await _context.Consumables
            .Include(c => c.Supplier)
            .Where(c => c.StockQuantity <= c.MinStock)
            .OrderBy(c => c.StockQuantity)
            .ToListAsync();

        return items.Select(c => c.ToResponse()).ToList();
    }

    public async Task<ConsumableResponse?> GetByIdAsync(int id)
    {
        var consumable = await _context.Consumables
            .Include(c => c.Supplier)
            .FirstOrDefaultAsync(c => c.Id == id);

        return consumable?.ToResponse();
    }

    public async Task<ConsumableResponse> CreateAsync(CreateConsumableRequest request)
    {
        var consumable = new Consumable
        {
            Name = request.Name,
            Category = request.Category,
            StockQuantity = request.StockQuantity,
            MinStock = request.MinStock,
            UnitPrice = request.UnitPrice,
            SupplierId = request.SupplierId
        };

        _context.Consumables.Add(consumable);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(consumable.Id))!;
    }

    public async Task<ConsumableResponse?> UpdateAsync(int id, UpdateConsumableRequest request)
    {
        var consumable = await _context.Consumables.FindAsync(id);
        if (consumable is null) return null;

        consumable.Name = request.Name;
        consumable.Category = request.Category;
        consumable.StockQuantity = request.StockQuantity;
        consumable.MinStock = request.MinStock;
        consumable.UnitPrice = request.UnitPrice;
        consumable.SupplierId = request.SupplierId;
        consumable.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (await GetByIdAsync(id))!;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var consumable = await _context.Consumables.FindAsync(id);
        if (consumable is null) return false;

        consumable.IsDeleted = true;
        consumable.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}
