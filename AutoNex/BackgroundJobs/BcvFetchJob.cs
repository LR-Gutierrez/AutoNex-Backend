using System.Text.Json;
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

        var action = await ExecuteFetchAsync(db, scraper, hubContext, "Auto", _logger, context.CancellationToken);

        if (action == "Auto_Inserted")
        {
            var setting = await db.Settings.FirstAsync(s => s.Key == "bcv_retry_enabled", context.CancellationToken);
            setting.Value = "false";
            setting.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("BCV retry desactivado tras Auto Inserted");
        }
    }

    public static async Task<string> ExecuteFetchAsync(AppDbContext db, IBcvScraperService scraper,
        IHubContext<ExchangeRateHub> hubContext, string fetchedBy, ILogger logger,
        CancellationToken ct = default)
    {
        var result = await scraper.FetchCurrentRatesAsync(ct);

        if (!result.IsSuccess)
        {
            db.BcvFetchLogs.Add(new BcvFetchLog
            {
                ValueDate = DateTime.UtcNow,
                IsSuccess = false,
                Error = result.Error,
                Action = $"{fetchedBy}_Failed",
                FetchedBy = fetchedBy,
                FetchedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync(ct);
            logger.LogError("{FetchedBy} BCV fetch falló: {Error}", fetchedBy, result.Error);
            return $"{fetchedBy}_Failed";
        }

        var valueDate = result.ValueDate!.Value.ToUniversalTime().Date;
        var ratesJson = JsonSerializer.Serialize(result.Rates);

        var alreadyPublished = await db.CurrencyNewsletters
            .AnyAsync(n => n.ValueDate.Date == valueDate
                        && n.Status == NewsletterStatus.Published
                        && n.IsActive, ct);

        if (alreadyPublished)
        {
            db.BcvFetchLogs.Add(new BcvFetchLog
            {
                ValueDate = valueDate,
                RatesJson = ratesJson,
                IsSuccess = true,
                Action = $"{fetchedBy}_Skipped_AlreadyPublished",
                FetchedBy = fetchedBy,
                FetchedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync(ct);
            logger.LogInformation("{FetchedBy} BCV fetch omitido — ya hay Publicado para {ValueDate}", fetchedBy, valueDate);
            return $"{fetchedBy}_Skipped_AlreadyPublished";
        }

        var existingDraft = await db.CurrencyNewsletters
            .AnyAsync(n => n.ValueDate.Date == valueDate
                        && n.Status == NewsletterStatus.Draft
                        && n.IsActive, ct);

        if (existingDraft)
        {
            db.BcvFetchLogs.Add(new BcvFetchLog
            {
                ValueDate = valueDate,
                RatesJson = ratesJson,
                IsSuccess = true,
                Action = $"{fetchedBy}_Skipped_AlreadyDraft",
                FetchedBy = fetchedBy,
                FetchedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync(ct);
            logger.LogInformation("{FetchedBy} BCV fetch omitido — ya hay Draft para {ValueDate}", fetchedBy, valueDate);
            return $"{fetchedBy}_Skipped_AlreadyDraft";
        }

        var newsletter = new CurrencyNewsletter
        {
            PublishedAt = DateTime.UtcNow,
            ValueDate = valueDate,
            Observations = "Sincronización oficial BCV.",
            CreatedBy = 1,
            IpAddress = "127.0.0.1",
            IsActive = true,
            Status = NewsletterStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.CurrencyNewsletters.Add(newsletter);
        await db.SaveChangesAsync(ct);

        foreach (var (iso, rate) in result.Rates)
        {
            var currencyId = await db.Currencies
                .Where(c => c.IsoCode == iso)
                .Select(c => c.Id)
                .FirstOrDefaultAsync(ct);

            if (currencyId > 0)
            {
                db.ExchangeRates.Add(new ExchangeRate
                {
                    Value = rate,
                    CurrencyId = currencyId,
                    CurrencyNewsletterId = newsletter.Id,
                    CreatedBy = 1,
                    IpAddress = "127.0.0.1"
                });
            }
        }

        db.BcvFetchLogs.Add(new BcvFetchLog
        {
            ValueDate = valueDate,
            RatesJson = ratesJson,
            IsSuccess = true,
            Action = $"{fetchedBy}_Inserted",
            FetchedBy = fetchedBy,
            FetchedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);

        await hubContext.Clients.Group("exchange-updates")
            .SendAsync("ExchangeRatePublished", new { newsletterId = newsletter.Id }, ct);

        logger.LogInformation("{FetchedBy} BCV nuevo Draft #{NewsletterId} para {ValueDate}", fetchedBy, newsletter.Id, valueDate);
        return $"{fetchedBy}_Inserted";
    }
}
