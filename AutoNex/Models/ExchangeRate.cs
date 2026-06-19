namespace AutoNex.Models;

public class ExchangeRate
{
    public int Id { get; set; }
    public decimal Value { get; set; }
    public int CurrencyId { get; set; }
    public int CurrencyNewsletterId { get; set; }
    public int CreatedBy { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Currency Currency { get; set; } = null!;
    public CurrencyNewsletter Newsletter { get; set; } = null!;
}
