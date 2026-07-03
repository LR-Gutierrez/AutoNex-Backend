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

    public const int WarningWeeksMargin = 2;

    public MileageAlertService(AppDbContext context)
    {
        _context = context;
    }

    public static bool IsDue(DateTime? nextAlertDate, int currentKm, int estimatedWeeklyKm, int nextAlertKm)
    {
        if (nextAlertDate is not null && DateTime.UtcNow >= nextAlertDate)
            return true;

        return currentKm + (estimatedWeeklyKm * WarningWeeksMargin) >= nextAlertKm;
    }

    public async Task<PagedResponse<MileageAlertResponse>> GetAllAsync(bool? due, int? page, int? pageSize, CancellationToken cancellationToken = default)
    {
        var p = Math.Max(page ?? 1, 1);
        var ps = Math.Clamp(pageSize ?? 20, 1, 100);

        var query = _context.MileageAlerts
            .AsNoTracking()
            .Include(a => a.Vehicle)
            .Include(a => a.Service)
            .Where(a => !a.Vehicle.IsDeleted)
            .AsQueryable();

        if (due.HasValue && due.Value)
        {
            query = query.Where(a => a.IsActive && (
                a.NextAlertDate != null && DateTime.UtcNow >= a.NextAlertDate
                ||
                _context.ServiceOrders
                    .Where(o => o.VehicleId == a.VehicleId && o.Status != Enums.ServiceOrderStatus.Cancelled)
                    .OrderByDescending(o => o.Date)
                    .Select(o => o.CurrentKm)
                    .FirstOrDefault() + (a.EstimatedWeeklyKm * WarningWeeksMargin) >= a.NextAlertKm
            ));
        }

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((p - 1) * ps)
            .Take(ps)
            .ToListAsync(cancellationToken);

        var vehicleIds = items.Select(a => a.VehicleId).ToList();
        var latestKms = await GetLatestKmBatchAsync(vehicleIds, cancellationToken).ConfigureAwait(false);

        return new PagedResponse<MileageAlertResponse>
        {
            Items = items.Select(a => a.ToResponse(latestKms.GetValueOrDefault(a.VehicleId, 0))).ToList(),
            Page = p,
            PageSize = ps,
            TotalCount = totalCount
        };
    }

    public async Task<MileageAlertResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var alert = await _context.MileageAlerts
            .AsNoTracking()
            .Include(a => a.Vehicle)
            .Include(a => a.Service)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (alert is null) return null;

        var currentKm = await GetLatestKmAsync(alert.VehicleId, cancellationToken).ConfigureAwait(false);
        return alert.ToResponse(currentKm);
    }

    public async Task<MileageAlertResponse> CreateAsync(CreateMileageAlertRequest request, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles.FindAsync(new object[] { request.VehicleId }, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Vehículo no encontrado");
        var service = await _context.Services.FindAsync(new object[] { request.ServiceId }, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Servicio no encontrado");

        if (!service.MaxKmInterval.HasValue && !service.MaxMonth.HasValue)
            throw new InvalidOperationException("El servicio no tiene intervalos de mantenimiento configurados");

        var existing = await _context.MileageAlerts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.VehicleId == request.VehicleId && a.ServiceId == request.ServiceId, cancellationToken);

        if (existing is not null)
            throw new InvalidOperationException("El vehículo ya tiene una alerta configurada para este servicio");

        var currentKm = await GetLatestKmAsync(request.VehicleId, cancellationToken);

        DateTime? nextAlertDate = service.MaxMonth.HasValue
            ? DateTime.UtcNow.AddMonths(service.MaxMonth.Value)
            : null;

        var alert = new MileageAlert
        {
            VehicleId = request.VehicleId,
            ServiceId = request.ServiceId,
            EstimatedWeeklyKm = request.EstimatedWeeklyKm,
            NextAlertKm = service.MaxKmInterval.HasValue ? currentKm + service.MaxKmInterval.Value : currentKm,
            NextAlertDate = nextAlertDate,
            IsActive = true
        };

        _context.MileageAlerts.Add(alert);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (await GetByIdAsync(alert.Id, cancellationToken).ConfigureAwait(false))!;
    }

    public async Task<MileageAlertResponse?> UpdateAsync(int id, UpdateMileageAlertRequest request, CancellationToken cancellationToken = default)
    {
        var alert = await _context.MileageAlerts.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
        if (alert is null) return null;

        alert.EstimatedWeeklyKm = request.EstimatedWeeklyKm;
        alert.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return (await GetByIdAsync(id, cancellationToken).ConfigureAwait(false))!;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var alert = await _context.MileageAlerts.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
        if (alert is null) return false;

        alert.IsDeleted = true;
        alert.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<MileageAlertResponse?> AttendAsync(int id, CancellationToken cancellationToken = default)
    {
        var alert = await _context.MileageAlerts.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
        if (alert is null) return null;

        alert.IsActive = false;
        alert.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<MileageAlertResponse>> CreateOrUpdateFromOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await _context.ServiceOrders
            .Include(o => o.Items)
                .ThenInclude(i => i.Service)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken)
            ?? throw new KeyNotFoundException("Orden no encontrada");

        var estimatedWeeklyKm = order.EstimatedDailyKm.HasValue && order.DaysPerWeek.HasValue
            ? order.EstimatedDailyKm.Value * order.DaysPerWeek.Value
            : (int?)null;

        var currentKm = order.CurrentKm;

        var serviceItems = order.Items
            .Where(i => i.Service is not null
                && (i.Service.MaxKmInterval.HasValue || i.Service.MaxMonth.HasValue))
            .ToList();

        if (serviceItems.Count == 0)
            return [];

        var serviceIds = serviceItems.Select(i => i.Service!.Id).ToList();
        var existingAlerts = await _context.MileageAlerts
            .IgnoreQueryFilters()
            .Where(a => a.VehicleId == order.VehicleId && serviceIds.Contains(a.ServiceId) && a.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var existingAlert in existingAlerts)
        {
            existingAlert.IsActive = false;
            existingAlert.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        foreach (var item in serviceItems)
        {
            var service = item.Service!;
            var hasKmInterval = service.MaxKmInterval.HasValue && service.MaxKmInterval.Value > 0;

            DateTime? nextAlertDate = service.MaxMonth.HasValue
                ? order.Date.AddMonths(service.MaxMonth.Value)
                : null;

            var nextAlertKm = hasKmInterval
                ? currentKm + service.MaxKmInterval!.Value
                : currentKm;

            var alert = new MileageAlert
            {
                VehicleId = order.VehicleId,
                ServiceId = service.Id,
                EstimatedWeeklyKm = estimatedWeeklyKm ?? 0,
                NextAlertKm = nextAlertKm,
                NextAlertDate = nextAlertDate,
                IsActive = true
            };
            _context.MileageAlerts.Add(alert);
        }

        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var updatedAlerts = await _context.MileageAlerts
            .AsNoTracking()
            .Include(a => a.Vehicle)
            .Include(a => a.Service)
            .Where(a => a.VehicleId == order.VehicleId && serviceIds.Contains(a.ServiceId))
            .ToListAsync(cancellationToken);

        var vehicleIds = updatedAlerts.Select(a => a.VehicleId).ToList();
        var latestKms = await GetLatestKmBatchAsync(vehicleIds, cancellationToken).ConfigureAwait(false);

        return updatedAlerts
            .Select(a => a.ToResponse(latestKms.GetValueOrDefault(a.VehicleId, 0)))
            .ToList();
    }

    private async Task<int> GetLatestKmAsync(int vehicleId, CancellationToken cancellationToken = default)
    {
        var lastOrder = await _context.ServiceOrders
            .AsNoTracking()
            .Where(o => o.VehicleId == vehicleId)
            .Where(o => o.Status != Enums.ServiceOrderStatus.Cancelled)
            .OrderByDescending(o => o.Date)
            .Select(o => (int?)o.CurrentKm)
            .FirstOrDefaultAsync(cancellationToken);

        return lastOrder ?? 0;
    }

    private async Task<Dictionary<int, int>> GetLatestKmBatchAsync(List<int> vehicleIds, CancellationToken cancellationToken = default)
    {
        if (vehicleIds.Count == 0) return [];

        var latest = await _context.ServiceOrders
            .AsNoTracking()
            .Where(o => vehicleIds.Contains(o.VehicleId))
            .Where(o => o.Status != Enums.ServiceOrderStatus.Cancelled)
            .GroupBy(o => o.VehicleId)
            .Select(g => new
            {
                VehicleId = g.Key,
                CurrentKm = g.OrderByDescending(o => o.Date).Select(o => o.CurrentKm).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return latest.ToDictionary(x => x.VehicleId, x => x.CurrentKm);
    }
}
