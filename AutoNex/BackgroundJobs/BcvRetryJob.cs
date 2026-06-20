using AutoNex.Data;
using AutoNex.Enums;
using AutoNex.Hubs;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace AutoNex.BackgroundJobs;

[DisallowConcurrentExecution]
public class BcvRetryJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BcvRetryJob> _logger;

    private static readonly TimeZoneInfo VetZone = TimeZoneInfo.FindSystemTimeZoneById("America/Caracas");

    public BcvRetryJob(IServiceScopeFactory scopeFactory, ILogger<BcvRetryJob> logger)
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

        var retryEnabled = await db.Settings
            .Where(s => s.Key == "bcv_retry_enabled")
            .Select(s => s.Value)
            .FirstOrDefaultAsync(context.CancellationToken);

        if (retryEnabled != "true")
        {
            _logger.LogInformation("BCV retry desactivado, saltando reintento");
            return;
        }

        var nowVet = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VetZone);
        var todayVet = nowVet.Date;
        var todayVetUtc = TimeZoneInfo.ConvertTimeToUtc(todayVet, VetZone);
        var hourVet = nowVet.Hour;
        var minuteVet = nowVet.Minute;

        // Only run from 6:00 PM to 11:50 PM VET
        if (hourVet < 18)
        {
            _logger.LogInformation("BCV retry fuera de ventana horaria (6pm-11:50pm VET)");
            db.BcvFetchLogs.Add(new Models.BcvFetchLog
            {
                ValueDate = todayVetUtc,
                IsSuccess = true,
                Action = "Retry_Skipped_OutsideWindow",
                FetchedBy = "Retry",
                FetchedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync(context.CancellationToken);
            return;
        }

        var hasPublishedToday = await db.CurrencyNewsletters
            .AnyAsync(n => n.ValueDate.Date == todayVetUtc
                        && n.Status == NewsletterStatus.Published
                        && n.IsActive, context.CancellationToken);

        if (hasPublishedToday)
        {
            _logger.LogInformation("BCV retry omitido — ya hay Published para hoy {Today}", todayVet);
            return;
        }

        var hasDraftToday = await db.CurrencyNewsletters
            .AnyAsync(n => n.ValueDate.Date == todayVetUtc
                        && n.Status == NewsletterStatus.Draft
                        && n.IsActive, context.CancellationToken);

        if (hasDraftToday)
        {
            _logger.LogInformation("BCV retry omitido — ya hay Draft para hoy {Today}", todayVet);
            return;
        }

        _logger.LogInformation("BCV retry: sin tasa publicada para hoy ({Today}), ejecutando fetch", todayVet);
        var result = await BcvFetchJob.ExecuteFetchAsync(db, scraper, hubContext, "Retry", _logger, context.CancellationToken);

        if (result == "Retry_Inserted")
        {
            var setting = await db.Settings.FirstAsync(s => s.Key == "bcv_retry_enabled", context.CancellationToken);
            setting.Value = "false";
            setting.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("BCV retry desactivado tras insert exitoso");
        }
    }
}
