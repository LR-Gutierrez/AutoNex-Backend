namespace AutoNex.DTOs.ExchangeRates;

public record CreateNewsletterRequest
{
    public DateTime PublishedAt { get; init; }
    public DateTime ValueDate { get; init; }
    public string? Observations { get; init; }
    public List<RateEntry> Rates { get; init; } = [];

    public record RateEntry
    {
        public int CurrencyId { get; init; }
        public decimal Value { get; init; }
    }
}
