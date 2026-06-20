namespace AutoNex.DTOs.ExchangeRates;

public record BcvFetchLogResponse(
    int Id,
    DateTime ValueDate,
    string? RatesJson,
    bool IsSuccess,
    string? Error,
    string Action,
    string FetchedBy,
    DateTime FetchedAt
);
