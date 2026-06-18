using AutoNex.Data;
using AutoNex.DTOs.Dashboard;
using AutoNex.DTOs.FinancialRecords;
using AutoNex.Enums;
using AutoNex.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoNex.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardResponse> GetDashboardAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var orderStart = ToUtc(startDate) ?? DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
        var orderEnd = ToUtc(endDate) ?? orderStart.AddDays(1);

        var orders = await GetOrdersSummaryAsync(orderStart, orderEnd, cancellationToken);
        var lowStock = await GetLowStockSummaryAsync(cancellationToken);
        var alerts = await GetAlertsSummaryAsync(cancellationToken);
        var financial = await GetFinancialSummaryAsync(orderStart, orderEnd, cancellationToken);

        return new DashboardResponse(orders, lowStock, alerts, financial);
    }

    private async Task<OrdersSummary> GetOrdersSummaryAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        var orders = await _context.ServiceOrders
            .AsNoTracking()
            .Where(o => o.Date >= start && o.Date < end && !o.IsDeleted)
            .ToListAsync(cancellationToken);

        return new OrdersSummary(
            Total: orders.Count,
            Open: orders.Count(o => o.Status == ServiceOrderStatus.Open),
            InProgress: orders.Count(o => o.Status == ServiceOrderStatus.InProgress),
            Completed: orders.Count(o => o.Status == ServiceOrderStatus.Completed),
            Paid: orders.Count(o => o.Status == ServiceOrderStatus.Paid),
            TotalAmount: orders.Where(o => o.Status == ServiceOrderStatus.Completed || o.Status == ServiceOrderStatus.Paid).Sum(o => o.TotalAmount)
        );
    }

    private async Task<LowStockSummary> GetLowStockSummaryAsync(CancellationToken cancellationToken = default)
    {
        var items = await _context.Consumables
            .AsNoTracking()
            .Where(c => c.StockQuantity <= c.MinStock)
            .OrderBy(c => c.StockQuantity)
            .Select(c => new LowStockItem(
                c.Id,
                c.Name,
                c.StockQuantity,
                c.MinStock
            ))
            .ToListAsync(cancellationToken);

        return new LowStockSummary(Items: items, TotalItems: items.Count);
    }

    private async Task<AlertsSummary> GetAlertsSummaryAsync(CancellationToken cancellationToken = default)
    {
        var pending = await _context.MileageAlerts
            .AsNoTracking()
            .Include(a => a.Vehicle)
            .Where(a => a.IsActive && !a.Vehicle.IsDeleted)
            .CountAsync(cancellationToken);

        var completed = await _context.MileageAlerts
            .AsNoTracking()
            .Include(a => a.Vehicle)
            .Where(a => !a.IsActive && !a.Vehicle.IsDeleted)
            .CountAsync(cancellationToken);

        return new AlertsSummary(
            Pending: pending,
            Completed: completed
        );
    }

    private async Task<FinancialSummaryResponse> GetFinancialSummaryAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        var records = await _context.FinancialRecords
            .AsNoTracking()
            .Where(r => r.Date >= start && r.Date < end)
            .ToListAsync(cancellationToken);

        var income = records.Where(r => r.Type == FinancialRecordType.Income).Sum(r => r.Amount);
        var expenses = records.Where(r => r.Type == FinancialRecordType.Expense).Sum(r => r.Amount);

        return new FinancialSummaryResponse(
            income,
            expenses,
            income - expenses,
            records.Count(r => r.Type == FinancialRecordType.Income),
            records.Count(r => r.Type == FinancialRecordType.Expense)
        );
    }

    public async Task<Dictionary<string, DashboardResponse>> GetMultiPeriodDashboardAsync(
        Dictionary<string, (DateTime start, DateTime end)> periods,
        CancellationToken cancellationToken = default)
    {
        var minStart = periods.Values.Min(p => p.start);
        var maxEnd = periods.Values.Max(p => p.end);

        var allOrders = await _context.ServiceOrders
            .AsNoTracking()
            .Where(o => o.Date >= minStart && o.Date < maxEnd && !o.IsDeleted)
            .ToListAsync(cancellationToken);

        var allFinancialRecords = await _context.FinancialRecords
            .AsNoTracking()
            .Where(r => r.Date >= minStart && r.Date < maxEnd)
            .ToListAsync(cancellationToken);

        var lowStock = await GetLowStockSummaryAsync(cancellationToken);
        var alerts = await GetAlertsSummaryAsync(cancellationToken);

        var result = new Dictionary<string, DashboardResponse>(periods.Count);
        foreach (var (name, (start, end)) in periods)
        {
            var periodOrders = allOrders.Where(o => o.Date >= start && o.Date < end).ToList();
            var ordersSummary = new OrdersSummary(
                periodOrders.Count,
                periodOrders.Count(o => o.Status == ServiceOrderStatus.Open),
                periodOrders.Count(o => o.Status == ServiceOrderStatus.InProgress),
                periodOrders.Count(o => o.Status == ServiceOrderStatus.Completed),
                periodOrders.Count(o => o.Status == ServiceOrderStatus.Paid),
                periodOrders.Where(o => o.Status == ServiceOrderStatus.Completed || o.Status == ServiceOrderStatus.Paid).Sum(o => o.TotalAmount)
            );

            var periodRecords = allFinancialRecords.Where(r => r.Date >= start && r.Date < end).ToList();
            var income = periodRecords.Where(r => r.Type == FinancialRecordType.Income).Sum(r => r.Amount);
            var expenses = periodRecords.Where(r => r.Type == FinancialRecordType.Expense).Sum(r => r.Amount);
            var financialSummary = new FinancialSummaryResponse(
                income, expenses, income - expenses,
                periodRecords.Count(r => r.Type == FinancialRecordType.Income),
                periodRecords.Count(r => r.Type == FinancialRecordType.Expense)
            );

            result[name] = new DashboardResponse(ordersSummary, lowStock, alerts, financialSummary);
        }

        return result;
    }

    private static DateTime? ToUtc(DateTime? dt) =>
        dt.HasValue ? (dt.Value.Kind == DateTimeKind.Utc ? dt.Value : dt.Value.ToUniversalTime()) : null;
}
