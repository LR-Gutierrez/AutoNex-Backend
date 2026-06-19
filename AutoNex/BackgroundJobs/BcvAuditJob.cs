using AutoNex.Data;
using AutoNex.Enums;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace AutoNex.BackgroundJobs;

[DisallowConcurrentExecution]
public class BcvAuditJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BcvAuditJob> _logger;

    public BcvAuditJob(IServiceScopeFactory scopeFactory, ILogger<BcvAuditJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var hasPublished = await db.CurrencyNewsletters
            .AnyAsync(n => n.Status == NewsletterStatus.Published && n.IsActive,
                context.CancellationToken);

        if (!hasPublished)
        {
            _logger.LogWarning("No hay boletines publicados activos — forzando fetch manual");

            var schedulerFactory = scope.ServiceProvider.GetRequiredService<ISchedulerFactory>();
            var scheduler = await schedulerFactory.GetScheduler(context.CancellationToken);
            await scheduler.TriggerJob(new JobKey("bcv-fetch"), context.CancellationToken);
        }
        else
        {
            _logger.LogInformation("Auditoría BCV OK — hay boletines publicados");
        }
    }
}
