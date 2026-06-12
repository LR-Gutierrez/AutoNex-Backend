using AutoNex.DTOs.Services;
using AutoNex.Helpers;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoNex.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServicesController : ControllerBase
{
    private readonly IServiceCatalogService _serviceCatalogService;

    public ServicesController(IServiceCatalogService serviceCatalogService)
    {
        _serviceCatalogService = serviceCatalogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var services = await _serviceCatalogService.GetAllAsync();
        return Ok(ApiResponse<List<ServiceResponse>>.Ok(services));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var service = await _serviceCatalogService.GetByIdAsync(id);
        if (service is null)
            return NotFound(ApiResponse<ServiceResponse>.Fail("Servicio no encontrado"));

        return Ok(ApiResponse<ServiceResponse>.Ok(service));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateServiceRequest request)
    {
        var service = await _serviceCatalogService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = service.Id },
            ApiResponse<ServiceResponse>.Ok(service, "Servicio creado exitosamente"));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceRequest request)
    {
        var service = await _serviceCatalogService.UpdateAsync(id, request);
        if (service is null)
            return NotFound(ApiResponse<ServiceResponse>.Fail("Servicio no encontrado"));

        return Ok(ApiResponse<ServiceResponse>.Ok(service, "Servicio actualizado exitosamente"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _serviceCatalogService.DeleteAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<object>.Fail("Servicio no encontrado"));

        return Ok(ApiResponse<object>.Ok(null!, "Servicio eliminado exitosamente"));
    }
}
