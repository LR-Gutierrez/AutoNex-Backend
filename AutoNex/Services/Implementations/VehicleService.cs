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

    public async Task<PagedResponse<VehicleResponse>> GetAllAsync(string? search, int? page, int? pageSize)
    {
        var query = _context.Vehicles
            .Include(v => v.Client)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(v =>
                v.LicensePlate.Contains(search) ||
                v.Client.FullName.Contains(search));

        query = query.OrderByDescending(v => v.CreatedAt);

        return await query.ToPagedResponseAsync(page, pageSize, v => v.ToResponse());
    }

    public async Task<VehicleResponse?> GetByIdAsync(int id)
    {
        var vehicle = await _context.Vehicles
            .Include(v => v.Client)
            .FirstOrDefaultAsync(v => v.Id == id);

        return vehicle?.ToResponse();
    }

    public async Task<VehicleResponse> CreateAsync(CreateVehicleRequest request)
    {
        var clientExists = await _context.Clients.AnyAsync(c => c.Id == request.ClientId);
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
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(vehicle.Id))!;
    }

    public async Task<VehicleResponse?> UpdateAsync(int id, UpdateVehicleRequest request)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle is null) return null;

        vehicle.Brand = request.Brand;
        vehicle.Model = request.Model;
        vehicle.Year = request.Year;
        vehicle.LicensePlate = request.LicensePlate;
        vehicle.VIN = request.VIN;
        vehicle.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (await GetByIdAsync(id))!;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle is null) return false;

        vehicle.IsDeleted = true;
        vehicle.UpdatedAt = DateTime.UtcNow;

        var alerts = await _context.MileageAlerts
            .Where(a => a.VehicleId == id)
            .ToListAsync();

        foreach (var alert in alerts)
        {
            alert.IsDeleted = true;
            alert.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }
}
