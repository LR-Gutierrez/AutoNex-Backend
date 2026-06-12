using System.Security.Claims;
using AutoNex.DTOs;
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

    public ServiceOrdersController(IServiceOrderService serviceOrderService)
    {
        _serviceOrderService = serviceOrderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? clientId,
        [FromQuery] int? vehicleId,
        [FromQuery] string? status,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        var orders = await _serviceOrderService.GetAllAsync(from, to, clientId, vehicleId, status, page, pageSize);
        return Ok(ApiResponse<PagedResponse<ServiceOrderResponse>>.Ok(orders));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _serviceOrderService.GetByIdAsync(id);
        if (order is null)
            return NotFound(ApiResponse<ServiceOrderResponse>.Fail("Orden no encontrada"));

        return Ok(ApiResponse<ServiceOrderResponse>.Ok(order));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateServiceOrderRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var order = await _serviceOrderService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = order.Id },
            ApiResponse<ServiceOrderResponse>.Ok(order, "Orden creada exitosamente"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceOrderRequest request)
    {
        var order = await _serviceOrderService.UpdateAsync(id, request);
        if (order is null)
            return NotFound(ApiResponse<ServiceOrderResponse>.Fail("Orden no encontrada"));

        return Ok(ApiResponse<ServiceOrderResponse>.Ok(order, "Orden actualizada exitosamente"));
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateServiceOrderStatusRequest request)
    {
        var order = await _serviceOrderService.UpdateStatusAsync(id, request);
        if (order is null)
            return NotFound(ApiResponse<ServiceOrderResponse>.Fail("Orden no encontrada"));

        return Ok(ApiResponse<ServiceOrderResponse>.Ok(order, "Estado actualizado exitosamente"));
    }
}
