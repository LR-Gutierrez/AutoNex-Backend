using AutoNex.Data;
using AutoNex.DTOs;
using AutoNex.DTOs.Tools;
using AutoNex.Enums;
using AutoNex.Helpers;
using AutoNex.Models;
using AutoNex.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoNex.Services.Implementations;

public class ToolService : IToolService
{
    private readonly AppDbContext _context;

    public ToolService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<ToolResponse>> GetAllAsync(string? search, string? categoryName, string? status, int? page, int? pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Tools
            .AsNoTracking()
            .Include(t => t.ToolCategory)
            .Where(t => !t.ToolCategory.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => t.Name.Contains(search));

        if (!string.IsNullOrWhiteSpace(categoryName))
            query = query.Where(t => t.ToolCategory.Name.Contains(categoryName));

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ToolStatus>(status, true, out var st))
            query = query.Where(t => t.Status == st);

        query = query.OrderByDescending(t => t.CreatedAt);

        return await query.ToPagedResponseAsync(page, pageSize, t => t.ToResponse(), cancellationToken).ConfigureAwait(false);
    }

    public async Task<ToolResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var tool = await _context.Tools
            .AsNoTracking()
            .Include(t => t.ToolCategory)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        return tool?.ToResponse();
    }

    public async Task<ToolResponse> CreateAsync(CreateToolRequest request, CancellationToken cancellationToken = default)
    {
        var tool = new Tool
        {
            Name = request.Name,
            ToolCategoryId = request.ToolCategoryId,
            Quantity = request.Quantity,
            Status = request.Status,
            PurchaseDate = request.PurchaseDate
        };

        _context.Tools.Add(tool);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (await GetByIdAsync(tool.Id, cancellationToken).ConfigureAwait(false))!;
    }

    public async Task<ToolResponse?> UpdateAsync(int id, UpdateToolRequest request, CancellationToken cancellationToken = default)
    {
        var tool = await _context.Tools.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
        if (tool is null) return null;

        tool.Name = request.Name;
        tool.ToolCategoryId = request.ToolCategoryId;
        tool.Quantity = request.Quantity;
        tool.Status = request.Status;
        tool.PurchaseDate = request.PurchaseDate;
        tool.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return (await GetByIdAsync(id, cancellationToken).ConfigureAwait(false))!;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var tool = await _context.Tools.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
        if (tool is null) return false;

        tool.IsDeleted = true;
        tool.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
