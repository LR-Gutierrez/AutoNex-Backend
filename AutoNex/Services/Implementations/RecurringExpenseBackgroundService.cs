using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using AutoNex.Hubs;

namespace AutoNex.Services.Implementations;

public class RecurringExpenseBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RecurringExpenseBackgroundService> _logger;
    private const int CheckIntervalHours = 6;

    public RecurringExpenseBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<RecurringExpenseBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RecurringExpenseBackgroundService iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await GenerateAndNotifyAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar gastos recurrentes");
            }

            await Task.Delay(TimeSpan.FromHours(CheckIntervalHours), stoppingToken);
        }
    }

    private async Task GenerateAndNotifyAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var recurringService = scope.ServiceProvider.GetRequiredService<IRecurringExpenseService>();
        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<DashboardHub>>();

        await recurringService.GenerateOccurrencesAsync(stoppingToken);

        var dueToday = await recurringService.GetDueTodayAsync(stoppingToken);

        if (dueToday.Count > 0)
        {
            _logger.LogInformation("Notificando {Count} gastos recurrentes vencidos", dueToday.Count);
            await hubContext.Clients.Group("dashboard")
                .SendAsync("recurringExpensesDue", dueToday, stoppingToken);
        }
    }
}
