using AutoNex.Enums;

namespace AutoNex.Models;

public class Notification
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int? VehicleId { get; set; }
    public NotificationType Type { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime? SentAt { get; set; }
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Client Client { get; set; } = null!;
    public Vehicle? Vehicle { get; set; }
}
