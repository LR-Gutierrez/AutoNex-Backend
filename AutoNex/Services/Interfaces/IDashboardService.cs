using AutoNex.DTOs.Dashboard;

namespace AutoNex.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardResponse> GetDashboardAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<Dictionary<string, DashboardResponse>> GetMultiPeriodDashboardAsync(
        Dictionary<string, (DateTime start, DateTime end)> periods,
        CancellationToken cancellationToken = default);
}
