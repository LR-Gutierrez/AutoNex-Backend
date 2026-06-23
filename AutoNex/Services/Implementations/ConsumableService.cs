using AutoNex.Data;
using AutoNex.DTOs;
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

    public async Task<PagedResponse<ConsumableResponse>> GetAllAsync(string? search, string? category, int? page, int? pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Consumables
            .AsNoTracking()
            .Include(c => c.Supplier)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Name.Contains(search));

        if (!string.IsNullOrWhiteSpace(category) && Enum.TryParse<ConsumableCategory>(category, true, out var cat))
            query = query.Where(c => c.Category == cat);

        query = query.OrderByDescending(c => c.CreatedAt);

        return await query.ToPagedResponseAsync(page, pageSize, c => c.ToResponse(), cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<ConsumableResponse>> GetLowStockAsync(CancellationToken cancellationToken = default)
    {
        var items = await _context.Consumables
            .AsNoTracking()
            .Include(c => c.Supplier)
            .Where(c => c.StockQuantity <= c.MinStock)
            .OrderBy(c => c.StockQuantity)
            .ToListAsync(cancellationToken);

        return items.Select(c => c.ToResponse()).ToList();
    }

    public async Task<ConsumableResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var consumable = await _context.Consumables
            .AsNoTracking()
            .Include(c => c.Supplier)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        return consumable?.ToResponse();
    }

    public async Task<ConsumableResponse> CreateAsync(CreateConsumableRequest request, CancellationToken cancellationToken = default)
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
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (await GetByIdAsync(consumable.Id, cancellationToken).ConfigureAwait(false))!;
    }

    public async Task<ConsumableResponse?> UpdateAsync(int id, UpdateConsumableRequest request, CancellationToken cancellationToken = default)
    {
        var consumable = await _context.Consumables.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
        if (consumable is null) return null;

        consumable.Name = request.Name;
        consumable.Category = request.Category;
        consumable.StockQuantity = request.StockQuantity;
        consumable.MinStock = request.MinStock;
        consumable.UnitPrice = request.UnitPrice;
        consumable.SupplierId = request.SupplierId;
        consumable.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return (await GetByIdAsync(id, cancellationToken).ConfigureAwait(false))!;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var consumable = await _context.Consumables.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
        if (consumable is null) return false;

        consumable.IsDeleted = true;
        consumable.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
