using AutoNex.Hubs;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AutoNex.Services.Implementations;

public class DashboardNotifier : IDashboardNotifier
{
    private readonly IHubContext<DashboardHub> _hubContext;
    private readonly ILogger<DashboardNotifier> _logger;

    public DashboardNotifier(
        IHubContext<DashboardHub> hubContext,
        ILogger<DashboardNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Clients.Group("dashboard").SendAsync("dashboardUpdated", cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al notificar cambios del dashboard");
        }
    }
}
