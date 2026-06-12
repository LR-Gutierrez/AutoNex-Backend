using AutoNex.DTOs;
using AutoNex.DTOs.Vehicles;
using AutoNex.Helpers;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoNex.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    public VehiclesController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int? page, [FromQuery] int? pageSize)
    {
        var vehicles = await _vehicleService.GetAllAsync(search, page, pageSize);
        return Ok(ApiResponse<PagedResponse<VehicleResponse>>.Ok(vehicles));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var vehicle = await _vehicleService.GetByIdAsync(id);
        if (vehicle is null)
            return NotFound(ApiResponse<VehicleResponse>.Fail("Vehículo no encontrado"));

        return Ok(ApiResponse<VehicleResponse>.Ok(vehicle));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVehicleRequest request)
    {
        try
        {
            var vehicle = await _vehicleService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = vehicle.Id },
                ApiResponse<VehicleResponse>.Ok(vehicle, "Vehículo creado exitosamente"));
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(ApiResponse<VehicleResponse>.Fail(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateVehicleRequest request)
    {
        var vehicle = await _vehicleService.UpdateAsync(id, request);
        if (vehicle is null)
            return NotFound(ApiResponse<VehicleResponse>.Fail("Vehículo no encontrado"));

        return Ok(ApiResponse<VehicleResponse>.Ok(vehicle, "Vehículo actualizado exitosamente"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _vehicleService.DeleteAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<object>.Fail("Vehículo no encontrado"));

        return NoContent();
    }
}
