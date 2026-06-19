using AutoNex.Data;
using AutoNex.DTOs.ExchangeRates;
using AutoNex.Enums;
using AutoNex.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AutoNex.Services.Implementations;

public class ExchangeRateService : IExchangeRateService
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ExchangeRateService> _logger;

    private const string CacheKey = "current_exchange_newsletter";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    public ExchangeRateService(AppDbContext db, IMemoryCache cache, ILogger<ExchangeRateService> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<NewsletterDto?> GetCurrentNewsletterAsync(CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            _logger.LogDebug("Cache miss — consultando DB");

            var newsletter = await _db.CurrencyNewsletters
                .AsNoTracking()
                .Include(n => n.ExchangeRates).ThenInclude(r => r.Currency)
                .Where(n => n.Status == NewsletterStatus.Published && n.IsActive)
                .OrderByDescending(n => n.ValueDate)
                .FirstOrDefaultAsync(ct);

            return newsletter?.ToDto();
        });
    }

    public async Task<decimal?> GetLatestValueByCodeAsync(string code, CancellationToken ct = default)
    {
        var newsletter = await GetCurrentNewsletterAsync(ct);
        return newsletter?.ExchangeRates?
            .FirstOrDefault(r => r.CurrencyCode.Equals(code, StringComparison.OrdinalIgnoreCase))
            ?.Value;
    }

    public void ClearCache()
    {
        _cache.Remove(CacheKey);
        _logger.LogInformation("Cache de tasas limpiado");
    }
}
