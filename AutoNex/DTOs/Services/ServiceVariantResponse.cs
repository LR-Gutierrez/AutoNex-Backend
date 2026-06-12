namespace AutoNex.DTOs.Services;

public record ServiceVariantResponse(
    int Id,
    int ServiceId,
    string ServiceName,
    string Name,
    string? Description,
    int MinKmInterval,
    int MaxKmInterval,
    int? RecommendedMonths,
    bool IsActive,
    DateTime CreatedAt
);
