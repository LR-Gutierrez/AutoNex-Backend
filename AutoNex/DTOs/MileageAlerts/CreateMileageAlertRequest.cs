namespace AutoNex.DTOs.MileageAlerts;

public record CreateMileageAlertRequest
{
    public int VehicleId { get; set; }
    public int EstimatedWeeklyKm { get; set; }
}
