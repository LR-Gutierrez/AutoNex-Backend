using AutoNex.DTOs.ExchangeRates;

namespace AutoNex.Services.Interfaces;

public interface IExchangeRateService
{
    Task<NewsletterDto?> GetCurrentNewsletterAsync(CancellationToken ct = default);
    Task<decimal?> GetLatestValueByCodeAsync(string code, CancellationToken ct = default);
    void ClearCache();
}
