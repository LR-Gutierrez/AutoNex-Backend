using AutoNex.Enums;

namespace AutoNex.DTOs.RecurringExpenses;

public record RecurringExpenseResponse(
    int Id,
    string Name,
    decimal Amount,
    RecurringExpenseFrequency Frequency,
    int DayOfMonth,
    AccountType AccountType,
    FinancialRecordType Type,
    string? Description,
    bool IsActive,
    DateTime CreatedAt
);
