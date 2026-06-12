using AutoNex.Enums;

namespace AutoNex.DTOs.Notifications;

public record SendNotificationRequest(
    int ClientId,
    int? VehicleId,
    NotificationType Type,
    string Recipient,
    string Message
);
