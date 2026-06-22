namespace AutoNex.DTOs.Vehicles;

public record ServiceOrderBriefResponse(
    int Id,
    DateTime Date,
    string Status,
    decimal TotalAmount,
    int CurrentKm,
    bool ApplyLaborPercentage,
    decimal? LaborPercentage,
    decimal? LaborCharge
);
