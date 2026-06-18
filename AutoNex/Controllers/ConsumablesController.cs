using AutoNex.DTOs;
using AutoNex.DTOs.Consumables;
using AutoNex.Helpers;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoNex.Controllers;

[ApiController]
[Route("api/consumables")]
[Authorize]
public class ConsumablesController : ControllerBase
{
    private readonly IConsumableService _consumableService;
    private readonly IDashboardNotifier _dashboardNotifier;

    public ConsumablesController(IConsumableService consumableService, IDashboardNotifier dashboardNotifier)
    {
        _consumableService = consumableService;
        _dashboardNotifier = dashboardNotifier;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category, [FromQuery] int? page, [FromQuery] int? pageSize)
    {
        var consumables = await _consumableService.GetAllAsync(category, page, pageSize);
        return Ok(ApiResponse<PagedResponse<ConsumableResponse>>.Ok(consumables));
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock()
    {
        var consumables = await _consumableService.GetLowStockAsync();
        return Ok(ApiResponse<List<ConsumableResponse>>.Ok(consumables));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var consumable = await _consumableService.GetByIdAsync(id);
        if (consumable is null)
            return NotFound(ApiResponse<ConsumableResponse>.Fail("Consumible no encontrado"));

        return Ok(ApiResponse<ConsumableResponse>.Ok(consumable));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateConsumableRequest request)
    {
        var consumable = await _consumableService.CreateAsync(request);
        await _dashboardNotifier.NotifyAllAsync();
        return CreatedAtAction(nameof(GetById), new { id = consumable.Id },
            ApiResponse<ConsumableResponse>.Ok(consumable, "Consumible creado exitosamente"));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateConsumableRequest request)
    {
        var consumable = await _consumableService.UpdateAsync(id, request);
        if (consumable is null)
            return NotFound(ApiResponse<ConsumableResponse>.Fail("Consumible no encontrado"));

        await _dashboardNotifier.NotifyAllAsync();
        return Ok(ApiResponse<ConsumableResponse>.Ok(consumable, "Consumible actualizado exitosamente"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _consumableService.DeleteAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<object>.Fail("Consumible no encontrado"));

        return NoContent();
    }
}
