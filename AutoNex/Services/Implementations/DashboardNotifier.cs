using AutoNex.Hubs;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AutoNex.Services.Implementations;

public class DashboardNotifier : IDashboardNotifier
{
    private readonly IHubContext<DashboardHub> _hubContext;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DashboardNotifier> _logger;

    public DashboardNotifier(
        IHubContext<DashboardHub> hubContext,
        IServiceScopeFactory scopeFactory,
        ILogger<DashboardNotifier> logger)
    {
        _hubContext = hubContext;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task NotifyAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dashboardService = scope.ServiceProvider.GetRequiredService<IDashboardService>();

            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var todayData = await dashboardService.GetDashboardAsync(today, tomorrow);
            await _hubContext.Clients.Group("today").SendAsync("DashboardUpdated", todayData, cancellationToken);

            var yesterdayStart = today.AddDays(-1);
            var yesterdayData = await dashboardService.GetDashboardAsync(yesterdayStart, today);
            await _hubContext.Clients.Group("yesterday").SendAsync("DashboardUpdated", yesterdayData, cancellationToken);

            var dayOfWeek = (int)today.DayOfWeek;
            var weekStart = today.AddDays(-(dayOfWeek == 0 ? 6 : dayOfWeek - 1));
            var weekEnd = weekStart.AddDays(7);
            var weekData = await dashboardService.GetDashboardAsync(weekStart, weekEnd);
            await _hubContext.Clients.Group("this-week").SendAsync("DashboardUpdated", weekData, cancellationToken);

            var monthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1);
            var monthData = await dashboardService.GetDashboardAsync(monthStart, monthEnd);
            await _hubContext.Clients.Group("this-month").SendAsync("DashboardUpdated", monthData, cancellationToken);

            var lastMonthStart = monthStart.AddMonths(-1);
            var lastMonthData = await dashboardService.GetDashboardAsync(lastMonthStart, monthStart);
            await _hubContext.Clients.Group("last-month").SendAsync("DashboardUpdated", lastMonthData, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al notificar cambios del dashboard");
        }
    }
}
