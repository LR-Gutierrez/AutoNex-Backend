using AutoNex.DTOs;
using AutoNex.DTOs.Notifications;

namespace AutoNex.Services.Interfaces;

public interface INotificationService
{
    Task<PagedResponse<NotificationResponse>> GetAllAsync(int? clientId, int? vehicleId, string? status, int? page, int? pageSize);
    Task<NotificationResponse?> GetByIdAsync(int id);
    Task<NotificationResponse> SendAsync(SendNotificationRequest request);
    Task<NotificationResponse?> SendReminderAsync(int alertId);
}
