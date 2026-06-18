using AutoNex.DTOs;
using AutoNex.DTOs.MileageAlerts;
using AutoNex.DTOs.Notifications;
using AutoNex.Helpers;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoNex.Controllers;

[ApiController]
[Route("api/mileage-alerts")]
[Authorize]
public class MileageAlertsController : ControllerBase
{
    private readonly IMileageAlertService _mileageAlertService;
    private readonly INotificationService _notificationService;
    private readonly IDashboardNotifier _dashboardNotifier;

    public MileageAlertsController(
        IMileageAlertService mileageAlertService,
        INotificationService notificationService,
        IDashboardNotifier dashboardNotifier)
    {
        _mileageAlertService = mileageAlertService;
        _notificationService = notificationService;
        _dashboardNotifier = dashboardNotifier;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? due, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken cancellationToken)
    {
        var alerts = await _mileageAlertService.GetAllAsync(due, page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResponse<MileageAlertResponse>>.Ok(alerts));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var alert = await _mileageAlertService.GetByIdAsync(id, cancellationToken);
        if (alert is null)
            return NotFound(ApiResponse<MileageAlertResponse>.Fail("Alerta no encontrada"));

        return Ok(ApiResponse<MileageAlertResponse>.Ok(alert));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMileageAlertRequest request, CancellationToken cancellationToken)
    {
        var alert = await _mileageAlertService.CreateAsync(request, cancellationToken);
        await _dashboardNotifier.NotifyAllAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = alert.Id },
            ApiResponse<MileageAlertResponse>.Ok(alert, "Alerta creada exitosamente"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMileageAlertRequest request, CancellationToken cancellationToken)
    {
        var alert = await _mileageAlertService.UpdateAsync(id, request, cancellationToken);
        if (alert is null)
            return NotFound(ApiResponse<MileageAlertResponse>.Fail("Alerta no encontrada"));

        await _dashboardNotifier.NotifyAllAsync(cancellationToken);
        return Ok(ApiResponse<MileageAlertResponse>.Ok(alert, "Alerta actualizada exitosamente"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _mileageAlertService.DeleteAsync(id, cancellationToken);
        if (!deleted)
            return NotFound(ApiResponse<object>.Fail("Alerta no encontrada"));

        await _dashboardNotifier.NotifyAllAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("from-order/{orderId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateFromOrder(int orderId, CancellationToken cancellationToken)
    {
        try
        {
            var alerts = await _mileageAlertService.CreateOrUpdateFromOrderAsync(orderId, cancellationToken);
            await _dashboardNotifier.NotifyAllAsync(cancellationToken);
            return Ok(ApiResponse<List<MileageAlertResponse>>.Ok(alerts, "Alertas creadas/actualizadas exitosamente"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpPost("{id}/attend")]
    public async Task<IActionResult> Attend(int id, CancellationToken cancellationToken)
    {
        var alert = await _mileageAlertService.AttendAsync(id, cancellationToken);
        if (alert is null)
            return NotFound(ApiResponse<MileageAlertResponse>.Fail("Alerta no encontrada"));

        await _dashboardNotifier.NotifyAllAsync(cancellationToken);
        return Ok(ApiResponse<MileageAlertResponse>.Ok(alert, "Alerta atendida exitosamente"));
    }

    [HttpPost("{id}/send")]
    public async Task<IActionResult> SendReminder(int id, CancellationToken cancellationToken)
    {
        var notification = await _notificationService.SendReminderAsync(id, cancellationToken);
        if (notification is null)
            return NotFound(ApiResponse<object>.Fail("Alerta no encontrada"));

        return Ok(ApiResponse<NotificationResponse>.Ok(notification, "Recordatorio enviado exitosamente"));
    }
}
