using AutoNex.Data;
using AutoNex.DTOs;
using AutoNex.DTOs.ToolCategories;
using AutoNex.Helpers;
using AutoNex.Models;
using AutoNex.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoNex.Services.Implementations;

public class ToolCategoryService : IToolCategoryService
{
    private readonly AppDbContext _context;

    public ToolCategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<ToolCategoryResponse>> GetAllAsync(int? page, int? pageSize)
    {
        var query = _context.ToolCategories
            .OrderByDescending(tc => tc.CreatedAt)
            .AsQueryable();

        return await query.ToPagedResponseAsync(page, pageSize, tc => tc.ToResponse());
    }

    public async Task<ToolCategoryResponse?> GetByIdAsync(int id)
    {
        var category = await _context.ToolCategories.FindAsync(id);
        return category?.ToResponse();
    }

    public async Task<ToolCategoryResponse> CreateAsync(CreateToolCategoryRequest request)
    {
        var category = new ToolCategory
        {
            Name = request.Name
        };

        _context.ToolCategories.Add(category);
        await _context.SaveChangesAsync();

        return category.ToResponse();
    }

    public async Task<ToolCategoryResponse?> UpdateAsync(int id, UpdateToolCategoryRequest request)
    {
        var category = await _context.ToolCategories.FindAsync(id);
        if (category is null) return null;

        category.Name = request.Name;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return category.ToResponse();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _context.ToolCategories.FindAsync(id);
        if (category is null) return false;

        category.IsDeleted = true;
        category.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}
