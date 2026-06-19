using AutoNex.DTOs.ExchangeRates;

namespace AutoNex.Services.Interfaces;

public interface IBcvScraperService
{
    Task<BcvScrapeResult> FetchCurrentRatesAsync(CancellationToken ct = default);
}
