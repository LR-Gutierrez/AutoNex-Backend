using AutoNex.DTOs;
using AutoNex.DTOs.InventoryMovements;
using AutoNex.Helpers;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoNex.Controllers;

[ApiController]
[Route("api/inventory-movements")]
[Authorize]
public class InventoryMovementsController : ControllerBase
{
    private readonly IInventoryMovementService _inventoryMovementService;

    public InventoryMovementsController(IInventoryMovementService inventoryMovementService)
    {
        _inventoryMovementService = inventoryMovementService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? consumableId, [FromQuery] int? toolId, [FromQuery] int? page, [FromQuery] int? pageSize)
    {
        var movements = await _inventoryMovementService.GetAllAsync(consumableId, toolId, page, pageSize);
        return Ok(ApiResponse<PagedResponse<InventoryMovementResponse>>.Ok(movements));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var movement = await _inventoryMovementService.GetByIdAsync(id);
        if (movement is null)
            return NotFound(ApiResponse<InventoryMovementResponse>.Fail("Movimiento no encontrado"));

        return Ok(ApiResponse<InventoryMovementResponse>.Ok(movement));
    }
}
