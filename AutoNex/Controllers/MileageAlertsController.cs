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

    public MileageAlertsController(IMileageAlertService mileageAlertService, INotificationService notificationService)
    {
        _mileageAlertService = mileageAlertService;
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? due, [FromQuery] int? page, [FromQuery] int? pageSize)
    {
        var alerts = await _mileageAlertService.GetAllAsync(due, page, pageSize);
        return Ok(ApiResponse<PagedResponse<MileageAlertResponse>>.Ok(alerts));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var alert = await _mileageAlertService.GetByIdAsync(id);
        if (alert is null)
            return NotFound(ApiResponse<MileageAlertResponse>.Fail("Alerta no encontrada"));

        return Ok(ApiResponse<MileageAlertResponse>.Ok(alert));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMileageAlertRequest request)
    {
        try
        {
            var alert = await _mileageAlertService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = alert.Id },
                ApiResponse<MileageAlertResponse>.Ok(alert, "Alerta creada exitosamente"));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<MileageAlertResponse>.Fail(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMileageAlertRequest request)
    {
        var alert = await _mileageAlertService.UpdateAsync(id, request);
        if (alert is null)
            return NotFound(ApiResponse<MileageAlertResponse>.Fail("Alerta no encontrada"));

        return Ok(ApiResponse<MileageAlertResponse>.Ok(alert, "Alerta actualizada exitosamente"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _mileageAlertService.DeleteAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<object>.Fail("Alerta no encontrada"));

        return Ok(ApiResponse<object>.Ok(null!, "Alerta desactivada exitosamente"));
    }

    [HttpPost("{id}/send")]
    public async Task<IActionResult> SendReminder(int id)
    {
        try
        {
            var notification = await _notificationService.SendReminderAsync(id);
            if (notification is null)
                return NotFound(ApiResponse<object>.Fail("Alerta no encontrada"));

            return Ok(ApiResponse<NotificationResponse>.Ok(notification, "Recordatorio enviado exitosamente"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }
}
