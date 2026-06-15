namespace AutoNex.DTOs.MileageAlerts;

public record MileageAlertResponse(
    int Id,
    int VehicleId,
    string VehicleInfo,
    int CurrentKm,
    int EstimatedWeeklyKm,
    int NextAlertKm,
    int? RemainingKm,
    bool IsDue,
    DateTime? LastAlertDate,
    DateTime? NextAlertDate,
    bool IsActive,
    DateTime CreatedAt
);
