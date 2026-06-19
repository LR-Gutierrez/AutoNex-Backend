namespace AutoNex.DTOs.ExchangeRates;

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
