namespace AutoNex.Models;

public class MileageAlert
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public int ServiceId { get; set; }
    public int EstimatedWeeklyKm { get; set; }
    public int NextAlertKm { get; set; }
    public DateTime? LastAlertDate { get; set; }
    public DateTime? NextAlertDate { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Vehicle Vehicle { get; set; } = null!;
    public Service Service { get; set; } = null!;
}
