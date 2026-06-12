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

    public async Task<PagedResponse<ToolResponse>> GetAllAsync(string? category, string? status, int? page, int? pageSize)
    {
        var query = _context.Tools.AsQueryable();

        if (!string.IsNullOrWhiteSpace(category) && Enum.TryParse<ToolCategory>(category, true, out var cat))
            query = query.Where(t => t.Category == cat);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ToolStatus>(status, true, out var st))
            query = query.Where(t => t.Status == st);

        query = query.OrderByDescending(t => t.CreatedAt);

        return await query.ToPagedResponseAsync(page, pageSize, t => t.ToResponse());
    }

    public async Task<ToolResponse?> GetByIdAsync(int id)
    {
        var tool = await _context.Tools.FindAsync(id);
        return tool?.ToResponse();
    }

    public async Task<ToolResponse> CreateAsync(CreateToolRequest request)
    {
        var tool = new Tool
        {
            Name = request.Name,
            Category = request.Category,
            Quantity = request.Quantity,
            Status = request.Status,
            PurchaseDate = request.PurchaseDate
        };

        _context.Tools.Add(tool);
        await _context.SaveChangesAsync();

        return tool.ToResponse();
    }

    public async Task<ToolResponse?> UpdateAsync(int id, UpdateToolRequest request)
    {
        var tool = await _context.Tools.FindAsync(id);
        if (tool is null) return null;

        tool.Name = request.Name;
        tool.Category = request.Category;
        tool.Quantity = request.Quantity;
        tool.Status = request.Status;
        tool.PurchaseDate = request.PurchaseDate;
        tool.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return tool.ToResponse();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var tool = await _context.Tools.FindAsync(id);
        if (tool is null) return false;

        tool.IsDeleted = true;
        tool.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}
