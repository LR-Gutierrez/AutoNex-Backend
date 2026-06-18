using AutoNex.DTOs;
using AutoNex.DTOs.ToolCategories;

namespace AutoNex.Services.Interfaces;

public interface IToolCategoryService
{
    Task<PagedResponse<ToolCategoryResponse>> GetAllAsync(int? page, int? pageSize, CancellationToken cancellationToken = default);
    Task<ToolCategoryResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ToolCategoryResponse> CreateAsync(CreateToolCategoryRequest request, CancellationToken cancellationToken = default);
    Task<ToolCategoryResponse?> UpdateAsync(int id, UpdateToolCategoryRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
