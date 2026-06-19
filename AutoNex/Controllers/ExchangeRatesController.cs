using AutoNex.Data;
using AutoNex.DTOs.ExchangeRates;
using AutoNex.Enums;
using AutoNex.Hubs;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AutoNex.Controllers;

[ApiController]
[Route("api/exchange-rates")]
[Authorize]
public class ExchangeRatesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IExchangeRateService _rateService;
    private readonly IHubContext<ExchangeRateHub> _hubContext;

    public ExchangeRatesController(AppDbContext db, IExchangeRateService rateService, IHubContext<ExchangeRateHub> hubContext)
    {
        _db = db;
        _rateService = rateService;
        _hubContext = hubContext;
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
            .SendAsync("RateAuthorized", new { newsletterId = id });

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

    [HttpPost("autoupdate-bcv/toggle")]
    public async Task<IActionResult> ToggleAutoConsult()
    {
        var setting = await _db.Settings.FirstAsync(s => s.Key == "bcv_auto_consult");
        setting.Value = setting.Value == "true" ? "false" : "true";
        setting.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { bcv_auto_consult = setting.Value == "true" });
    }
}
