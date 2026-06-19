using System.Globalization;
using System.Text.RegularExpressions;
using AutoNex.DTOs.ExchangeRates;
using AutoNex.Services.Interfaces;
using HtmlAgilityPack;

namespace AutoNex.Services.Implementations;

public class BcvScraperService : IBcvScraperService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BcvScraperService> _logger;

    private static readonly Dictionary<string, string> CurrencyMap = new()
    {
        ["dolar"] = "USD",
        ["euro"] = "EUR",
        ["yuan"] = "CNY",
        ["rublo"] = "RUB",
        ["lira"] = "TRY"
    };

    private static readonly Dictionary<string, string> MonthsSpanishToEnglish = new()
    {
        ["Enero"] = "January", ["Febrero"] = "February", ["Marzo"] = "March",
        ["Abril"] = "April", ["Mayo"] = "May", ["Junio"] = "June",
        ["Julio"] = "July", ["Agosto"] = "August", ["Septiembre"] = "September",
        ["Octubre"] = "October", ["Noviembre"] = "November", ["Diciembre"] = "December"
    };

    public BcvScraperService(IHttpClientFactory httpClientFactory, ILogger<BcvScraperService> logger)
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

            var section = doc.DocumentNode
                .SelectSingleNode("//*[contains(@class, 'view-tipo-de-cambio-oficial-del-bcv')]");

            section ??= doc.DocumentNode
                .SelectSingleNode("//section[contains(@class, 'block-views')]" +
                    "[contains(., 'Tipo de Cambio') or contains(., 'BCV')]");

            section ??= FindSectionByCurrencyIds(doc);

            if (section == null)
            {
                _logger.LogWarning("No se encontró la sección de tasas BCV");
                return BcvScrapeResult.Failure("Sección de tasas no encontrada en el HTML");
            }

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
        var nodes = section.SelectNodes(".//div[contains(@class, 'recuadrotsmc')]");
        return nodes?.FirstOrDefault(n => n.InnerText.Contains(iso));
    }

    private static decimal? ExtractRateValue(HtmlNode container)
    {
        var valueNode = container.SelectSingleNode(".//*[contains(@class, 'centrado')]/strong")
                    ?? container.SelectSingleNode(".//*[contains(@class, 'centrado')]");

        if (valueNode == null) return null;

        var raw = valueNode.InnerText.Trim();

        var cleaned = raw.Replace(".", "").Replace(" ", "");
        cleaned = cleaned.Replace(",", ".");

        if (decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
            return value;

        return null;
    }

    private static DateTime ParseBcvDate(string raw)
    {
        var cleaned = Regex.Replace(raw, @"^[^,]+,\s*", "");

        foreach (var (es, en) in MonthsSpanishToEnglish)
            cleaned = cleaned.Replace(es, en);

        cleaned = cleaned.Replace(" de ", " ");

        return DateTime.TryParse(cleaned, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)
            ? dt
            : DateTime.UtcNow;
    }
}
