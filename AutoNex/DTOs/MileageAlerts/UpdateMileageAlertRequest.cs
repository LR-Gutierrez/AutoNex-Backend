namespace AutoNex.DTOs.MileageAlerts;

public record UpdateMileageAlertRequest
{
    public int EstimatedWeeklyKm { get; set; }
}
