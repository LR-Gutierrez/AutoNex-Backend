using AutoNex.Enums;

namespace AutoNex.DTOs.RecurringExpenses;

public record UpdateRecurringExpenseRequest(
    string Name,
    decimal Amount,
    RecurringExpenseFrequency Frequency,
    int DayOfMonth,
    AccountType AccountType,
    FinancialRecordType Type,
    bool IsActive,
    string? Description
);
