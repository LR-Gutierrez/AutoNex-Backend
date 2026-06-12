using AutoNex.DTOs;
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
    public async Task<IActionResult> GetAll([FromQuery] int? page, [FromQuery] int? pageSize)
    {
        var services = await _serviceCatalogService.GetAllAsync(page, pageSize);
        return Ok(ApiResponse<PagedResponse<ServiceResponse>>.Ok(services));
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

    // Service Variants

    [HttpGet("{serviceId}/variants")]
    public async Task<IActionResult> GetVariants(int serviceId)
    {
        var variants = await _serviceCatalogService.GetVariantsAsync(serviceId);
        return Ok(ApiResponse<List<ServiceVariantResponse>>.Ok(variants));
    }

    [HttpGet("variants/{id}", Name = "GetVariantById")]
    public async Task<IActionResult> GetVariantById(int id)
    {
        var variant = await _serviceCatalogService.GetVariantByIdAsync(id);
        if (variant is null)
            return NotFound(ApiResponse<ServiceVariantResponse>.Fail("Variante no encontrada"));

        return Ok(ApiResponse<ServiceVariantResponse>.Ok(variant));
    }

    [HttpPost("{serviceId}/variants")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateVariant(int serviceId, [FromBody] CreateServiceVariantRequest request)
    {
        try
        {
            var variant = await _serviceCatalogService.CreateVariantAsync(serviceId, request);
            return CreatedAtRoute("GetVariantById", new { id = variant.Id },
                ApiResponse<ServiceVariantResponse>.Ok(variant, "Variante creada exitosamente"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ServiceVariantResponse>.Fail(ex.Message));
        }
    }

    [HttpPut("variants/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateVariant(int id, [FromBody] UpdateServiceVariantRequest request)
    {
        var variant = await _serviceCatalogService.UpdateVariantAsync(id, request);
        if (variant is null)
            return NotFound(ApiResponse<ServiceVariantResponse>.Fail("Variante no encontrada"));

        return Ok(ApiResponse<ServiceVariantResponse>.Ok(variant, "Variante actualizada exitosamente"));
    }

    [HttpDelete("variants/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteVariant(int id)
    {
        var deleted = await _serviceCatalogService.DeleteVariantAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<object>.Fail("Variante no encontrada"));

        return Ok(ApiResponse<object>.Ok(null!, "Variante desactivada exitosamente"));
    }
}
