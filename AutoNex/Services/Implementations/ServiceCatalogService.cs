using AutoNex.Data;
using AutoNex.DTOs.Services;
using AutoNex.Helpers;
using AutoNex.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoNex.Services.Implementations;

public class ServiceCatalogService : IServiceCatalogService
{
    private readonly AppDbContext _context;

    public ServiceCatalogService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ServiceResponse>> GetAllAsync()
    {
        return await _context.Services
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => s.ToResponse())
            .ToListAsync();
    }

    public async Task<ServiceResponse?> GetByIdAsync(int id)
    {
        var service = await _context.Services.FindAsync(id);
        return service?.ToResponse();
    }

    public async Task<ServiceResponse> CreateAsync(CreateServiceRequest request)
    {
        var service = new Models.Service
        {
            Name = request.Name,
            Description = request.Description,
            DefaultPrice = request.DefaultPrice
        };

        _context.Services.Add(service);
        await _context.SaveChangesAsync();

        return service.ToResponse();
    }

    public async Task<ServiceResponse?> UpdateAsync(int id, UpdateServiceRequest request)
    {
        var service = await _context.Services.FindAsync(id);
        if (service is null) return null;

        service.Name = request.Name;
        service.Description = request.Description;
        service.DefaultPrice = request.DefaultPrice;
        service.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return service.ToResponse();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service is null) return false;

        service.IsDeleted = true;
        service.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}
