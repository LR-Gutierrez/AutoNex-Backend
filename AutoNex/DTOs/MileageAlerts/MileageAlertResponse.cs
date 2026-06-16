namespace AutoNex.DTOs.MileageAlerts;

public record MileageAlertResponse(
    int Id,
    int VehicleId,
    string VehicleInfo,
    int ServiceId,
    string ServiceName,
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
