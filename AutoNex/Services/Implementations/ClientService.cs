using AutoNex.Data;
using AutoNex.DTOs;
using AutoNex.DTOs.Clients;
using AutoNex.Helpers;
using AutoNex.Models;
using AutoNex.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoNex.Services.Implementations;

public class ClientService : IClientService
{
    private readonly AppDbContext _context;

    public ClientService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<ClientResponse>> GetAllAsync(string? search, int? page, int? pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Clients
            .AsNoTracking()
            .Include(c => c.Vehicles)
            .Where(c => !c.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c =>
                c.FullName.Contains(search) || c.Phone.Contains(search));

        query = query.OrderByDescending(c => c.CreatedAt);

        return await query.ToPagedResponseAsync(page, pageSize, c => c.ToResponse(), cancellationToken).ConfigureAwait(false);
    }

    public async Task<ClientResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var client = await _context.Clients
            .AsNoTracking()
            .Include(c => c.Vehicles)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        return client?.ToResponse();
    }

    public async Task<ClientResponse> CreateAsync(CreateClientRequest request, CancellationToken cancellationToken = default)
    {
        var client = new Client
        {
            FullName = request.FullName,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return client.ToResponse();
    }

    public async Task<ClientResponse?> UpdateAsync(int id, UpdateClientRequest request, CancellationToken cancellationToken = default)
    {
        var client = await _context.Clients.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
        if (client is null) return null;

        client.FullName = request.FullName;
        client.Phone = request.Phone;
        client.Email = request.Email;
        client.Address = request.Address;
        client.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return client.ToResponse();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var client = await _context.Clients.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
        if (client is null) return false;

        client.IsDeleted = true;
        client.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
