using AutoNex.Enums;

namespace AutoNex.DTOs.RecurringExpenses;

public record RecurringExpenseOccurrenceResponse(
    int Id,
    int RecurringExpenseId,
    string ExpenseName,
    DateOnly DueDate,
    decimal Amount,
    bool IsPaid,
    DateTime? PaidDate,
    AccountType? PaidAccountType,
    DateOnly? DismissedDate
);
