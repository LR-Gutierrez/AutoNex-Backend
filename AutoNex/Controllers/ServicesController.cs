using AutoNex.DTOs;
using AutoNex.DTOs.Services;
using AutoNex.Helpers;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoNex.Controllers;

[ApiController]
[Route("api/services")]
[Authorize]
public class ServicesController : ControllerBase
{
    private readonly IServiceCatalogService _serviceCatalogService;

    public ServicesController(IServiceCatalogService serviceCatalogService)
    {
        _serviceCatalogService = serviceCatalogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken cancellationToken)
    {
        var services = await _serviceCatalogService.GetAllAsync(page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResponse<ServiceResponse>>.Ok(services));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var service = await _serviceCatalogService.GetByIdAsync(id, cancellationToken);
        if (service is null)
            return NotFound(ApiResponse<ServiceResponse>.Fail("Servicio no encontrado"));

        return Ok(ApiResponse<ServiceResponse>.Ok(service));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateServiceRequest request, CancellationToken cancellationToken)
    {
        var service = await _serviceCatalogService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = service.Id },
            ApiResponse<ServiceResponse>.Ok(service, "Servicio creado exitosamente"));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceRequest request, CancellationToken cancellationToken)
    {
        var service = await _serviceCatalogService.UpdateAsync(id, request, cancellationToken);
        if (service is null)
            return NotFound(ApiResponse<ServiceResponse>.Fail("Servicio no encontrado"));

        return Ok(ApiResponse<ServiceResponse>.Ok(service, "Servicio actualizado exitosamente"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _serviceCatalogService.DeleteAsync(id, cancellationToken);
        if (!deleted)
            return NotFound(ApiResponse<object>.Fail("Servicio no encontrado"));

        return NoContent();
    }
}
