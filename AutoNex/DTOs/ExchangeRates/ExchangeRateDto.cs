namespace AutoNex.DTOs.ExchangeRates;

public record ExchangeRateDto
{
    public int Id { get; init; }
    public decimal Value { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public string CurrencyName { get; init; } = string.Empty;
    public string CurrencySymbol { get; init; } = string.Empty;
}
