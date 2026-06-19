namespace AutoNex.Models;

public class Currency
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IsoCode { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public bool IsPrincipal { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ExchangeRate> ExchangeRates { get; set; } = [];
}
