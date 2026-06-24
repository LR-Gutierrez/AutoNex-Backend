using System.Security.Claims;
using AutoNex.DTOs;
using AutoNex.DTOs.Notifications;
using AutoNex.DTOs.ServiceOrders;
using AutoNex.Helpers;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoNex.Controllers;

[ApiController]
[Route("api/service-orders")]
[Authorize]
public class ServiceOrdersController : ControllerBase
{
    private readonly IServiceOrderService _serviceOrderService;
    private readonly IDashboardNotifier _dashboardNotifier;
    private readonly INotificationService _notificationService;

    public ServiceOrdersController(IServiceOrderService serviceOrderService, IDashboardNotifier dashboardNotifier, INotificationService notificationService)
    {
        _serviceOrderService = serviceOrderService;
        _dashboardNotifier = dashboardNotifier;
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? clientId,
        [FromQuery] int? vehicleId,
        [FromQuery] string? status,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var orders = await _serviceOrderService.GetAllAsync(from, to, clientId, vehicleId, status, page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResponse<ServiceOrderResponse>>.Ok(orders));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var order = await _serviceOrderService.GetByIdAsync(id, cancellationToken);
        if (order is null)
            return NotFound(ApiResponse<ServiceOrderResponse>.Fail("Orden no encontrada"));

        return Ok(ApiResponse<ServiceOrderResponse>.Ok(order));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateServiceOrderRequest request, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var order = await _serviceOrderService.CreateAsync(request, userId, cancellationToken);
        await _dashboardNotifier.NotifyAllAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = order.Id },
            ApiResponse<ServiceOrderResponse>.Ok(order, "Orden creada exitosamente"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await _serviceOrderService.UpdateAsync(id, request, cancellationToken);
        if (order is null)
            return NotFound(ApiResponse<ServiceOrderResponse>.Fail("Orden no encontrada"));

        await _dashboardNotifier.NotifyAllAsync(cancellationToken);
        return Ok(ApiResponse<ServiceOrderResponse>.Ok(order, "Orden actualizada exitosamente"));
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateServiceOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var order = await _serviceOrderService.UpdateStatusAsync(id, request, cancellationToken);
        if (order is null)
            return NotFound(ApiResponse<ServiceOrderResponse>.Fail("Orden no encontrada"));

        await _dashboardNotifier.NotifyAllAsync(cancellationToken);
        return Ok(ApiResponse<ServiceOrderResponse>.Ok(order, "Estado actualizado exitosamente"));
    }

    [HttpPost("{id}/pay")]
    public async Task<IActionResult> Pay(int id, [FromBody] PayOrderRequest request, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var order = await _serviceOrderService.PayAsync(id, userId, request, cancellationToken);

        await _dashboardNotifier.NotifyAllAsync(cancellationToken);
        return Ok(ApiResponse<ServiceOrderResponse>.Ok(order, "Orden marcada como pagada exitosamente"));
    }

    [HttpGet("{id}/alert-preview")]
    public async Task<IActionResult> GetAlertPreview(int id, CancellationToken cancellationToken)
    {
        var previews = await _notificationService.BuildPreviewsForOrderAsync(id, cancellationToken);
        return Ok(ApiResponse<List<AlertPreviewDto>>.Ok(previews));
    }

    [HttpPost("{id}/resend-alerts")]
    public async Task<IActionResult> ResendAlerts(int id, [FromBody] ResendAlertsRequest request, CancellationToken cancellationToken)
    {
        var results = await _notificationService.ResendRemindersForOrderAsync(id, request.AlertIds, cancellationToken);
        return Ok(ApiResponse<object>.Ok(
            new { sentCount = results.Count, notifications = results },
            $"{results.Count} notificación(es) reenviada(s)"
        ));
    }
}
