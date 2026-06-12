using AutoNex.DTOs.Suppliers;
using AutoNex.Helpers;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoNex.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var suppliers = await _supplierService.GetAllAsync();
        return Ok(ApiResponse<List<SupplierResponse>>.Ok(suppliers));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var supplier = await _supplierService.GetByIdAsync(id);
        if (supplier is null)
            return NotFound(ApiResponse<SupplierResponse>.Fail("Proveedor no encontrado"));

        return Ok(ApiResponse<SupplierResponse>.Ok(supplier));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupplierRequest request)
    {
        var supplier = await _supplierService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = supplier.Id },
            ApiResponse<SupplierResponse>.Ok(supplier, "Proveedor creado exitosamente"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSupplierRequest request)
    {
        var supplier = await _supplierService.UpdateAsync(id, request);
        if (supplier is null)
            return NotFound(ApiResponse<SupplierResponse>.Fail("Proveedor no encontrado"));

        return Ok(ApiResponse<SupplierResponse>.Ok(supplier, "Proveedor actualizado exitosamente"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _supplierService.DeleteAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<object>.Fail("Proveedor no encontrado"));

        return Ok(ApiResponse<object>.Ok(null!, "Proveedor eliminado exitosamente"));
    }
}
