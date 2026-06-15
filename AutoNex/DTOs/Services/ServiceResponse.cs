namespace AutoNex.DTOs.Services;

public record ServiceResponse(
    int Id,
    string Name,
    string? Description,
    decimal DefaultPrice,
    int? MinKmInterval,
    int? MaxKmInterval,
    int? RecommendedMonths,
    DateTime CreatedAt
);
