using AutoNex.Models;

namespace AutoNex.DTOs.ExchangeRates;

public static class MappingExtensions
{
    public static NewsletterDto ToDto(this CurrencyNewsletter n) => new()
    {
        Id = n.Id,
        PublishedAt = n.PublishedAt,
        ValueDate = n.ValueDate,
        Observations = n.Observations,
        Status = n.Status,
        ExchangeRates = n.ExchangeRates.Select(r => new ExchangeRateDto
        {
            Id = r.Id,
            Value = r.Value,
            CurrencyCode = r.Currency.IsoCode,
            CurrencyName = r.Currency.Name,
            CurrencySymbol = r.Currency.Symbol
        }).ToList()
    };
}
