using AutoNex.DTOs.FinancialRecords;

namespace AutoNex.DTOs.Dashboard;

public record DashboardResponse(
    OrdersSummary OrdersToday,
    LowStockSummary LowStock,
    AlertsSummary KmAlerts,
    FinancialSummaryResponse FinancialMonth
);

public record OrdersSummary(
    int Total,
    int Open,
    int InProgress,
    int Completed,
    int Paid,
    decimal TotalAmount
);

public record LowStockSummary(
    int TotalItems,
    List<LowStockItem> Items
);

public record LowStockItem(
    int Id,
    string Name,
    int StockQuantity,
    int MinStock
);

public record AlertsSummary(
    int Pending,
    int Completed
);
