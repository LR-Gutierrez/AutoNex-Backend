using AutoNex.Data;
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

    public async Task<List<ClientResponse>> GetAllAsync(string? search)
    {
        var query = _context.Clients.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c =>
                c.FullName.Contains(search) || c.Phone.Contains(search));

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => c.ToResponse())
            .ToListAsync();
    }

    public async Task<ClientResponse?> GetByIdAsync(int id)
    {
        var client = await _context.Clients
            .Include(c => c.Vehicles)
            .FirstOrDefaultAsync(c => c.Id == id);

        return client?.ToResponse();
    }

    public async Task<ClientResponse> CreateAsync(CreateClientRequest request)
    {
        var client = new Client
        {
            FullName = request.FullName,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        return client.ToResponse();
    }

    public async Task<ClientResponse?> UpdateAsync(int id, UpdateClientRequest request)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client is null) return null;

        client.FullName = request.FullName;
        client.Phone = request.Phone;
        client.Email = request.Email;
        client.Address = request.Address;
        client.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return client.ToResponse();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client is null) return false;

        client.IsDeleted = true;
        client.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}
