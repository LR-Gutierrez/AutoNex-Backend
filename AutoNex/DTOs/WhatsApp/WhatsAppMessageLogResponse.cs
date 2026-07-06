namespace AutoNex.DTOs.WhatsApp;

public record WhatsAppMessageLogResponse(
    int Id,
    string Phone,
    string Message,
    string Type,
    string Status,
    string? ErrorMessage,
    string SentBy,
    DateTime CreatedAt
);
