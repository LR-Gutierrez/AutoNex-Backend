using AutoNex.Data;
using AutoNex.Enums;
using AutoNex.Hubs;
using AutoNex.Models;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace AutoNex.BackgroundJobs;

[DisallowConcurrentExecution]
public class BcvFetchJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BcvFetchJob> _logger;

    public BcvFetchJob(IServiceScopeFactory scopeFactory, ILogger<BcvFetchJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var scraper = scope.ServiceProvider.GetRequiredService<IBcvScraperService>();
        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<ExchangeRateHub>>();

        var autoEnabled = await db.Settings
            .Where(s => s.Key == "bcv_auto_consult")
            .Select(s => s.Value)
            .FirstOrDefaultAsync(context.CancellationToken);

        if (autoEnabled != "true")
        {
            _logger.LogInformation("BCV auto-consulta desactivada, saltando fetch");
            return;
        }

        var result = await scraper.FetchCurrentRatesAsync(context.CancellationToken);
        if (!result.IsSuccess)
        {
            _logger.LogError("Fallo BCV fetch: {Error}", result.Error);
            return;
        }

        var dateString = result.ValueDate!.Value.ToUniversalTime().Date;
        var exists = await db.CurrencyNewsletters
            .AnyAsync(n => n.ValueDate.Date == dateString
                        && n.Status == NewsletterStatus.Draft
                        && n.IsActive, context.CancellationToken);

        if (exists)
        {
            await db.CurrencyNewsletters
                .Where(n => n.ValueDate.Date == dateString
                         && n.Status == NewsletterStatus.Draft
                         && n.IsActive)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(n => n.IsActive, false)
                    .SetProperty(n => n.Observations, n => n.Observations + " (reemplazado)"),
                    context.CancellationToken);
        }

        var newsletter = new CurrencyNewsletter
        {
            PublishedAt = DateTime.UtcNow,
            ValueDate = result.ValueDate!.Value.ToUniversalTime(),
            Observations = "Sincronización oficial BCV.",
            CreatedBy = 1,
            IpAddress = "127.0.0.1",
            IsActive = true,
            Status = NewsletterStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.CurrencyNewsletters.Add(newsletter);
        await db.SaveChangesAsync(context.CancellationToken);

        foreach (var (iso, value) in result.Rates)
        {
            var currencyId = await db.Currencies
                .Where(c => c.IsoCode == iso)
                .Select(c => c.Id)
                .FirstOrDefaultAsync(context.CancellationToken);

            if (currencyId > 0)
            {
                db.ExchangeRates.Add(new ExchangeRate
                {
                    Value = value,
                    CurrencyId = currencyId,
                    CurrencyNewsletterId = newsletter.Id,
                    CreatedBy = 1,
                    IpAddress = "127.0.0.1"
                });
            }
        }
        await db.SaveChangesAsync(context.CancellationToken);

        await hubContext.Clients.Group("exchange-updates")
            .SendAsync("ExchangeRatePublished", new { newsletterId = newsletter.Id },
                context.CancellationToken);

        _logger.LogInformation("Nuevo draft BCV creado: #{NewsletterId}", newsletter.Id);
    }
}
