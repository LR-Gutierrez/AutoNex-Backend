namespace AutoNex.DTOs.Services;

public record ServiceResponse(
    int Id,
    string Name,
    string? Description,
    decimal DefaultPrice,
    int? RecommendedKmInterval,
    DateTime CreatedAt
);
