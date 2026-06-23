using AutoNex.DTOs;
using AutoNex.DTOs.Suppliers;
using AutoNex.Helpers;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoNex.Controllers;

[ApiController]
[Route("api/suppliers")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken cancellationToken)
    {
        var suppliers = await _supplierService.GetAllAsync(search, page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResponse<SupplierResponse>>.Ok(suppliers));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var supplier = await _supplierService.GetByIdAsync(id, cancellationToken);
        if (supplier is null)
            return NotFound(ApiResponse<SupplierResponse>.Fail("Proveedor no encontrado"));

        return Ok(ApiResponse<SupplierResponse>.Ok(supplier));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateSupplierRequest request, CancellationToken cancellationToken)
    {
        var supplier = await _supplierService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = supplier.Id },
            ApiResponse<SupplierResponse>.Ok(supplier, "Proveedor creado exitosamente"));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSupplierRequest request, CancellationToken cancellationToken)
    {
        var supplier = await _supplierService.UpdateAsync(id, request, cancellationToken);
        if (supplier is null)
            return NotFound(ApiResponse<SupplierResponse>.Fail("Proveedor no encontrado"));

        return Ok(ApiResponse<SupplierResponse>.Ok(supplier, "Proveedor actualizado exitosamente"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _supplierService.DeleteAsync(id, cancellationToken);
        if (!deleted)
            return NotFound(ApiResponse<object>.Fail("Proveedor no encontrado"));

        return NoContent();
    }
}
