namespace AutoNex.DTOs.MileageAlerts;

public record MileageAlertResponse(
    int Id,
    int VehicleId,
    string VehicleInfo,
    int LastRecordedKm,
    int EstimatedWeeklyKm,
    int NextAlertKm,
    int? RemainingKm,
    bool IsDue,
    DateTime? LastAlertDate,
    bool IsActive,
    DateTime CreatedAt
);
