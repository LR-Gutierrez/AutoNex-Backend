using AutoNex.Data;
using AutoNex.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AutoNex.Services.Implementations;

public class MileageAlertBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MileageAlertBackgroundService> _logger;
    private readonly int _intervalSeconds;

    public MileageAlertBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<MileageAlertBackgroundService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _intervalSeconds = configuration.GetValue<int>("MileageAlert:IntervalSeconds", 3600);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MileageAlertBackgroundService iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDueAlertsAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar alertas de kilometraje");
            }

            await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds), stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task ProcessDueAlertsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var alerts = await context.MileageAlerts
            .Include(a => a.Vehicle)
            .Include(a => a.Service)
            .Where(a => a.IsActive && !a.Vehicle.IsDeleted)
            .ToListAsync(stoppingToken);

        var vehicleIds = alerts.Select(a => a.VehicleId).Distinct().ToList();
        var latestKms = await GetLatestKmBatchAsync(context, vehicleIds, stoppingToken);

        var dueAlerts = alerts
            .Where(a => MileageAlertService.IsDue(
                a.NextAlertDate,
                latestKms.GetValueOrDefault(a.VehicleId, 0),
                a.EstimatedWeeklyKm,
                a.NextAlertKm))
            .ToList();

        var minInterval = TimeSpan.FromHours(23);

        foreach (var alert in dueAlerts)
        {
            if (alert.LastAlertDate.HasValue && DateTime.UtcNow - alert.LastAlertDate.Value < minInterval)
                continue;

            try
            {
                await notificationService.SendReminderAsync(alert.Id).ConfigureAwait(false);
                alert.LastAlertDate = DateTime.UtcNow;
                alert.UpdatedAt = DateTime.UtcNow;
                _logger.LogInformation("Recordatorio enviado para alerta {AlertId} (vehículo {VehicleId})", alert.Id, alert.VehicleId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al enviar recordatorio para alerta {AlertId}", alert.Id);
            }
        }

        await context.SaveChangesAsync(stoppingToken).ConfigureAwait(false);
    }

    private static async Task<Dictionary<int, int>> GetLatestKmBatchAsync(AppDbContext context, List<int> vehicleIds, CancellationToken cancellationToken)
    {
        if (vehicleIds.Count == 0) return [];

        var latest = await context.ServiceOrders
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
