using AutoNex.DTOs;
using AutoNex.DTOs.Notifications;

namespace AutoNex.Services.Interfaces;

public interface INotificationService
{
    Task<PagedResponse<NotificationResponse>> GetAllAsync(int? clientId, int? vehicleId, string? status, int? page, int? pageSize, CancellationToken cancellationToken = default);
    Task<NotificationResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<NotificationResponse> SendAsync(SendNotificationRequest request, CancellationToken cancellationToken = default);
    Task<NotificationResponse?> SendReminderAsync(int alertId, CancellationToken cancellationToken = default);
}
