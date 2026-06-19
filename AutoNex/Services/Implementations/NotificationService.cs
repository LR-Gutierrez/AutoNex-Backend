using AutoNex.Data;
using AutoNex.DTOs;
using AutoNex.DTOs.Notifications;
using AutoNex.Enums;
using AutoNex.Helpers;
using AutoNex.Hubs;
using AutoNex.Models;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AutoNex.Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;
    private readonly ITwilioService _twilioService;
    private readonly IHubContext<NotificationsHub> _hubContext;

    public NotificationService(
        AppDbContext context,
        ITwilioService twilioService,
        IHubContext<NotificationsHub> hubContext)
    {
        _context = context;
        _twilioService = twilioService;
        _hubContext = hubContext;
    }

    public async Task<PagedResponse<NotificationResponse>> GetAllAsync(int? clientId, int? vehicleId, string? status, int? page, int? pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications
            .AsNoTracking()
            .Include(n => n.Client)
            .Include(n => n.Vehicle)
            .Where(n => !n.Client.IsDeleted)
            .AsQueryable();

        if (clientId.HasValue)
            query = query.Where(n => n.ClientId == clientId.Value);
        if (vehicleId.HasValue)
            query = query.Where(n => n.VehicleId == vehicleId.Value);
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<NotificationStatus>(status, true, out var parsedStatus))
            query = query.Where(n => n.Status == parsedStatus);

        query = query.OrderByDescending(n => n.CreatedAt);

        return await query.ToPagedResponseAsync(page, pageSize, n => n.ToResponse(), cancellationToken).ConfigureAwait(false);
    }

    public async Task<NotificationResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var notification = await _context.Notifications
            .AsNoTracking()
            .Include(n => n.Client)
            .Include(n => n.Vehicle)
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

        return notification?.ToResponse();
    }

    public async Task<NotificationResponse> SendAsync(SendNotificationRequest request, CancellationToken cancellationToken = default)
    {
        var client = await _context.Clients.FindAsync(new object[] { request.ClientId }, cancellationToken).ConfigureAwait(false)
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
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if (request.Type == NotificationType.WhatsApp && _twilioService.IsConfigured)
        {
            var sent = await _twilioService.SendWhatsAppAsync(request.Recipient, request.Message, cancellationToken).ConfigureAwait(false);
            notification.Status = sent ? NotificationStatus.Sent : NotificationStatus.Failed;
            notification.SentAt = sent ? DateTime.UtcNow : null;
        }
        else
        {
            notification.Status = NotificationStatus.Sent;
            notification.SentAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var response = (await GetByIdAsync(notification.Id, cancellationToken).ConfigureAwait(false))!;

        await _hubContext.Clients.Group("all").SendAsync("newNotification", response, cancellationToken).ConfigureAwait(false);

        return response;
    }

    public async Task<NotificationResponse?> SendReminderAsync(int alertId, CancellationToken cancellationToken = default)
    {
        var alert = await _context.MileageAlerts
            .AsNoTracking()
            .Include(a => a.Vehicle)
                .ThenInclude(v => v.Client)
            .FirstOrDefaultAsync(a => a.Id == alertId, cancellationToken);

        if (alert is null) return null;

        var client = alert.Vehicle.Client;
        var phone = client.Phone;
        if (string.IsNullOrWhiteSpace(phone))
            throw new InvalidOperationException("El cliente no tiene teléfono registrado");

        var currentKm = await _context.ServiceOrders
            .AsNoTracking()
            .Where(o => o.VehicleId == alert.VehicleId && o.Status == Enums.ServiceOrderStatus.Completed)
            .OrderByDescending(o => o.Date)
            .Select(o => (int?)o.CurrentKm)
            .FirstOrDefaultAsync(cancellationToken) ?? 0;

        var vehicleInfo = $"{alert.Vehicle.Brand} {alert.Vehicle.Model} ({alert.Vehicle.LicensePlate})";
        var message = $"Recordatorio: El vehículo {vehicleInfo} requiere atención. " +
                      $"Kilometraje actual: {currentKm} km. " +
                      $"Próxima alerta: {alert.NextAlertKm} km.";

        var request = new SendNotificationRequest(
            client.Id,
            alert.VehicleId,
            NotificationType.WhatsApp,
            phone,
            message
        );

        return await SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
