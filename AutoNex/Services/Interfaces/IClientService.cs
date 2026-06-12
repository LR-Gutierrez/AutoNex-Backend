using AutoNex.DTOs.Clients;

namespace AutoNex.Services.Interfaces;

public interface IClientService
{
    Task<List<ClientResponse>> GetAllAsync(string? search);
    Task<ClientResponse?> GetByIdAsync(int id);
    Task<ClientResponse> CreateAsync(CreateClientRequest request);
    Task<ClientResponse?> UpdateAsync(int id, UpdateClientRequest request);
    Task<bool> DeleteAsync(int id);
}
