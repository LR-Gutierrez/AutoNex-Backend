using AutoNex.DTOs;
using AutoNex.DTOs.Tools;

namespace AutoNex.Services.Interfaces;

public interface IToolService
{
    Task<PagedResponse<ToolResponse>> GetAllAsync(string? categoryName, string? status, int? page, int? pageSize, CancellationToken cancellationToken = default);
    Task<ToolResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ToolResponse> CreateAsync(CreateToolRequest request, CancellationToken cancellationToken = default);
    Task<ToolResponse?> UpdateAsync(int id, UpdateToolRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
