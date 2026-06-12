using AutoNex.DTOs.Notifications;

namespace AutoNex.Services.Interfaces;

public interface INotificationService
{
    Task<List<NotificationResponse>> GetAllAsync(int? clientId, int? vehicleId, string? status);
    Task<NotificationResponse?> GetByIdAsync(int id);
    Task<NotificationResponse> SendAsync(SendNotificationRequest request);
    Task<NotificationResponse?> SendReminderAsync(int alertId);
}
