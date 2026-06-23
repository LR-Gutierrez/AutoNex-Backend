using AutoNex.Enums;

namespace AutoNex.Models;

public class RecurringExpense
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public RecurringExpenseFrequency Frequency { get; set; }
    public int DayOfMonth { get; set; }
    public AccountType AccountType { get; set; }
    public FinancialRecordType Type { get; set; } = FinancialRecordType.Expense;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<RecurringExpenseOccurrence> Occurrences { get; set; } = [];
}
