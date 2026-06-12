using AutoNex.Data;
using AutoNex.DTOs.MileageAlerts;
using AutoNex.Helpers;
using AutoNex.Models;
using AutoNex.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoNex.Services.Implementations;

public class MileageAlertService : IMileageAlertService
{
    private readonly AppDbContext _context;

    public MileageAlertService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<MileageAlertResponse>> GetAllAsync(bool? due)
    {
        var alerts = await _context.MileageAlerts
            .Include(a => a.Vehicle)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        var filtered = due.HasValue && due.Value
            ? alerts.Where(a => a.IsActive && IsDue(a))
            : alerts;

        return filtered.Select(a => a.ToResponse()).ToList();
    }

    public async Task<MileageAlertResponse?> GetByIdAsync(int id)
    {
        var alert = await _context.MileageAlerts
            .Include(a => a.Vehicle)
            .FirstOrDefaultAsync(a => a.Id == id);

        return alert?.ToResponse();
    }

    public async Task<MileageAlertResponse> CreateAsync(CreateMileageAlertRequest request)
    {
        var vehicle = await _context.Vehicles.FindAsync(request.VehicleId)
            ?? throw new KeyNotFoundException("Vehículo no encontrado");

        var existing = await _context.MileageAlerts
            .FirstOrDefaultAsync(a => a.VehicleId == request.VehicleId);

        if (existing is not null)
            throw new InvalidOperationException("El vehículo ya tiene una alerta configurada");

        var alert = new MileageAlert
        {
            VehicleId = request.VehicleId,
            LastRecordedKm = 0,
            EstimatedWeeklyKm = request.EstimatedWeeklyKm,
            NextAlertKm = 5000,
            IsActive = true
        };

        _context.MileageAlerts.Add(alert);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(alert.Id))!;
    }

    public async Task<MileageAlertResponse?> UpdateAsync(int id, UpdateMileageAlertRequest request)
    {
        var alert = await _context.MileageAlerts.FindAsync(id);
        if (alert is null) return null;

        alert.EstimatedWeeklyKm = request.EstimatedWeeklyKm;
        alert.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (await GetByIdAsync(id))!;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var alert = await _context.MileageAlerts.FindAsync(id);
        if (alert is null) return false;

        alert.IsActive = false;
        alert.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task UpdateAlertFromServiceOrderAsync(int vehicleId, int currentKm, List<int> orderItemIds)
    {
        var alert = await _context.MileageAlerts
            .FirstOrDefaultAsync(a => a.VehicleId == vehicleId && a.IsActive);

        if (alert is null) return;

        // Step 1: Refine EstimatedWeeklyKm with historical data
        var previousOrder = await _context.ServiceOrders
            .Where(o => o.VehicleId == vehicleId && o.Status == Enums.ServiceOrderStatus.Completed)
            .OrderByDescending(o => o.Date)
            .FirstOrDefaultAsync();

        if (previousOrder is not null && previousOrder.CurrentKm > 0 && previousOrder.CurrentKm != currentKm)
        {
            var kmDiff = currentKm - previousOrder.CurrentKm;
            var daysDiff = (DateTime.UtcNow - previousOrder.Date).TotalDays;
            if (daysDiff > 0)
            {
                var calculatedWeekly = (int)(kmDiff / (daysDiff / 7));
                if (calculatedWeekly > 0)
                    alert.EstimatedWeeklyKm = calculatedWeekly;
            }
        }

        // Step 2: Calculate NextAlertKm using ServiceVariants or fallback to Service.RecommendedKmInterval
        var items = await _context.ServiceOrderItems
            .Where(i => orderItemIds.Contains(i.Id))
            .Include(i => i.ServiceVariant)
            .Include(i => i.Service)
            .ToListAsync();

        int maxKmInterval = 0;

        foreach (var item in items)
        {
            if (item.ServiceVariant is not null)
            {
                if (item.ServiceVariant.MaxKmInterval > maxKmInterval)
                    maxKmInterval = item.ServiceVariant.MaxKmInterval;
            }
            else if (item.Service?.RecommendedKmInterval is not null)
            {
                if (item.Service.RecommendedKmInterval.Value > maxKmInterval)
                    maxKmInterval = item.Service.RecommendedKmInterval.Value;
            }
        }

        if (maxKmInterval == 0)
            maxKmInterval = 5000;

        alert.LastRecordedKm = currentKm;
        alert.NextAlertKm = currentKm + maxKmInterval;
        alert.UpdatedAt = DateTime.UtcNow;

        // Don't save here — the caller owns the SaveChangesAsync
    }

    private static bool IsDue(MileageAlert alert)
    {
        return alert.LastRecordedKm + (alert.EstimatedWeeklyKm * 2) >= alert.NextAlertKm;
    }
}
