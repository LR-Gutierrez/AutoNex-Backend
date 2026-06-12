using AutoNex.DTOs.Tools;

namespace AutoNex.Services.Interfaces;

public interface IToolService
{
    Task<List<ToolResponse>> GetAllAsync(string? category, string? status);
    Task<ToolResponse?> GetByIdAsync(int id);
    Task<ToolResponse> CreateAsync(CreateToolRequest request);
    Task<ToolResponse?> UpdateAsync(int id, UpdateToolRequest request);
    Task<bool> DeleteAsync(int id);
}
