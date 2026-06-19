using AutoNex.Data;
using AutoNex.Enums;
using AutoNex.Hubs;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace AutoNex.BackgroundJobs;

[DisallowConcurrentExecution]
public class BcvActivateJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BcvActivateJob> _logger;

    public BcvActivateJob(IServiceScopeFactory scopeFactory, ILogger<BcvActivateJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var rateService = scope.ServiceProvider.GetRequiredService<IExchangeRateService>();
        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<ExchangeRateHub>>();

        var authorized = await db.CurrencyNewsletters
            .Where(n => n.Status == NewsletterStatus.Authorized && n.IsActive)
            .OrderByDescending(n => n.ValueDate)
            .FirstOrDefaultAsync(context.CancellationToken);

        if (authorized == null)
        {
            _logger.LogInformation("No hay boletines autorizados para activar");
            return;
        }

        await using var transaction = await db.Database.BeginTransactionAsync(context.CancellationToken);

        await db.CurrencyNewsletters
            .Where(n => n.Status == NewsletterStatus.Published && n.IsActive)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(n => n.Status, NewsletterStatus.Historical)
                .SetProperty(n => n.UpdatedAt, DateTime.UtcNow),
                context.CancellationToken);

        authorized.Status = NewsletterStatus.Published;
        authorized.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(context.CancellationToken);

        await transaction.CommitAsync(context.CancellationToken);

        rateService.ClearCache();

        var usdRate = await rateService.GetLatestValueByCodeAsync("USD", context.CancellationToken);

        await hubContext.Clients.Group("exchange-updates")
            .SendAsync("RateInEffect", new { newsletterId = authorized.Id },
                context.CancellationToken);

        if (usdRate.HasValue)
        {
            await hubContext.Clients.Group("exchange-rates-public")
                .SendAsync("LiveUpdate", new { currency = "USD", value = usdRate.Value },
                    context.CancellationToken);
        }

        _logger.LogInformation("Boletín #{NewsletterId} activado", authorized.Id);
    }
}
