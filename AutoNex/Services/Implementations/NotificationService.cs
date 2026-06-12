using AutoNex.Data;
using AutoNex.DTOs;
using AutoNex.DTOs.Notifications;
using AutoNex.Enums;
using AutoNex.Helpers;
using AutoNex.Models;
using AutoNex.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoNex.Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;
    private readonly ITwilioService _twilioService;

    public NotificationService(AppDbContext context, ITwilioService twilioService)
    {
        _context = context;
        _twilioService = twilioService;
    }

    public async Task<PagedResponse<NotificationResponse>> GetAllAsync(int? clientId, int? vehicleId, string? status, int? page, int? pageSize)
    {
        var query = _context.Notifications
            .Include(n => n.Client)
            .Include(n => n.Vehicle)
            .AsQueryable();

        if (clientId.HasValue)
            query = query.Where(n => n.ClientId == clientId.Value);
        if (vehicleId.HasValue)
            query = query.Where(n => n.VehicleId == vehicleId.Value);
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<NotificationStatus>(status, true, out var parsedStatus))
            query = query.Where(n => n.Status == parsedStatus);

        query = query.OrderByDescending(n => n.CreatedAt);

        var paged = await query.ToPagedAsync(page, pageSize);

        return new PagedResponse<NotificationResponse>
        {
            Items = paged.Items.Select(n => n.ToResponse()).ToList(),
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount
        };
    }

    public async Task<NotificationResponse?> GetByIdAsync(int id)
    {
        var notification = await _context.Notifications
            .Include(n => n.Client)
            .Include(n => n.Vehicle)
            .FirstOrDefaultAsync(n => n.Id == id);

        return notification?.ToResponse();
    }

    public async Task<NotificationResponse> SendAsync(SendNotificationRequest request)
    {
        var client = await _context.Clients.FindAsync(request.ClientId)
            ?? throw new KeyNotFoundException("Cliente no encontrado");

        var notification = new Notification
        {
            ClientId = request.ClientId,
            VehicleId = request.VehicleId,
            Type = request.Type,
            Recipient = request.Recipient,
            Message = request.Message,
            Status = NotificationStatus.Pending
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        if (request.Type == NotificationType.WhatsApp && _twilioService.IsConfigured)
        {
            var sent = await _twilioService.SendWhatsAppAsync(request.Recipient, request.Message);
            notification.Status = sent ? NotificationStatus.Sent : NotificationStatus.Failed;
            notification.SentAt = sent ? DateTime.UtcNow : null;
        }
        else
        {
            notification.Status = NotificationStatus.Sent;
            notification.SentAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(notification.Id))!;
    }

    public async Task<NotificationResponse?> SendReminderAsync(int alertId)
    {
        var alert = await _context.MileageAlerts
            .Include(a => a.Vehicle)
                .ThenInclude(v => v.Client)
            .FirstOrDefaultAsync(a => a.Id == alertId);

        if (alert is null) return null;

        var client = alert.Vehicle.Client;
        var phone = client.Phone;
        if (string.IsNullOrWhiteSpace(phone))
            throw new InvalidOperationException("El cliente no tiene teléfono registrado");

        var vehicleInfo = $"{alert.Vehicle.Brand} {alert.Vehicle.Model} ({alert.Vehicle.LicensePlate})";
        var message = $"Recordatorio: El vehículo {vehicleInfo} requiere atención. " +
                      $"Kilometraje actual: {alert.LastRecordedKm} km. " +
                      $"Próxima alerta: {alert.NextAlertKm} km.";

        var request = new SendNotificationRequest(
            client.Id,
            alert.VehicleId,
            NotificationType.WhatsApp,
            phone,
            message
        );

        return await SendAsync(request);
    }
}
