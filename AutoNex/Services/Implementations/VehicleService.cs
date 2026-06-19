using AutoNex.Data;
using AutoNex.DTOs;
using AutoNex.DTOs.Vehicles;
using AutoNex.Helpers;
using AutoNex.Models;
using AutoNex.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoNex.Services.Implementations;

public class VehicleService : IVehicleService
{
    private readonly AppDbContext _context;

    public VehicleService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<VehicleResponse>> GetAllAsync(string? search, int? page, int? pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Vehicles
            .AsNoTracking()
            .Include(v => v.Client)
            .Where(v => !v.Client.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(v =>
                v.LicensePlate.Contains(search) ||
                v.Client.FullName.Contains(search));

        query = query.OrderByDescending(v => v.CreatedAt);

        return await query.ToPagedResponseAsync(page, pageSize, v => v.ToResponse(), cancellationToken);
    }

    public async Task<VehicleResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .Include(v => v.Client)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

        if (vehicle is null) return null;

        var orders = await _context.ServiceOrders
            .AsNoTracking()
            .Where(o => o.VehicleId == id && !o.IsDeleted)
            .OrderByDescending(o => o.Date)
            .Select(o => new ServiceOrderBriefResponse(
                o.Id,
                o.Date,
                o.Status.ToString(),
                o.TotalAmount,
                o.CurrentKm
            ))
            .ToListAsync(cancellationToken);

        return vehicle.ToResponse(orders);
    }

    public async Task<VehicleResponse> CreateAsync(CreateVehicleRequest request, CancellationToken cancellationToken = default)
    {
        var clientExists = await _context.Clients.AnyAsync(c => c.Id == request.ClientId, cancellationToken);
        if (!clientExists)
            throw new KeyNotFoundException("El cliente no existe");

        var vehicle = new Vehicle
        {
            ClientId = request.ClientId,
            Brand = request.Brand,
            Model = request.Model,
            Year = request.Year,
            LicensePlate = request.LicensePlate,
            VIN = request.VIN
        };

        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync(cancellationToken);

        return (await GetByIdAsync(vehicle.Id, cancellationToken))!;
    }

    public async Task<VehicleResponse?> UpdateAsync(int id, UpdateVehicleRequest request, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles.FindAsync(new object[] { id }, cancellationToken);
        if (vehicle is null) return null;

        vehicle.Brand = request.Brand;
        vehicle.Model = request.Model;
        vehicle.Year = request.Year;
        vehicle.LicensePlate = request.LicensePlate;
        vehicle.VIN = request.VIN;
        vehicle.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return (await GetByIdAsync(id, cancellationToken))!;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles.FindAsync(new object[] { id }, cancellationToken);
        if (vehicle is null) return false;

        vehicle.IsDeleted = true;
        vehicle.UpdatedAt = DateTime.UtcNow;

        var alerts = await _context.MileageAlerts
            .Where(a => a.VehicleId == id)
            .ToListAsync(cancellationToken);

        foreach (var alert in alerts)
        {
            alert.IsDeleted = true;
            alert.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
