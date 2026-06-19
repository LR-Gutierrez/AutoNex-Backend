using AutoNex.Enums;

namespace AutoNex.DTOs.ExchangeRates;

public record NewsletterDto
{
    public int Id { get; init; }
    public DateTime PublishedAt { get; init; }
    public DateTime ValueDate { get; init; }
    public string? Observations { get; init; }
    public int Status { get; init; }
    public List<ExchangeRateDto> ExchangeRates { get; init; } = [];
}
