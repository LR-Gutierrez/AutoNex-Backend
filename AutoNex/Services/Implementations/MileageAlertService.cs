using AutoNex.Data;
using AutoNex.DTOs;
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

    public async Task<PagedResponse<MileageAlertResponse>> GetAllAsync(bool? due, int? page, int? pageSize)
    {
        var alerts = await _context.MileageAlerts
            .Include(a => a.Vehicle)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        var vehicleIds = alerts.Select(a => a.VehicleId).ToList();
        var latestKms = await GetLatestKmBatchAsync(vehicleIds);

        var filtered = due.HasValue && due.Value
            ? alerts.Where(a => a.IsActive && IsDue(a, latestKms.GetValueOrDefault(a.VehicleId, 0))).ToList()
            : alerts;

        var p = Math.Max(page ?? 1, 1);
        var ps = Math.Clamp(pageSize ?? 20, 1, 100);
        var totalCount = filtered.Count;
        var items = filtered
            .Skip((p - 1) * ps)
            .Take(ps)
            .Select(a => a.ToResponse(latestKms.GetValueOrDefault(a.VehicleId, 0)))
            .ToList();

        return new PagedResponse<MileageAlertResponse>
        {
            Items = items,
            Page = p,
            PageSize = ps,
            TotalCount = totalCount
        };
    }

    public async Task<MileageAlertResponse?> GetByIdAsync(int id)
    {
        var alert = await _context.MileageAlerts
            .Include(a => a.Vehicle)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (alert is null) return null;

        var currentKm = await GetLatestKmAsync(alert.VehicleId);
        return alert.ToResponse(currentKm);
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

    public async Task<MileageAlertResponse> CreateOrUpdateFromOrderAsync(int orderId)
    {
        var order = await _context.ServiceOrders
            .Include(o => o.Items)
                .ThenInclude(i => i.Service)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new KeyNotFoundException("Orden no encontrada");

        var alert = await _context.MileageAlerts
            .FirstOrDefaultAsync(a => a.VehicleId == order.VehicleId && a.IsActive);

        var estimatedWeeklyKm = order.EstimatedDailyKm.HasValue && order.DaysPerWeek.HasValue
            ? order.EstimatedDailyKm.Value * order.DaysPerWeek.Value
            : (int?)null;

        // Step 1: Calculate NextAlertKm and NextAlertDate from services
        int maxKmInterval = 0;
        DateTime? nextAlertDate = null;
        foreach (var item in order.Items.Where(i => i.Service is not null))
        {
            if (item.Service!.MaxKmInterval > maxKmInterval)
                maxKmInterval = item.Service!.MaxKmInterval.Value;

            if (item.Service!.RecommendedMonths is not null)
            {
                var date = order.Date.AddMonths(item.Service!.RecommendedMonths.Value);
                if (nextAlertDate is null || date < nextAlertDate)
                    nextAlertDate = date;
            }
        }
        bool hasKmInterval = maxKmInterval > 0;
        if (!hasKmInterval) maxKmInterval = 5000;

        var currentKm = order.CurrentKm;

        if (alert is null)
        {
            alert = new MileageAlert
            {
                VehicleId = order.VehicleId,
                EstimatedWeeklyKm = estimatedWeeklyKm ?? 0,
                NextAlertKm = hasKmInterval ? currentKm + maxKmInterval : currentKm + 5000,
                NextAlertDate = nextAlertDate,
                IsActive = true
            };
            _context.MileageAlerts.Add(alert);
        }
        else
        {
            // Refine EstimatedWeeklyKm with historical data
            var previousOrder = await _context.ServiceOrders
                .Where(o => o.VehicleId == order.VehicleId
                    && o.Status == Enums.ServiceOrderStatus.Completed
                    && o.Id != order.Id)
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

            alert.EstimatedWeeklyKm = estimatedWeeklyKm ?? alert.EstimatedWeeklyKm;
            if (hasKmInterval) alert.NextAlertKm = currentKm + maxKmInterval;
            alert.NextAlertDate = nextAlertDate ?? alert.NextAlertDate;
            alert.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(alert.Id))!;
    }

    private async Task<int> GetLatestKmAsync(int vehicleId)
    {
        var lastOrder = await _context.ServiceOrders
            .Where(o => o.VehicleId == vehicleId && o.Status == Enums.ServiceOrderStatus.Completed)
            .OrderByDescending(o => o.Date)
            .Select(o => (int?)o.CurrentKm)
            .FirstOrDefaultAsync();

        return lastOrder ?? 0;
    }

    private async Task<Dictionary<int, int>> GetLatestKmBatchAsync(List<int> vehicleIds)
    {
        if (vehicleIds.Count == 0) return [];

        var latest = await _context.ServiceOrders
            .Where(o => vehicleIds.Contains(o.VehicleId) && o.Status == Enums.ServiceOrderStatus.Completed)
            .GroupBy(o => o.VehicleId)
            .Select(g => new
            {
                VehicleId = g.Key,
                CurrentKm = g.OrderByDescending(o => o.Date).Select(o => o.CurrentKm).FirstOrDefault()
            })
            .ToListAsync();

        return latest.ToDictionary(x => x.VehicleId, x => x.CurrentKm);
    }

    private static bool IsDue(MileageAlert alert, int currentKm)
    {
        if (!alert.IsActive) return false;
        bool kmDue = currentKm + (alert.EstimatedWeeklyKm * 2) >= alert.NextAlertKm;
        bool timeDue = alert.NextAlertDate.HasValue && DateTime.UtcNow >= alert.NextAlertDate.Value;
        return kmDue || timeDue;
    }
}
