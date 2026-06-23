namespace AutoNex.Models;

public class WhatsAppMessageLog
{
    public int Id { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "Test"; // "Test" | "Reminder"
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string SentBy { get; set; } = "System";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
