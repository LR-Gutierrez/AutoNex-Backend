using AutoNex.Enums;

namespace AutoNex.DTOs.FinancialRecords;

public record AccountBalanceDto(
    AccountType AccountType,
    decimal Balance,
    string Currency
);

public record FinancialSummaryResponse(
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal Balance,
    int IncomeCount,
    int ExpenseCount,
    List<AccountBalanceDto> Balances
);
