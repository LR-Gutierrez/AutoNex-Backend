using AutoNex.DTOs.Dashboard;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AutoNex.Hubs;

[Authorize]
public class DashboardHub : Hub
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardHub> _logger;

    public DashboardHub(IDashboardService dashboardService, ILogger<DashboardHub> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var now = DateTime.UtcNow;

        await Groups.AddToGroupAsync(Context.ConnectionId, "dashboard");
        await Groups.AddToGroupAsync(Context.ConnectionId, "today");

        var yesterday = now.AddDays(-1).Date;
        await Groups.AddToGroupAsync(Context.ConnectionId, "yesterday");

        var dayOfWeek = (int)now.DayOfWeek;
        var weekStart = now.AddDays(dayOfWeek == 0 ? -6 : -(dayOfWeek - 1)).Date;
        await Groups.AddToGroupAsync(Context.ConnectionId, "this-week");

        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        await Groups.AddToGroupAsync(Context.ConnectionId, "this-month");
        await Groups.AddToGroupAsync(Context.ConnectionId, "last-month");

        await base.OnConnectedAsync();
    }

    public async Task JoinDashboardGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveDashboardGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task<DashboardResponse?> RequestDashboardData(DateTime? startDate, DateTime? endDate)
    {
        try
        {
            return await _dashboardService.GetDashboardAsync(startDate, endDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener datos del dashboard");
            throw;
        }
    }
}
