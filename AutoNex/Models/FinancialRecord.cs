using AutoNex.Enums;

namespace AutoNex.Models;

public class FinancialRecord
{
    public int Id { get; set; }
    public FinancialRecordType Type { get; set; }
    public FinancialCategory Category { get; set; }
    public decimal Amount { get; set; }
    public AccountType AccountType { get; set; }
    public decimal? AmountInBs { get; set; }
    public decimal? ExchangeRateValue { get; set; }
    public string? Description { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public int UserId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
