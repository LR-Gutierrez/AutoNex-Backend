namespace AutoNex.DTOs.FinancialRecords;

public record DailySummaryResponse(
    string Date,
    decimal Income,
    decimal Expense
);
