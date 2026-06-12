namespace AutoNex.DTOs.FinancialRecords;

public record FinancialSummaryResponse(
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal Balance,
    int IncomeCount,
    int ExpenseCount
);
