using AutoNex.DTOs;
using AutoNex.DTOs.Clients;

namespace AutoNex.Services.Interfaces;

public interface IClientService
{
    Task<PagedResponse<ClientResponse>> GetAllAsync(string? search, int? page, int? pageSize, CancellationToken cancellationToken = default);
    Task<ClientResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ClientResponse> CreateAsync(CreateClientRequest request, CancellationToken cancellationToken = default);
    Task<ClientResponse?> UpdateAsync(int id, UpdateClientRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
