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
    int? WarningKm,
    bool IsDue,
    DateTime? LastAlertDate,
    DateTime? NextAlertDate,
    int? ServiceMinKmInterval,
    int? ServiceMaxKmInterval,
    int? ServiceMinMonth,
    int? ServiceMaxMonth,
    bool IsActive,
    DateTime CreatedAt
);
