using AutoNex.Data;
using AutoNex.DTOs.ExchangeRates;
using AutoNex.Enums;
using AutoNex.Models;
using AutoNex.Services.Implementations;
using AutoNex.Tests.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutoNex.Tests.Services;

public class ExchangeRateServiceTests
{
    [Fact]
    public async Task GetCurrentNewsletterAsync_NoPublished_ReturnsNull()
    {
        var context = TestDbContextFactory.Create();
        var service = CreateService(context);

        var result = await service.GetCurrentNewsletterAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCurrentNewsletterAsync_WithPublished_ReturnsNewsletter()
    {
        var context = TestDbContextFactory.Create();
        SeedPublishedNewsletter(context);
        var service = CreateService(context);

        var result = await service.GetCurrentNewsletterAsync();

        Assert.NotNull(result);
        Assert.Equal((int)NewsletterStatus.Published, result.Status);
        Assert.NotEmpty(result.ExchangeRates);
        Assert.Equal("USD", result.ExchangeRates[0].CurrencyCode);
        Assert.Equal(50.25m, result.ExchangeRates[0].Value);
    }

    [Fact]
    public async Task GetLatestValueByCodeAsync_WithPublished_ReturnsRate()
    {
        var context = TestDbContextFactory.Create();
        SeedPublishedNewsletter(context);
        var service = CreateService(context);

        var usd = await service.GetLatestValueByCodeAsync("USD");
        var eur = await service.GetLatestValueByCodeAsync("EUR");
        var unknown = await service.GetLatestValueByCodeAsync("GBP");

        Assert.Equal(50.25m, usd);
        Assert.Equal(55.10m, eur);
        Assert.Null(unknown);
    }

    [Fact]
    public async Task GetLatestValueByCodeAsync_CaseInsensitive()
    {
        var context = TestDbContextFactory.Create();
        SeedPublishedNewsletter(context);
        var service = CreateService(context);

        var result = await service.GetLatestValueByCodeAsync("usd");

        Assert.Equal(50.25m, result);
    }

    [Fact]
    public void ClearCache_Invalidates()
    {
        var context = TestDbContextFactory.Create();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var logger = Mock.Of<ILogger<ExchangeRateService>>();
        var service = new ExchangeRateService(context, cache, logger);

        cache.Set("current_exchange_newsletter", new NewsletterDto());
        Assert.NotNull(cache.Get("current_exchange_newsletter"));

        service.ClearCache();

        Assert.Null(cache.Get("current_exchange_newsletter"));
    }

    private static ExchangeRateService CreateService(AppDbContext context)
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var logger = Mock.Of<ILogger<ExchangeRateService>>();
        return new ExchangeRateService(context, cache, logger);
    }

    private static void SeedPublishedNewsletter(AppDbContext context)
    {
        var usd = new Currency { IsoCode = "USD", Name = "Dólar estadounidense", Symbol = "$", IsPrincipal = true };
        var eur = new Currency { IsoCode = "EUR", Name = "Euro", Symbol = "€", IsPrincipal = false };

        context.Set<Currency>().AddRange(usd, eur);
        context.SaveChanges();

        var newsletter = new CurrencyNewsletter
        {
            PublishedAt = DateTime.UtcNow,
            ValueDate = DateTime.UtcNow.Date,
            Status = NewsletterStatus.Published,
            IsActive = true,
            CreatedBy = 1
        };
        context.Set<CurrencyNewsletter>().Add(newsletter);
        context.SaveChanges();

        context.Set<ExchangeRate>().AddRange(
            new ExchangeRate { Value = 50.25m, CurrencyId = usd.Id, Newsletter = newsletter, CreatedBy = 1 },
            new ExchangeRate { Value = 55.10m, CurrencyId = eur.Id, Newsletter = newsletter, CreatedBy = 1 }
        );
        context.SaveChanges();
    }
}
