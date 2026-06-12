namespace AutoNex.DTOs.MileageAlerts;

public class CreateMileageAlertRequest
{
    public int VehicleId { get; set; }
    public int EstimatedWeeklyKm { get; set; }
}
