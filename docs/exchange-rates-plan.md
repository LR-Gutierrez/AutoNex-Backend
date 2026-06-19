# Plan de Implementación — Tasas de Cambio BCV

Stack: **.NET 10 · ASP.NET Core · EF Core + Npgsql · SignalR · JWT Bearer · FluentValidation · BCrypt · Quartz.NET · HtmlAgilityPack**

---

## Tabla de Contenidos

1. [Arquitectura General](#1-arquitectura-general)
2. [Estructura del Proyecto](#2-estructura-del-proyecto)
3. [Base de Datos — Migraciones y Entidades](#3-base-de-datos--migraciones-y-entidades)
4. [Servicio de Scraping BCV](#4-servicio-de-scraping-bcv)
5. [Servicio de Consulta de Tasas (Cacheado)](#5-servicio-de-consulta-de-tasas-cacheado)
6. [Background Jobs con Quartz.NET](#6-background-jobs-con-quartznetscheduler)
7. [SignalR Hubs — Tiempo Real](#7-signalr-hubs--tiempo-real)
8. [Controladores REST API](#8-controladores-rest-api)
9. [JWT Authentication + SignalR](#9-jwt-authentication--signalr)
10. [FluentValidation](#10-fluentvalidation)
11. [Seeders — Datos Iniciales](#11-seeders--datos-iniciales)
12. [DI y Program.cs](#12-di-y-programcs)
13. [Checklist de Paquetes NuGet](#13-checklist-de-paquetes-nuget)
14. [Diagrama de Flujo Final](#14-diagrama-de-flujo-final)

---

## 1. Arquitectura General

```
┌──────────────────────────────────────────────────────────────────┐
│                    QUARTZ SCHEDULER (Jobs)                        │
│                                                                   │
│  Settings (DB):                                                   │
│   bcv_auto_consult (bool) ───► BcvFetchJob                        │
│   bcv_update_cron (cron)        Lun-Vie 4:10PM VET                │
│                                                                   │
│   bcv_audit_enabled (bool) ───► BcvAuditJob                       │
│   bcv_audit_cron (cron)          6:00PM VET (auto-recovery)       │
│                                                                   │
│   always ──► BcvActivateJob       00:00 VET (publicar tasas)     │
└──────────────────────────────────────────────────────────────────┘
```

---

## 2. Estructura del Proyecto

```
src/BcvExchangeRates/
├── BcvExchangeRates.sln
│
├── Domain/
│   ├── Entities/
│   │   ├── Currency.cs
│   │   ├── CurrencyNewsletter.cs         # boletín
│   │   ├── ExchangeRate.cs               # tasa individual
│   │   └── Setting.cs                    # config dinámica (bcv_*)
│   └── Enums/
│       └── NewsletterStatus.cs           # Draft=1, Authorized=2, Published=3, Historical=4
│
├── Application/
│   ├── Services/
│   │   ├── IBcvScraperService.cs
│   │   ├── BcvScraperService.cs           # scraping del portal BCV
│   │   ├── IExchangeRateService.cs
│   │   └── ExchangeRateService.cs         # consulta cacheada
│   ├── DTOs/
│   │   ├── NewsletterDto.cs
│   │   ├── ExchangeRateDto.cs
│   │   ├── CreateNewsletterRequest.cs
│   │   └── BcvScrapeResult.cs
│   └── Validators/
│       ├── CreateNewsletterValidator.cs
│       └── ToggleBcvSettingsValidator.cs
│
├── Infrastructure/
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   ├── Configurations/
│   │   │   ├── CurrencyConfiguration.cs
│   │   │   ├── CurrencyNewsletterConfiguration.cs
│   │   │   ├── ExchangeRateConfiguration.cs
│   │   │   └── SettingConfiguration.cs
│   │   └── Seeders/
│   │       └── AppDbSeeder.cs
│   ├── BackgroundJobs/
│   │   ├── BcvFetchJob.cs
│   │   ├── BcvActivateJob.cs
│   │   └── BcvAuditJob.cs
│   └── DependencyInjection.cs
│
└── Api/
    ├── Controllers/
    │   ├── AuthController.cs
    │   ├── ExchangeRatesController.cs
    │   └── SettingsController.cs
    ├── Hubs/
    │   └── ExchangeRateHub.cs
    ├── Middleware/
    │   └── ErrorHandlingMiddleware.cs
    ├── Program.cs
    └── appsettings.json
```

---

## 3. Base de Datos — Migraciones y Entidades

### 3.1 AppDbContext

```csharp
public class AppDbContext : DbContext
{
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<CurrencyNewsletter> CurrencyNewsletters => Set<CurrencyNewsletter>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();
    public DbSet<Setting> Settings => Set<Setting>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("public");
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```

### 3.2 Enums

```csharp
namespace BcvExchangeRates.Domain.Enums;

public enum NewsletterStatus
{
    Draft = 1,
    Authorized = 2,
    Published = 3,
    Historical = 4
}
```

### 3.3 Entidades

**Currency.cs**

```csharp
namespace BcvExchangeRates.Domain.Entities;

public class Currency
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IsoCode { get; set; } = string.Empty;   // USD, EUR, CNY, RUB, TRY
    public string Symbol { get; set; } = string.Empty;
    public bool IsPrincipal { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ExchangeRate> ExchangeRates { get; set; } = [];
}
```

**CurrencyNewsletter.cs**

```csharp
namespace BcvExchangeRates.Domain.Entities;

public class CurrencyNewsletter
{
    public int Id { get; set; }
    public DateTime PublishedAt { get; set; }
    public DateTime ValueDate { get; set; }
    public string? Observations { get; set; }
    public int CreatedBy { get; set; }
    public string? IpAddress { get; set; }
    public bool IsActive { get; set; } = true;
    public NewsletterStatus Status { get; set; } = NewsletterStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ExchangeRate> ExchangeRates { get; set; } = [];
}
```

**ExchangeRate.cs**

```csharp
namespace BcvExchangeRates.Domain.Entities;

public class ExchangeRate
{
    public int Id { get; set; }
    public decimal Value { get; set; }              // DECIMAL(18,8)
    public int CurrencyId { get; set; }
    public int CurrencyNewsletterId { get; set; }
    public int CreatedBy { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Currency Currency { get; set; } = null!;
    public CurrencyNewsletter Newsletter { get; set; } = null!;
}
```

**Setting.cs**

```csharp
namespace BcvExchangeRates.Domain.Entities;

public class Setting
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;    // bcv_auto_consult, bcv_update_cron, etc.
    public string? Value { get; set; }
    public string Type { get; set; } = "string";       // boolean, integer, json, string
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
```

### 3.4 Configuraciones EF Core

**CurrencyConfiguration.cs**

```csharp
namespace BcvExchangeRates.Infrastructure.Data.Configurations;

public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.ToTable("security_currencies");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.IsoCode).HasMaxLength(3).IsRequired();
        builder.HasIndex(c => c.IsoCode).IsUnique();
        builder.Property(c => c.Symbol).HasMaxLength(10);
    }
}
```

**CurrencyNewsletterConfiguration.cs**

```csharp
public class CurrencyNewsletterConfiguration : IEntityTypeConfiguration<CurrencyNewsletter>
{
    public void Configure(EntityTypeBuilder<CurrencyNewsletter> builder)
    {
        builder.ToTable("security_currency_newsletters");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Status)
               .HasConversion<int>();   // enum → int en DB
        builder.HasIndex(n => new { n.PublishedAt, n.IsActive });
    }
}
```

**ExchangeRateConfiguration.cs**

```csharp
public class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.ToTable("security_exchange_rates");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Value).HasPrecision(18, 8);

        builder.HasIndex(r => new { r.CurrencyId, r.CurrencyNewsletterId }).IsUnique();

        builder.HasOne(r => r.Currency)
               .WithMany(c => c.ExchangeRates)
               .HasForeignKey(r => r.CurrencyId);

        builder.HasOne(r => r.Newsletter)
               .WithMany(n => n.ExchangeRates)
               .HasForeignKey(r => r.CurrencyNewsletterId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
```

**SettingConfiguration.cs**

```csharp
public class SettingConfiguration : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.ToTable("security_settings");
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => s.Key).IsUnique();
    }
}
```

### 3.5 Migraciones

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

## 4. Servicio de Scraping BCV

### 4.1 Interfaz

```csharp
namespace BcvExchangeRates.Application.Services;

public interface IBcvScraperService
{
    Task<BcvScrapeResult> FetchCurrentRatesAsync(CancellationToken ct = default);
}
```

### 4.2 DTO Resultado

```csharp
namespace BcvExchangeRates.Application.DTOs;

public record BcvScrapeResult
{
    public bool IsSuccess { get; init; }
    public Dictionary<string, decimal> Rates { get; init; } = [];
    public DateTime? ValueDate { get; init; }
    public string? Error { get; init; }

    public static BcvScrapeResult Success(Dictionary<string, decimal> rates, DateTime? date)
        => new() { IsSuccess = true, Rates = rates, ValueDate = date };

    public static BcvScrapeResult Failure(string error)
        => new() { IsSuccess = false, Error = error };
}
```

### 4.3 Implementación

```csharp
using System.Globalization;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace BcvExchangeRates.Application.Services;

public class BcvScraperService : IBcvScraperService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BcvScraperService> _logger;

    private static readonly Dictionary<string, string> CurrencyMap = new()
    {
        ["dolar"] = "USD",
        ["euro"]  = "EUR",
        ["yuan"]  = "CNY",
        ["rublo"] = "RUB",
        ["lira"]  = "TRY"
    };

    private static readonly Dictionary<string, string> MonthsSpanishToEnglish = new()
    {
        ["Enero"] = "January", ["Febrero"] = "February", ["Marzo"] = "March",
        ["Abril"] = "April",   ["Mayo"] = "May",         ["Junio"] = "June",
        ["Julio"] = "July",    ["Agosto"] = "August",    ["Septiembre"] = "September",
        ["Octubre"] = "October", ["Noviembre"] = "November", ["Diciembre"] = "December"
    };

    public BcvScraperService(IHttpClientFactory httpClientFactory,
                             ILogger<BcvScraperService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<BcvScrapeResult> FetchCurrentRatesAsync(CancellationToken ct = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("bcv");
            var html = await client.GetStringAsync("https://www.bcv.org.ve/", ct);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // ————— ESTRATEGIA 1: clase específica —————
            var section = doc.DocumentNode
                .SelectSingleNode("//*[contains(@class, 'view-tipo-de-cambio-oficial-del-bcv')]");

            // ————— ESTRATEGIA 2: buscar por texto —————
            section ??= doc.DocumentNode
                .SelectSingleNode("//section[contains(@class, 'block-views')]" +
                    "[contains(., 'Tipo de Cambio') or contains(., 'BCV')]");

            // ————— ESTRATEGIA 3: buscar por IDs de moneda —————
            section ??= FindSectionByCurrencyIds(doc);

            if (section == null)
            {
                _logger.LogWarning("No se encontró la sección de tasas BCV");
                return BcvScrapeResult.Failure("Sección de tasas no encontrada en el HTML");
            }

            // ————— Extraer cada tasa —————
            var rates = new Dictionary<string, decimal>();
            foreach (var (id, iso) in CurrencyMap)
            {
                var node = section.SelectSingleNode($".//*[@id='{id}']")
                         ?? FindCurrencyNodeByText(section, iso);

                if (node == null) continue;

                var value = ExtractRateValue(node);
                if (value.HasValue)
                    rates[iso] = value.Value;
            }

            if (rates.Count == 0)
            {
                _logger.LogWarning("No se pudieron extraer tasas del HTML");
                return BcvScrapeResult.Failure("No se pudieron extraer las tasas de cambio");
            }

            // ————— Fecha —————
            var dateText = section.SelectSingleNode(".//*[contains(@class, 'date-display-single')]")
                          ?? section.SelectSingleNode(".//div[contains(., 'Fecha Valor')]");

            var parsedDate = dateText != null
                ? ParseBcvDate(dateText.InnerText.Trim())
                : DateTime.UtcNow;

            return BcvScrapeResult.Success(rates, parsedDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping BCV");
            return BcvScrapeResult.Failure($"Error al conectar con BCV: {ex.Message}");
        }
    }

    private static HtmlNode? FindSectionByCurrencyIds(HtmlDocument doc)
    {
        var sections = doc.DocumentNode.SelectNodes("//section");
        if (sections == null) return null;

        return sections.FirstOrDefault(s =>
            s.InnerHtml.Contains("id=\"dolar\"") &&
            (s.InnerHtml.Contains("id=\"euro\"") || s.InnerHtml.Contains("id=\"yuan\"")));
    }

    private static HtmlNode? FindCurrencyNodeByText(HtmlNode section, string iso)
    {
        // Busca un div.row.recuadrotsmc que contenga el texto del ISO
        var nodes = section.SelectNodes(".//div[contains(@class, 'recuadrotsmc')]");
        return nodes?.FirstOrDefault(n => n.InnerText.Contains(iso));
    }

    private static decimal? ExtractRateValue(HtmlNode container)
    {
        var valueNode = container.SelectSingleNode(".//*[contains(@class, 'centrado')]/strong")
                    ?? container.SelectSingleNode(".//*[contains(@class, 'centrado')]");

        if (valueNode == null) return null;

        var raw = valueNode.InnerText.Trim();

        // 1. Quitar separadores de miles (punto y espacios)
        var cleaned = raw.Replace(".", "").Replace(" ", "");
        // 2. Reemplazar coma decimal por punto
        cleaned = cleaned.Replace(",", ".");

        if (decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
            return value;

        return null;
    }

    private static DateTime ParseBcvDate(string raw)
    {
        // Formato: "lunes, 16 de marzo de 2026"
        // 1. Quitar día de semana y coma
        var cleaned = Regex.Replace(raw, @"^[^,]+,\s*", "");

        // 2. Reemplazar meses español → inglés
        foreach (var (es, en) in MonthsSpanishToEnglish)
            cleaned = cleaned.Replace(es, en);

        // 3. Quitar "de" → "16 March 2026"
        cleaned = cleaned.Replace(" de ", " ");

        return DateTime.TryParse(cleaned, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)
            ? dt
            : DateTime.UtcNow;
    }
}
```

### 4.4 Ciclo de Vida del Boletín

```
Status 1 (Draft / Pendiente)
   │  ← Creado por BcvFetchJob (automático) o POST manual (UI)
   │
   ▼  ← Usuario autoriza vía API (POST /api/exchange-rates/{id}/authorize)
Status 2 (Authorized)
   │
   ▼  ← BcvActivateJob a las 00:00 VET
Status 3 (Published / Vigente)
   │
   ▼  ← Cuando llega un nuevo boletín, el anterior pasa a histórico
Status 4 (Historical)
```

---

## 5. Servicio de Consulta de Tasas (Cacheado)

### 5.1 Interfaz

```csharp
namespace BcvExchangeRates.Application.Services;

public interface IExchangeRateService
{
    Task<NewsletterDto?> GetCurrentNewsletterAsync(CancellationToken ct = default);
    Task<decimal?> GetLatestValueByCodeAsync(string code, CancellationToken ct = default);
    void ClearCache();
}
```

### 5.2 Implementación

```csharp
using Microsoft.Extensions.Caching.Memory;

namespace BcvExchangeRates.Application.Services;

public class ExchangeRateService : IExchangeRateService
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ExchangeRateService> _logger;

    private const string CacheKey = "current_exchange_newsletter";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    public ExchangeRateService(AppDbContext db, IMemoryCache cache,
                               ILogger<ExchangeRateService> logger)
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
```

### 5.3 DTOs

```csharp
namespace BcvExchangeRates.Application.DTOs;

public record NewsletterDto
{
    public int Id { get; init; }
    public DateTime PublishedAt { get; init; }
    public DateTime ValueDate { get; init; }
    public string? Observations { get; init; }
    public NewsletterStatus Status { get; init; }
    public List<ExchangeRateDto> ExchangeRates { get; init; } = [];
}

public record ExchangeRateDto
{
    public int Id { get; init; }
    public decimal Value { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public string CurrencyName { get; init; } = string.Empty;
    public string CurrencySymbol { get; init; } = string.Empty;
}

public static class MappingExtensions
{
    public static NewsletterDto ToDto(this CurrencyNewsletter n) => new()
    {
        Id = n.Id,
        PublishedAt = n.PublishedAt,
        ValueDate = n.ValueDate,
        Observations = n.Observations,
        Status = n.Status,
        ExchangeRates = n.ExchangeRates.Select(r => new ExchangeRateDto
        {
            Id = r.Id,
            Value = r.Value,
            CurrencyCode = r.Currency.IsoCode,
            CurrencyName = r.Currency.Name,
            CurrencySymbol = r.Currency.Symbol
        }).ToList()
    };
}
```

---

## 6. Background Jobs con Quartz.NET (Scheduler)

### 6.1 Instalación

```bash
dotnet add package Quartz.Extensions.Hosting
```

### 6.2 BcvFetchJob → `bcv:fetch-rates`

```csharp
using Quartz;

namespace BcvExchangeRates.Infrastructure.BackgroundJobs;

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

        // 1. Verificar setting
        var autoEnabled = await db.Settings
            .Where(s => s.Key == "bcv_auto_consult")
            .Select(s => s.Value)
            .FirstOrDefaultAsync();

        if (autoEnabled != "true")
        {
            _logger.LogInformation("BCV auto-consulta desactivada, saltando fetch");
            return;
        }

        // 2. Scrapear
        var result = await scraper.FetchCurrentRatesAsync(context.CancellationToken);
        if (!result.IsSuccess)
        {
            _logger.LogError("Fallo BCV fetch: {Error}", result.Error);
            return;
        }

        // 3. Verificar duplicados
        var dateString = result.ValueDate!.Value.Date;
        var exists = await db.CurrencyNewsletters
            .AnyAsync(n => n.ValueDate.Date == dateString
                        && n.Status == NewsletterStatus.Draft
                        && n.IsActive);

        if (exists)
        {
            // Forzar: desactivar drafts anteriores para esta fecha
            await db.CurrencyNewsletters
                .Where(n => n.ValueDate.Date == dateString
                         && n.Status == NewsletterStatus.Draft
                         && n.IsActive)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(n => n.IsActive, false)
                    .SetProperty(n => n.Observations, n => n.Observations + " (reemplazado)"));
        }

        // 4. Crear newsletter (Draft)
        var newsletter = new CurrencyNewsletter
        {
            PublishedAt = DateTime.UtcNow,
            ValueDate = result.ValueDate!.Value,
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

        // 5. Insertar tasas
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

        // 6. SignalR broadcast
        await hubContext.Clients.Group("exchange-updates")
            .SendAsync("RatePublished", new { newsletterId = newsletter.Id },
                context.CancellationToken);

        _logger.LogInformation("Nuevo draft BCV creado: #{NewsletterId}", newsletter.Id);
    }
}
```

### 6.3 BcvActivateJob → `bcv:activate-rates`

```
[DisallowConcurrentExecution]
Ejecución: todos los días a las 00:00 VET

Lógica:
  1. Buscar newsletter Status=Authorized más reciente (order by value_date DESC)
  2. Si no existe → log y return
  3. Transacción:
     a. Pasar todos los Status=Published, IsActive=true → Status=Historical
     b. Pasar el Authorized encontrado → Status=Published
  4. ClearCache() via ExchangeRateService
  5. Obtener USD rate actual
  6. SignalR:
     - "RateInEffect" a grupo "exchange-updates"
     - "LiveUpdate" a grupo "exchange-rates-public"
```

### 6.4 BcvAuditJob → `bcv:audit`

```
[DisallowConcurrentExecution]
Ejecución: configurable vía setting bcv_audit_cron (default 6:00PM VET)

Lógica:
  1. Verificar que exista al menos un newsletter con Status=Published e IsActive=true
  2. Si NO existe → disparar BcvFetchJob manualmente (forzar fetch)
```

### 6.5 Registro Quartz en Program.cs

```csharp
// Opción A: schedule fijo
builder.Services.AddQuartz(q =>
{
    q.AddJob<BcvFetchJob>(j => j.WithIdentity("bcv-fetch"));
    q.AddTrigger(t => t.ForJob("bcv-fetch")
        .WithCronSchedule("10 16 * * 1-5", s =>
            s.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("America/Caracas"))));

    q.AddJob<BcvActivateJob>(j => j.WithIdentity("bcv-activate"));
    q.AddTrigger(t => t.ForJob("bcv-activate")
        .WithCronSchedule("0 0 * * *", s =>
            s.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("America/Caracas"))));

    q.AddJob<BcvAuditJob>(j => j.WithIdentity("bcv-audit"));
    q.AddTrigger(t => t.ForJob("bcv-audit")
        .WithCronSchedule("0 18 * * *", s =>
            s.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("America/Caracas"))));
});
builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});
```

> **Nota:** Si quieres que los schedules se lean dinámicamente desde `security_settings`, puedes implementar un `IJob` que se re-programe solo al arrancar o mediante un listener.

---

## 7. SignalR Hubs — Tiempo Real

### 7.1 ExchangeRateHub

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BcvExchangeRates.Api.Hubs;

[Authorize(Policy = "ExchangeRatesUpdates")]
public class ExchangeRateHub : Hub
{
    public async Task JoinGroup(string groupName)
    {
        // Grupos: "exchange-updates", "exchange-rates-public"
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}
```

### 7.2 Eventos que se broadcastean

| Evento           | Grupo                   | Cuándo                              |
| ---------------- | ----------------------- | ----------------------------------- |
| `RatePublished`  | `exchange-updates`      | BcvFetchJob crea un Draft           |
| `RateAuthorized` | `exchange-updates`      | Usuario autoriza Draft vía API      |
| `RateInEffect`   | `exchange-updates`      | BcvActivateJob publica a medianoche |
| `LiveUpdate`     | `exchange-rates-public` | Broadcast público del rate USD      |

### 7.3 Mapeo de Endpoint

En `Program.cs`:

```csharp
app.MapHub<ExchangeRateHub>("/hubs/exchange-rates");
```

### 7.4 Autorización en canal público

Para el grupo `exchange-rates-public`, puedes permitir acceso sin JWT usando un `AllowAnonymous` policy o creando un segundo Hub sin `[Authorize]`.

---

## 8. Controladores REST API

### 8.1 ExchangeRatesController

| Método | Ruta                                        | Acción                           | Permiso          |
| ------ | ------------------------------------------- | -------------------------------- | ---------------- |
| `GET`  | `/api/exchange-rates`                       | Listar boletines (DataTable)     | `read`           |
| `GET`  | `/api/exchange-rates/{id}`                  | Obtener boletín por ID           | `read`           |
| `POST` | `/api/exchange-rates`                       | Crear boletín manual             | `write`          |
| `PUT`  | `/api/exchange-rates/{id}`                  | Editar boletín                   | `write`          |
| `POST` | `/api/exchange-rates/{id}/authorize`        | Autorizar draft → status 2       | `authorize`      |
| `POST` | `/api/exchange-rates/{id}/disable`          | Desactivar boletín               | `disable`        |
| `POST` | `/api/exchange-rates/{id}/enable`           | Reactivar boletín                | `enable`         |
| `GET`  | `/api/exchange-rates/live/{currency}`       | Consultar tasa vigente (público) | `anonymous`      |
| `POST` | `/api/exchange-rates/autoupdate-bcv/toggle` | Toggle automation                | `autoupdate-bcv` |

```csharp
namespace BcvExchangeRates.Api.Controllers;

[ApiController]
[Route("api/exchange-rates")]
[Authorize]
public class ExchangeRatesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IExchangeRateService _rateService;
    private readonly IHubContext<ExchangeRateHub> _hubContext;

    // GET /api/exchange-rates
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int perPage = 50)
    {
        var query = _db.CurrencyNewsletters
            .AsNoTracking()
            .Include(n => n.ExchangeRates).ThenInclude(r => r.Currency)
            .OrderByDescending(n => n.CreatedAt);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * perPage).Take(perPage)
            .Select(n => n.ToDto())
            .ToListAsync();

        return Ok(new { data = items, total, page, perPage });
    }

    // GET /api/exchange-rates/live/{currency}
    [AllowAnonymous]
    [HttpGet("live/{currency}")]
    public async Task<IActionResult> GetLiveRate(string currency)
    {
        var value = await _rateService.GetLatestValueByCodeAsync(currency);
        if (value == null) return NotFound();
        return Ok(new { currency = currency.ToUpper(), value });
    }

    // POST /api/exchange-rates/{id}/authorize
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

    // POST /api/exchange-rates/autoupdate-bcv/toggle
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
```

### 8.2 AuthController

```csharp
namespace BcvExchangeRates.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { error = "Credenciales inválidas" });

        var token = GenerateJwtToken(user);
        return Ok(new { token, user.Name, user.Email });
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new("permission", "autoupdate-bcv"),
            new("permission", "read"),
            new("permission", "write"),
            new("permission", "authorize")
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### 8.3 SettingsController

| Método | Ruta                | Acción                             |
| ------ | ------------------- | ---------------------------------- |
| `GET`  | `/api/settings/bcv` | Obtener settings BCV               |
| `PUT`  | `/api/settings/bcv` | Actualizar settings (cron, toggle) |

---

## 9. JWT Authentication + SignalR

### 9.1 Configuración en Program.cs

```csharp
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };

        // 🔥 IMPORTANTE para SignalR: leer token del query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken)
                    && path.StartsWithSegments("/hubs/exchange-rates"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ExchangeRatesUpdates", policy =>
        policy.RequireClaim("permission", "autoupdate-bcv"));
});
```

### 9.2 appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=bcv_exchange;Username=postgres;Password=..."
  },
  "Jwt": {
    "Key": "tu-clave-super-segura-de-32-caracteres-minimo!",
    "Issuer": "BcvExchangeRates",
    "Audience": "BcvExchangeRatesApi"
  }
}
```

### 9.3 Conexión desde Frontend (JS)

```javascript
// Conexión SignalR con JWT
const connection = new signalR.HubConnectionBuilder()
  .withUrl("/hubs/exchange-rates", {
    accessTokenFactory: () => localStorage.getItem("jwt_token"),
  })
  .build();

await connection.start();
await connection.invoke("JoinGroup", "exchange-updates");

connection.on("RatePublished", (data) => {
  // Recargar tabla o mostrar notificación
  console.log("Nuevas tasas pendientes", data);
});

connection.on("LiveUpdate", (data) => {
  // Actualizar widget de tasa en vivo
  console.log("Tasa USD actualizada", data);
});
```

---

## 10. FluentValidation

### 10.1 Validación para crear boletín manual

```csharp
using FluentValidation;

namespace BcvExchangeRates.Application.Validators;

public class CreateNewsletterValidator : AbstractValidator<CreateNewsletterRequest>
{
    public CreateNewsletterValidator()
    {
        RuleFor(x => x.PublishedAt)
            .NotEmpty().WithMessage("Fecha de publicación es requerida");

        RuleFor(x => x.ValueDate)
            .NotEmpty().WithMessage("Fecha valor es requerida");

        RuleFor(x => x.Rates)
            .NotEmpty().WithMessage("Debe incluir al menos una tasa");

        RuleForEach(x => x.Rates).ChildRules(rate =>
        {
            rate.RuleFor(r => r.CurrencyId)
                .GreaterThan(0).WithMessage("Moneda inválida");

            rate.RuleFor(r => r.Value)
                .GreaterThan(0).WithMessage("La tasa debe ser mayor a 0")
                .PrecisionScale(18, 8, false).WithMessage("Formato de tasa inválido");
        });

        RuleFor(x => x.Rates.Select(r => r.CurrencyId))
            .Must(x => x.Distinct().Count() == x.Count())
            .WithMessage("No puede haber monedas duplicadas en el mismo boletín");
    }
}
```

### 10.2 DTO Request

```csharp
namespace BcvExchangeRates.Application.DTOs;

public record CreateNewsletterRequest
{
    public DateTime PublishedAt { get; init; }
    public DateTime ValueDate { get; init; }
    public string? Observations { get; init; }
    public List<RateEntry> Rates { get; init; } = [];

    public record RateEntry
    {
        public int CurrencyId { get; init; }
        public decimal Value { get; init; }
    }
}
```

### 10.3 Registro

```csharp
builder.Services.AddValidatorsFromAssemblyContaining<CreateNewsletterValidator>();
builder.Services.AddFluentValidationAutoValidation();
```

---

## 11. Seeders — Datos Iniciales

```csharp
namespace BcvExchangeRates.Infrastructure.Data.Seeders;

public static class AppDbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        // ————— Monedas —————
        if (!await db.Currencies.AnyAsync())
        {
            db.Currencies.AddRange(
                new Currency { IsoCode = "USD", Name = "Dólar estadounidense", Symbol = "$",   IsPrincipal = true,  IsActive = true },
                new Currency { IsoCode = "EUR", Name = "Euro",                    Symbol = "€", IsPrincipal = false, IsActive = true },
                new Currency { IsoCode = "CNY", Name = "Yuan chino",              Symbol = "¥", IsPrincipal = false, IsActive = true },
                new Currency { IsoCode = "RUB", Name = "Rublo ruso",              Symbol = "₽", IsPrincipal = false, IsActive = true },
                new Currency { IsoCode = "TRY", Name = "Lira turca",              Symbol = "₺", IsPrincipal = false, IsActive = true }
            );
        }

        // ————— Settings —————
        if (!await db.Settings.AnyAsync())
        {
            db.Settings.AddRange(
                new Setting { Key = "bcv_auto_consult", Value = "false", Type = "boolean", Description = "Activa la consulta automática al BCV" },
                new Setting { Key = "bcv_update_cron",  Value = "10 16 * * 1-5", Type = "string", Description = "Lun-Vie 4:10 PM VET (hora de actualización del BCV)" },
                new Setting { Key = "bcv_audit_enabled", Value = "true", Type = "boolean", Description = "Activa la auditoría diaria de tasas" },
                new Setting { Key = "bcv_audit_cron",   Value = "0 18 * * *", Type = "string", Description = "6:00 PM VET — verifica que existan tasas vigentes" }
            );
        }

        await db.SaveChangesAsync();
    }
}
```

---

## 12. DI y Program.cs

### 12.1 Infrastructure/DependencyInjection.cs

```csharp
namespace BcvExchangeRates.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration config)
    {
        // ——— EF Core + PostgreSQL ———
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        // ——— Servicios de aplicación ———
        services.AddScoped<IBcvScraperService, BcvScraperService>();
        services.AddScoped<IExchangeRateService, ExchangeRateService>();

        // ——— HTTP Client para BCV ———
        services.AddHttpClient("bcv", client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("es-ES,es;q=0.9");
            client.Timeout = TimeSpan.FromSeconds(30);
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        });

        // ——— Caching en memoria ———
        services.AddMemoryCache();

        // ——— SignalR ———
        services.AddSignalR();

        return services;
    }
}
```

### 12.2 Program.cs completo

```csharp
using System.Text;
using BcvExchangeRates.Application.Services;
using BcvExchangeRates.Api.Hubs;
using BcvExchangeRates.Api.Middleware;
using BcvExchangeRates.Infrastructure;
using BcvExchangeRates.Infrastructure.Data;
using BcvExchangeRates.Infrastructure.Data.Seeders;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// ─── Servicios ───

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Infraestructura (DB, HTTP, Caching, SignalR)
builder.Services.AddInfrastructure(builder.Configuration);

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token) &&
                    context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ExchangeRatesUpdates", policy =>
        policy.RequireClaim("permission", "autoupdate-bcv"));
});

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateNewsletterValidator>();
builder.Services.AddFluentValidationAutoValidation();

// Quartz Scheduler
builder.Services.AddQuartz(q =>
{
    // BcvFetchJob: Lun-Vie 4:10 PM VET
    q.AddJob<BcvFetchJob>(j => j.WithIdentity("bcv-fetch"));
    q.AddTrigger(t => t.ForJob("bcv-fetch")
        .WithCronSchedule("10 16 * * 1-5", s =>
            s.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("America/Caracas"))));

    // BcvActivateJob: todos los días 00:00 VET
    q.AddJob<BcvActivateJob>(j => j.WithIdentity("bcv-activate"));
    q.AddTrigger(t => t.ForJob("bcv-activate")
        .WithCronSchedule("0 0 * * *", s =>
            s.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("America/Caracas"))));

    // BcvAuditJob: 6:00 PM VET
    q.AddJob<BcvAuditJob>(j => j.WithIdentity("bcv-audit"));
    q.AddTrigger(t => t.ForJob("bcv-audit")
        .WithCronSchedule("0 18 * * *", s =>
            s.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("America/Caracas"))));
});
builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});

var app = builder.Build();

// ─── Middleware ───

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ExchangeRateHub>("/hubs/exchange-rates");

// ─── Seed data al arrancar ───
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await AppDbSeeder.SeedAsync(db);
}

app.Run();
```

---

## 13. Checklist de Paquetes NuGet

```xml
<ItemGroup>
    <!-- ORM PostgreSQL -->
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.*" />

    <!-- Design-time EF Core tools (migrations) -->
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.*" />

    <!-- JWT Authentication -->
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.*" />

    <!-- Password hashing -->
    <PackageReference Include="BCrypt.Net-Next" Version="4.*" />

    <!-- Validation -->
    <PackageReference Include="FluentValidation.AspNetCore" Version="11.*" />

    <!-- HTML Scraping -->
    <PackageReference Include="HtmlAgilityPack" Version="1.*" />

    <!-- Scheduler -->
    <PackageReference Include="Quartz.Extensions.Hosting" Version="3.*" />

    <!-- SignalR (built-in, no extra package needed) -->

    <!-- Opcional: Redis cache distribuido -->
    <PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="10.*" />
</ItemGroup>
```

```bash
# Instalación rápida (copia y pega)
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL && \
dotnet add package Microsoft.EntityFrameworkCore.Design && \
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer && \
dotnet add package BCrypt.Net-Next && \
dotnet add package FluentValidation.AspNetCore && \
dotnet add package HtmlAgilityPack && \
dotnet add package Quartz.Extensions.Hosting
```

---

## 14. Diagrama de Flujo Final

```
                         ┌──────────────────────┐
                         │     PostgreSQL DB     │
                         │  (4 tablas + usuarios)│
                         └──────┬───────┬───────┘
                                │       │
          ┌─────────────────────┘       └─────────────────────┐
          ▼                                                   ▼
  ┌───────────────┐                                   ┌───────────────┐
  │  Quartz Jobs  │                                   │  REST API     │
  │               │                                   │  (JWT Auth)   │
  │  BcvFetchJob  │── 4:10PM VET ──┐                 └───────┬───────┘
  │  (scraping)   │                 │                         │
  │               │                 ▼                         ▼
  │  BcvAuditJob  │── 6:00PM VET ──►  ExchangeRateService    Auth
  │  (verifica)   │                 │  (IMemoryCache, 1h TTL) Controller
  │               │                 │                         │
  │  BcvActivate  │── 00:00 VET ───┘    ┌─────────────────────┘
  │  (publica)    │                      │
  └───────┬───────┘                      │
          │                              │
          ▼                              ▼
  ┌───────────────────────────────────────────┐
  │           SignalR Hub                      │
  │                                           │
  │  "exchange-updates" (privado, JWT)        │
  │    └ RatePublished                        │
  │    └ RateAuthorized                       │
  │    └ RateInEffect                         │
  │                                           │
  │  "exchange-rates-public" (público/API Key)│
  │    └ LiveUpdate (USD broadcast)           │
  └───────────────────────────────────────────┘
          │
          ▼
  ┌───────────────────┐
  │  Frontend (SPA)   │
  │  SignalR client   │
  │  + DataTable      │
  └───────────────────┘
```

---

**Flujo de autorización de un boletín (ejemplo completo):**

```
1. [BCV] Publica tasa en bcv.org.ve
2. [BcvFetchJob] 4:10PM → scrapea → crea Draft (Status 1) en DB
3. [SignalR] → "RatePublished" → notifica admins
4. [Admin] Abre panel, ve el Draft, hace clic en "Autorizar"
   → POST /api/exchange-rates/{id}/authorize (JWT)
5. [API] Cambia Status 1 → 2, limpia cache
6. [SignalR] → "RateAuthorized" → actualiza UI
7. [BcvActivateJob] 00:00 VET → Status 2 → 3
   → Anterior Status 3 → 4 (Histórico)
   → Limpia cache
8. [SignalR] → "RateInEffect" + "LiveUpdate($$$)"
9. [ExchangeRateService] Desde ahora, GetLatestValueByCode("USD")
   devuelve la nueva tasa (cacheada por 1h)
10. [Checkout/Ventas] Usan la tasa vigente automáticamente
```

---

> **Hecho con ❤️ para ser replicado.** Si necesitas ayuda con la implementación de algún componente específico (scraping, jobs, SignalR, etc.), solo dilo y te paso el código completo.
