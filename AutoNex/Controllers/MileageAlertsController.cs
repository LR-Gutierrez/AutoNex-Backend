using AutoNex.DTOs.MileageAlerts;
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

    public MileageAlertsController(IMileageAlertService mileageAlertService)
    {
        _mileageAlertService = mileageAlertService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? due)
    {
        var alerts = await _mileageAlertService.GetAllAsync(due);
        return Ok(ApiResponse<List<MileageAlertResponse>>.Ok(alerts));
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
        var alert = await _mileageAlertService.GetByIdAsync(id);
        if (alert is null)
            return NotFound(ApiResponse<object>.Fail("Alerta no encontrada"));

        return Ok(ApiResponse<object>.Ok(new
        {
            alert.Id,
            alert.VehicleInfo,
            message = $"Recordatorio: El vehículo {alert.VehicleInfo} requiere atención. " +
                      $"Kilometraje alerta: {alert.NextAlertKm} km. " +
                      "(Envío real se integrará en Fase 6 - Notificaciones)"
        }, "Recordatorio generado"));
    }
}
