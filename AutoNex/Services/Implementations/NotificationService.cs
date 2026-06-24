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
using Microsoft.Extensions.Logging;

namespace AutoNex.Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;
    private readonly IWaNotifierService _waNotifierService;
    private readonly IHubContext<NotificationsHub> _hubContext;
    private readonly IMessageTemplateService _templateService;
    private readonly IWorkshopInfoService _workshopInfoService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        AppDbContext context,
        IWaNotifierService waNotifierService,
        IHubContext<NotificationsHub> hubContext,
        IMessageTemplateService templateService,
        IWorkshopInfoService workshopInfoService,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _waNotifierService = waNotifierService;
        _hubContext = hubContext;
        _templateService = templateService;
        _workshopInfoService = workshopInfoService;
        _logger = logger;
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

        NotificationStatus status;
        DateTime? sentAt;

        if (request.Type == NotificationType.WhatsApp)
        {
            var sent = await _waNotifierService.SendWhatsAppAsync(request.Recipient, request.Message, "Reminder", sentBy: null, cancellationToken).ConfigureAwait(false);
            status = sent ? NotificationStatus.Sent : NotificationStatus.Failed;
            sentAt = sent ? DateTime.UtcNow : null;
        }
        else
        {
            status = NotificationStatus.Sent;
            sentAt = DateTime.UtcNow;
        }

        var notification = new Notification
        {
            ClientId = request.ClientId,
            VehicleId = request.VehicleId,
            Type = request.Type,
            Recipient = request.Recipient,
            Message = request.Message,
            Status = status,
            SentAt = sentAt
        };

        _context.Notifications.Add(notification);
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
            .Include(a => a.Service)
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
        var message = await BuildMessageAsync(alert, currentKm, vehicleInfo, cancellationToken);

        var request = new SendNotificationRequest(
            client.Id,
            alert.VehicleId,
            NotificationType.WhatsApp,
            phone,
            message
        );

        return await SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<AlertPreviewDto>> BuildPreviewsForOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await _context.ServiceOrders
            .Include(o => o.Items)
                .ThenInclude(i => i.Service)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null) return [];

        var serviceItems = order.Items
            .Where(i => i.Service is not null && (i.Service.MaxKmInterval.HasValue || i.Service.MaxMonth.HasValue))
            .ToList();

        if (serviceItems.Count == 0) return [];

        var serviceIds = serviceItems.Select(i => i.Service!.Id).ToList();
        var alerts = await _context.MileageAlerts
            .AsNoTracking()
            .Include(a => a.Vehicle)
                .ThenInclude(v => v.Client)
            .Include(a => a.Service)
            .Where(a => a.VehicleId == order.VehicleId && serviceIds.Contains(a.ServiceId) && a.IsActive)
            .ToListAsync(cancellationToken);

        var currentKm = order.CurrentKm;
        var results = new List<AlertPreviewDto>();

        foreach (var alert in alerts)
        {
            var vehicleInfo = $"{alert.Vehicle.Brand} {alert.Vehicle.Model} ({alert.Vehicle.LicensePlate})";
            var message = await BuildMessageAsync(alert, currentKm, vehicleInfo, cancellationToken);
            results.Add(new AlertPreviewDto
            {
                AlertId = alert.Id,
                ServiceName = alert.Service?.Name ?? "Servicio",
                Message = message
            });
        }

        return results;
    }

    public async Task<List<NotificationResponse>> ResendRemindersForOrderAsync(int orderId, List<int>? alertIds = null, CancellationToken cancellationToken = default)
    {
        var previews = await BuildPreviewsForOrderAsync(orderId, cancellationToken);

        var filtered = alertIds is { Count: > 0 }
            ? previews.Where(p => alertIds.Contains(p.AlertId)).ToList()
            : previews;

        var results = new List<NotificationResponse>();

        foreach (var preview in filtered)
        {
            try
            {
                var result = await SendReminderAsync(preview.AlertId, cancellationToken);
                if (result is not null)
                    results.Add(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reenviar notificación para alerta {AlertId}", preview.AlertId);
            }
        }

        return results;
    }

    private async Task<string> BuildMessageAsync(MileageAlert alert, int currentKm, string vehicleInfo, CancellationToken cancellationToken)
    {
        var template = await _templateService.GetByKeyAsync("mileage_alert_reminder", cancellationToken);

        var workshop = await _workshopInfoService.GetAsync(cancellationToken);

        var replacements = new Dictionary<string, string>
        {
            ["{VehicleInfo}"] = vehicleInfo,
            ["{Brand}"] = alert.Vehicle.Brand,
            ["{Model}"] = alert.Vehicle.Model,
            ["{LicensePlate}"] = alert.Vehicle.LicensePlate,
            ["{CurrentKm}"] = currentKm.ToString(),
            ["{NextAlertKm}"] = alert.NextAlertKm.ToString(),
            ["{EstimatedWeeklyKm}"] = alert.EstimatedWeeklyKm.ToString(),
            ["{ClientName}"] = alert.Vehicle.Client.FullName,
            ["{ServiceName}"] = alert.Service?.Name ?? "servicio",
            ["{WorkshopName}"] = workshop?.BusinessName ?? "",
            ["{WorkshopRif}"] = workshop?.Rif ?? "",
            ["{WorkshopAddress}"] = workshop?.Address ?? "",
            ["{WorkshopCity}"] = workshop?.City ?? "",
            ["{WorkshopMapsUrl}"] = workshop?.MapsUrl ?? "",
            ["{WorkshopPhone}"] = workshop?.Phone ?? "",
            ["{WorkshopSecondaryPhone}"] = workshop?.SecondaryPhone ?? "",
            ["{WorkshopEmail}"] = workshop?.Email ?? "",
            ["{WorkshopWebsite}"] = workshop?.Website ?? "",
            ["{WorkshopHours}"] = workshop?.OpeningHours ?? "",
        };

        if (template is null)
        {
            return replacements.Aggregate(
                $"Recordatorio: El vehículo {vehicleInfo} requiere atención. " +
                $"Kilometraje actual: {currentKm} km. " +
                $"Próxima alerta: {alert.NextAlertKm} km.",
                (current, kv) => current.Replace(kv.Key, kv.Value));
        }

        return replacements.Aggregate(template.Template, (current, kv) => current.Replace(kv.Key, kv.Value));
    }
}
