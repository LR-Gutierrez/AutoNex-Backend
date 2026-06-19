using AutoNex.Data;
using AutoNex.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoNex.Services.Implementations;

public class MileageAlertBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MileageAlertBackgroundService> _logger;

    public MileageAlertBackgroundService(IServiceProvider serviceProvider, ILogger<MileageAlertBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MileageAlertBackgroundService iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDueAlertsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar alertas de kilometraje");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task ProcessDueAlertsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var dueAlerts = await context.MileageAlerts
            .Include(a => a.Vehicle)
            .Where(a => a.IsActive && !a.Vehicle.IsDeleted)
            .Where(a => a.NextAlertDate != null && DateTime.UtcNow >= a.NextAlertDate
                || context.ServiceOrders
                    .Where(o => o.VehicleId == a.VehicleId && o.Status != Enums.ServiceOrderStatus.Cancelled)
                    .OrderByDescending(o => o.Date)
                    .Select(o => o.CurrentKm)
                    .FirstOrDefault() + (a.EstimatedWeeklyKm * 2) >= a.NextAlertKm)
            .ToListAsync(stoppingToken);

        foreach (var alert in dueAlerts)
        {
            try
            {
                await notificationService.SendReminderAsync(alert.Id);
                alert.LastAlertDate = DateTime.UtcNow;
                alert.UpdatedAt = DateTime.UtcNow;
                _logger.LogInformation("Recordatorio enviado para alerta {AlertId} (vehículo {VehicleId})", alert.Id, alert.VehicleId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al enviar recordatorio para alerta {AlertId}", alert.Id);
            }
        }

        await context.SaveChangesAsync(stoppingToken);
    }
}
