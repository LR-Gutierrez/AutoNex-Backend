using AutoNex.DTOs;
using AutoNex.DTOs.Tools;

namespace AutoNex.Services.Interfaces;

public interface IToolService
{
    Task<PagedResponse<ToolResponse>> GetAllAsync(string? categoryName, string? status, int? page, int? pageSize);
    Task<ToolResponse?> GetByIdAsync(int id);
    Task<ToolResponse> CreateAsync(CreateToolRequest request);
    Task<ToolResponse?> UpdateAsync(int id, UpdateToolRequest request);
    Task<bool> DeleteAsync(int id);
}
