using AutoNex.Enums;

namespace AutoNex.DTOs.RecurringExpenses;

public record CreateRecurringExpenseRequest(
    string Name,
    decimal Amount,
    RecurringExpenseFrequency Frequency,
    int DayOfMonth,
    AccountType AccountType,
    FinancialRecordType Type,
    string? Description
);
