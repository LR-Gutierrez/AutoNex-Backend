using AutoNex.Enums;

namespace AutoNex.Models;

public class RecurringExpenseOccurrence
{
    public int Id { get; set; }
    public int RecurringExpenseId { get; set; }
    public DateOnly DueDate { get; set; }
    public decimal Amount { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaidDate { get; set; }
    public AccountType? PaidAccountType { get; set; }
    public int? FinancialRecordId { get; set; }
    public DateOnly? DismissedDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public RecurringExpense RecurringExpense { get; set; } = null!;
    public FinancialRecord? FinancialRecord { get; set; }
}
