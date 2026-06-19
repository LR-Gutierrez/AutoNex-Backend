using AutoNex.Enums;

namespace AutoNex.Models;

public class CurrencyNewsletter
{
    public int Id { get; set; }
    public DateTime PublishedAt { get; set; }
    public DateTime ValueDate { get; set; }
    public string? Observations { get; set; }
    public int CreatedBy { get; set; }
    public string? IpAddress { get; set; }
    public bool IsActive { get; set; } = true;
    public NewsletterStatus Status { get; set; } = NewsletterStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ExchangeRate> ExchangeRates { get; set; } = [];
}
