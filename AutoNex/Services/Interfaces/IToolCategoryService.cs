using AutoNex.DTOs;
using AutoNex.DTOs.ToolCategories;

namespace AutoNex.Services.Interfaces;

public interface IToolCategoryService
{
    Task<PagedResponse<ToolCategoryResponse>> GetAllAsync(int? page, int? pageSize);
    Task<ToolCategoryResponse?> GetByIdAsync(int id);
    Task<ToolCategoryResponse> CreateAsync(CreateToolCategoryRequest request);
    Task<ToolCategoryResponse?> UpdateAsync(int id, UpdateToolCategoryRequest request);
    Task<bool> DeleteAsync(int id);
}
