using AutoNex.DTOs.Dashboard;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AutoNex.Hubs;

[Authorize]
public class DashboardHub : Hub
{
    private readonly IDashboardService _dashboardService;

    public DashboardHub(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task JoinDashboardGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveDashboardGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task<DashboardResponse> RequestDashboardData(DateTime? startDate, DateTime? endDate)
    {
        return await _dashboardService.GetDashboardAsync(startDate, endDate);
    }
}
