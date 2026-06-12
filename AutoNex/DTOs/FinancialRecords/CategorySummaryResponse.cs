namespace AutoNex.DTOs.FinancialRecords;

public record CategorySummaryResponse(
    string Category,
    decimal TotalAmount,
    int Count
);
