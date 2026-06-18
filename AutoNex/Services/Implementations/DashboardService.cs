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

    public async Task<DashboardResponse> GetDashboardAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var orderStart = ToUtc(startDate) ?? DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
        var orderEnd = ToUtc(endDate) ?? orderStart.AddDays(1);

        var orders = await GetOrdersSummaryAsync(orderStart, orderEnd);
        var lowStock = await GetLowStockSummaryAsync();
        var alerts = await GetAlertsSummaryAsync();
        var financial = await GetFinancialSummaryAsync(orderStart, orderEnd);

        return new DashboardResponse(orders, lowStock, alerts, financial);
    }

    private async Task<OrdersSummary> GetOrdersSummaryAsync(DateTime start, DateTime end)
    {
        var orders = await _context.ServiceOrders
            .Where(o => o.Date >= start && o.Date < end && !o.IsDeleted)
            .ToListAsync();

        return new OrdersSummary(
            Total: orders.Count,
            Open: orders.Count(o => o.Status == ServiceOrderStatus.Open),
            InProgress: orders.Count(o => o.Status == ServiceOrderStatus.InProgress),
            Completed: orders.Count(o => o.Status == ServiceOrderStatus.Completed),
            TotalAmount: orders.Where(o => o.Status == ServiceOrderStatus.Completed).Sum(o => o.TotalAmount)
        );
    }

    private async Task<LowStockSummary> GetLowStockSummaryAsync()
    {
        var items = await _context.Consumables
            .Where(c => c.StockQuantity <= c.MinStock)
            .OrderBy(c => c.StockQuantity)
            .Select(c => new LowStockItem(
                c.Id,
                c.Name,
                c.StockQuantity,
                c.MinStock
            ))
            .ToListAsync();

        return new LowStockSummary(Items: items, TotalItems: items.Count);
    }

    private async Task<AlertsSummary> GetAlertsSummaryAsync()
    {
        var activeAlerts = await _context.MileageAlerts
            .Include(a => a.Vehicle)
            .Where(a => a.IsActive && !a.Vehicle.IsDeleted)
            .ToListAsync();

        var dueCount = activeAlerts.Count(a =>
            a.NextAlertDate != null && DateTime.UtcNow >= a.NextAlertDate
        );

        return new AlertsSummary(
            Active: activeAlerts.Count,
            Overdue: dueCount
        );
    }

    private async Task<FinancialSummaryResponse> GetFinancialSummaryAsync(DateTime start, DateTime end)
    {
        var records = await _context.FinancialRecords
            .Where(r => r.Date >= start && r.Date < end)
            .ToListAsync();

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

    private static DateTime? ToUtc(DateTime? dt) =>
        dt.HasValue ? (dt.Value.Kind == DateTimeKind.Utc ? dt.Value : dt.Value.ToUniversalTime()) : null;
}
