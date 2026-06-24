namespace AutoNex.DTOs.Notifications;

public class AlertPreviewDto
{
    public int AlertId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
