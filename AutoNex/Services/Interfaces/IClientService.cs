using AutoNex.DTOs;
using AutoNex.DTOs.Clients;

namespace AutoNex.Services.Interfaces;

public interface IClientService
{
    Task<PagedResponse<ClientResponse>> GetAllAsync(string? search, int? page, int? pageSize);
    Task<ClientResponse?> GetByIdAsync(int id);
    Task<ClientResponse> CreateAsync(CreateClientRequest request);
    Task<ClientResponse?> UpdateAsync(int id, UpdateClientRequest request);
    Task<bool> DeleteAsync(int id);
}
