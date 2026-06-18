using AutoNex.DTOs;
using AutoNex.DTOs.Notifications;
using AutoNex.Helpers;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoNex.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? clientId, [FromQuery] int? vehicleId, [FromQuery] string? status, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken cancellationToken)
    {
        var notifications = await _notificationService.GetAllAsync(clientId, vehicleId, status, page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResponse<NotificationResponse>>.Ok(notifications));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var notification = await _notificationService.GetByIdAsync(id, cancellationToken);
        if (notification is null)
            return NotFound(ApiResponse<NotificationResponse>.Fail("Notificación no encontrada"));

        return Ok(ApiResponse<NotificationResponse>.Ok(notification));
    }

    [HttpPost("send-whatsapp")]
    public async Task<IActionResult> SendWhatsApp([FromBody] SendNotificationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var notification = await _notificationService.SendAsync(request, cancellationToken);
            return Ok(ApiResponse<NotificationResponse>.Ok(notification, "Notificación enviada exitosamente"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }
}
