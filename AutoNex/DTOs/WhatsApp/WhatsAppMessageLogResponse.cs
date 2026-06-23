namespace AutoNex.DTOs.WhatsApp;

public record WhatsAppMessageLogResponse(
    int Id,
    string Phone,
    string Message,
    string Type,
    bool Success,
    string? ErrorMessage,
    string SentBy,
    DateTime CreatedAt
);
