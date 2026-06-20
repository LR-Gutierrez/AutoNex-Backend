using AutoNex.BackgroundJobs;
using AutoNex.Data;
using AutoNex.DTOs.ExchangeRates;
using AutoNex.Enums;
using AutoNex.Hubs;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace AutoNex.Controllers;

[ApiController]
[Route("api/exchange-rates")]
[Authorize]
public class ExchangeRatesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IExchangeRateService _rateService;
    private readonly IHubContext<ExchangeRateHub> _hubContext;
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly IBcvScraperService _scraper;
    private readonly ILogger<ExchangeRatesController> _logger;

    public ExchangeRatesController(AppDbContext db, IExchangeRateService rateService,
        IHubContext<ExchangeRateHub> hubContext, ISchedulerFactory schedulerFactory,
        IBcvScraperService scraper, ILogger<ExchangeRatesController> logger)
    {
        _db = db;
        _rateService = rateService;
        _hubContext = hubContext;
        _schedulerFactory = schedulerFactory;
        _scraper = scraper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int perPage = 50)
    {
        var query = _db.CurrencyNewsletters
            .AsNoTracking()
            .Include(n => n.ExchangeRates).ThenInclude(r => r.Currency)
            .OrderByDescending(n => n.CreatedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .Select(n => n.ToDto())
            .ToListAsync();

        return Ok(new { data = items, total, page, perPage });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var newsletter = await _db.CurrencyNewsletters
            .AsNoTracking()
            .Include(n => n.ExchangeRates).ThenInclude(r => r.Currency)
            .FirstOrDefaultAsync(n => n.Id == id);

        if (newsletter == null) return NotFound();
        return Ok(newsletter.ToDto());
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CreateNewsletterRequest request)
    {
        var newsletter = await _db.CurrencyNewsletters
            .Include(n => n.ExchangeRates)
            .FirstOrDefaultAsync(n => n.Id == id);

        if (newsletter == null) return NotFound();

        if (newsletter.Status != NewsletterStatus.Draft)
            return BadRequest("Solo se pueden editar boletines en estado Draft");

        newsletter.PublishedAt = request.PublishedAt;
        newsletter.ValueDate = request.ValueDate;
        newsletter.Observations = request.Observations;
        newsletter.UpdatedAt = DateTime.UtcNow;

        _db.ExchangeRates.RemoveRange(newsletter.ExchangeRates);

        var newRates = new List<Models.ExchangeRate>();
        foreach (var rate in request.Rates)
        {
            var er = new Models.ExchangeRate
            {
                Value = rate.Value,
                CurrencyId = rate.CurrencyId,
                CurrencyNewsletterId = newsletter.Id,
                CreatedBy = newsletter.CreatedBy,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1"
            };
            newRates.Add(er);
            _db.ExchangeRates.Add(er);
        }

        await _db.SaveChangesAsync();

        var currencies = await _db.Currencies.AsNoTracking().ToListAsync();
        var dtoRates = newRates.Select(r => new ExchangeRateDto
        {
            Id = r.Id,
            Value = r.Value,
            CurrencyCode = currencies.First(c => c.Id == r.CurrencyId).IsoCode,
            CurrencyName = currencies.First(c => c.Id == r.CurrencyId).Name,
            CurrencySymbol = currencies.First(c => c.Id == r.CurrencyId).Symbol
        }).ToList();

        return Ok(new NewsletterDto
        {
            Id = newsletter.Id,
            PublishedAt = newsletter.PublishedAt,
            ValueDate = newsletter.ValueDate,
            Observations = newsletter.Observations,
            Status = (int)newsletter.Status,
            ExchangeRates = dtoRates
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateNewsletterRequest request)
    {
        var newsletter = new Models.CurrencyNewsletter
        {
            PublishedAt = request.PublishedAt,
            ValueDate = request.ValueDate,
            Observations = request.Observations,
            CreatedBy = 1,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
            Status = NewsletterStatus.Draft
        };

        _db.CurrencyNewsletters.Add(newsletter);
        await _db.SaveChangesAsync();

        foreach (var rate in request.Rates)
        {
            _db.ExchangeRates.Add(new Models.ExchangeRate
            {
                Value = rate.Value,
                CurrencyId = rate.CurrencyId,
                CurrencyNewsletterId = newsletter.Id,
                CreatedBy = 1,
                IpAddress = newsletter.IpAddress
            });
        }
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = newsletter.Id }, newsletter.ToDto());
    }

    [HttpPost("{id}/authorize")]
    public async Task<IActionResult> Authorize(int id)
    {
        var newsletter = await _db.CurrencyNewsletters.FindAsync(id);
        if (newsletter == null) return NotFound();

        if (newsletter.Status != NewsletterStatus.Draft)
            return BadRequest("Solo se pueden autorizar boletines en estado Draft");

        newsletter.Status = NewsletterStatus.Authorized;
        newsletter.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _rateService.ClearCache();

        await _hubContext.Clients.Group("exchange-updates")
            .SendAsync("ExchangeRateAuthorized", new { newsletterId = id });

        return Ok(new { message = "Boletín autorizado correctamente" });
    }

    [AllowAnonymous]
    [HttpGet("live/{currency}")]
    public async Task<IActionResult> GetLiveRate(string currency)
    {
        var value = await _rateService.GetLatestValueByCodeAsync(currency);
        if (value == null) return NotFound();
        return Ok(new { currency = currency.ToUpper(), value });
    }

    [HttpPost("{id}/disable")]
    public async Task<IActionResult> Disable(int id)
    {
        var newsletter = await _db.CurrencyNewsletters.FindAsync(id);
        if (newsletter == null) return NotFound();

        newsletter.IsActive = false;
        newsletter.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _rateService.ClearCache();
        return Ok(new { message = "Boletín desactivado" });
    }

    [HttpPost("{id}/enable")]
    public async Task<IActionResult> Enable(int id)
    {
        var newsletter = await _db.CurrencyNewsletters.FindAsync(id);
        if (newsletter == null) return NotFound();

        newsletter.IsActive = true;
        newsletter.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _rateService.ClearCache();
        return Ok(new { message = "Boletín reactivado" });
    }

    [HttpPost("bcv-fetch")]
    public async Task<IActionResult> BcvFetch()
    {
        await BcvFetchJob.ExecuteFetchAsync(_db, _scraper, _hubContext, "Manual", _logger);
        var lastLog = await _db.BcvFetchLogs
            .AsNoTracking()
            .OrderByDescending(l => l.Id)
            .Select(l => new BcvFetchLogResponse(
                l.Id, l.ValueDate, l.RatesJson, l.IsSuccess,
                l.Error, l.Action, l.FetchedBy, l.FetchedAt))
            .FirstOrDefaultAsync();

        if (lastLog?.Action?.EndsWith("_Inserted") == true)
        {
            var setting = await _db.Settings.FirstAsync(s => s.Key == "bcv_retry_enabled");
            setting.Value = "false";
            setting.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            _logger.LogInformation("BCV retry desactivado tras insert manual exitoso");
        }

        return Ok(lastLog);
    }

    [HttpGet("fetch-logs")]
    public async Task<IActionResult> GetFetchLogs([FromQuery] int page = 1, [FromQuery] int perPage = 20)
    {
        var query = _db.BcvFetchLogs.AsNoTracking().OrderByDescending(l => l.FetchedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .Select(l => new BcvFetchLogResponse(
                l.Id, l.ValueDate, l.RatesJson, l.IsSuccess,
                l.Error, l.Action, l.FetchedBy, l.FetchedAt))
            .ToListAsync();

        return Ok(new { data = items, total, page, perPage });
    }

    [HttpGet("autoupdate-bcv/status")]
    public async Task<IActionResult> GetAutoConsultStatus()
    {
        var settings = await _db.Settings
            .AsNoTracking()
            .Where(s => s.Key == "bcv_auto_consult" || s.Key == "bcv_retry_enabled")
            .ToListAsync();

        DateTime? nextRetryUtc = null;
        try
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var triggers = await scheduler.GetTriggersOfJob(new JobKey("bcv-retry"));
            var next = triggers.MinBy(t => t.GetNextFireTimeUtc())?.GetNextFireTimeUtc();
            nextRetryUtc = next?.UtcDateTime;
        }
        catch { /* scheduler not available */ }

        return Ok(new
        {
            bcv_auto_consult = settings.FirstOrDefault(s => s.Key == "bcv_auto_consult")?.Value == "true",
            bcv_retry_enabled = settings.FirstOrDefault(s => s.Key == "bcv_retry_enabled")?.Value == "true",
            nextRetryUtc,
        });
    }

    [HttpPost("autoupdate-bcv/toggle")]
    public async Task<IActionResult> ToggleAutoConsult()
    {
        var setting = await _db.Settings.FirstAsync(s => s.Key == "bcv_auto_consult");
        setting.Value = setting.Value == "true" ? "false" : "true";
        setting.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var isActive = setting.Value == "true";

        if (isActive)
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            await scheduler.TriggerJob(new JobKey("bcv-fetch"));
        }

        return Ok(new { bcv_auto_consult = isActive });
    }
}
