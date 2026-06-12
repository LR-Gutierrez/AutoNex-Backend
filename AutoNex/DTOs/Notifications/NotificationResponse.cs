using AutoNex.Enums;

namespace AutoNex.DTOs.Notifications;

public record NotificationResponse(
    int Id,
    int ClientId,
    string ClientName,
    int? VehicleId,
    string? VehicleInfo,
    NotificationType Type,
    string Recipient,
    string Message,
    DateTime? SentAt,
    NotificationStatus Status,
    DateTime CreatedAt
);
