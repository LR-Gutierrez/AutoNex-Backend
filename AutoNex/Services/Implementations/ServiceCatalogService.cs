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

    public async Task<PagedResponse<ServiceResponse>> GetAllAsync(int? page, int? pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Services
            .AsNoTracking()
            .OrderByDescending(s => s.CreatedAt)
            .AsQueryable();

        return await query.ToPagedResponseAsync(page, pageSize, s => s.ToResponse(), cancellationToken).ConfigureAwait(false);
    }

    public async Task<ServiceResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var service = await _context.Services.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
        return service?.ToResponse();
    }

    public async Task<ServiceResponse> CreateAsync(CreateServiceRequest request, CancellationToken cancellationToken = default)
    {
        var service = new Models.Service
        {
            Name = request.Name,
            Description = request.Description,
            DefaultPrice = request.DefaultPrice,
            MinKmInterval = request.MinKmInterval,
            MaxKmInterval = request.MaxKmInterval,
            RecommendedMonths = request.RecommendedMonths
        };

        _context.Services.Add(service);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return service.ToResponse();
    }

    public async Task<ServiceResponse?> UpdateAsync(int id, UpdateServiceRequest request, CancellationToken cancellationToken = default)
    {
        var service = await _context.Services.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
        if (service is null) return null;

        service.Name = request.Name;
        service.Description = request.Description;
        service.DefaultPrice = request.DefaultPrice;
        service.MinKmInterval = request.MinKmInterval;
        service.MaxKmInterval = request.MaxKmInterval;
        service.RecommendedMonths = request.RecommendedMonths;
        service.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return service.ToResponse();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var service = await _context.Services.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
        if (service is null) return false;

        service.IsDeleted = true;
        service.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
