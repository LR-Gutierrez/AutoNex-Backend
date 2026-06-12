using AutoNex.Data;
using AutoNex.DTOs;
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

    public async Task<PagedResponse<ServiceResponse>> GetAllAsync(int? page, int? pageSize)
    {
        var query = _context.Services
            .OrderByDescending(s => s.CreatedAt)
            .AsQueryable();

        var paged = await query.ToPagedAsync(page, pageSize);

        return new PagedResponse<ServiceResponse>
        {
            Items = paged.Items.Select(s => s.ToResponse()).ToList(),
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount
        };
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
            DefaultPrice = request.DefaultPrice,
            RecommendedKmInterval = request.RecommendedKmInterval
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
        service.RecommendedKmInterval = request.RecommendedKmInterval;
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

    // Service Variants

    public async Task<List<ServiceVariantResponse>> GetVariantsAsync(int serviceId)
    {
        var variants = await _context.ServiceVariants
            .Include(v => v.Service)
            .Where(v => v.ServiceId == serviceId)
            .OrderBy(v => v.Name)
            .ToListAsync();

        return variants.Select(v => v.ToResponse()).ToList();
    }

    public async Task<ServiceVariantResponse?> GetVariantByIdAsync(int id)
    {
        var variant = await _context.ServiceVariants
            .Include(v => v.Service)
            .FirstOrDefaultAsync(v => v.Id == id);

        return variant?.ToResponse();
    }

    public async Task<ServiceVariantResponse> CreateVariantAsync(int serviceId, CreateServiceVariantRequest request)
    {
        var service = await _context.Services.FindAsync(serviceId)
            ?? throw new KeyNotFoundException("Servicio no encontrado");

        var variant = new Models.ServiceVariant
        {
            ServiceId = serviceId,
            Name = request.Name,
            Description = request.Description,
            MinKmInterval = request.MinKmInterval,
            MaxKmInterval = request.MaxKmInterval,
            RecommendedMonths = request.RecommendedMonths
        };

        _context.ServiceVariants.Add(variant);
        await _context.SaveChangesAsync();

        return (await GetVariantByIdAsync(variant.Id))!;
    }

    public async Task<ServiceVariantResponse?> UpdateVariantAsync(int id, UpdateServiceVariantRequest request)
    {
        var variant = await _context.ServiceVariants.FindAsync(id);
        if (variant is null) return null;

        variant.Name = request.Name;
        variant.Description = request.Description;
        variant.MinKmInterval = request.MinKmInterval;
        variant.MaxKmInterval = request.MaxKmInterval;
        variant.RecommendedMonths = request.RecommendedMonths;
        variant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (await GetVariantByIdAsync(id))!;
    }

    public async Task<bool> DeleteVariantAsync(int id)
    {
        var variant = await _context.ServiceVariants.FindAsync(id);
        if (variant is null) return false;

        variant.IsActive = false;
        variant.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}
