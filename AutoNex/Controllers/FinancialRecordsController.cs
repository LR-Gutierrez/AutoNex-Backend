using System.Security.Claims;
using AutoNex.DTOs;
using AutoNex.DTOs.FinancialRecords;
using AutoNex.Helpers;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoNex.Controllers;

[ApiController]
[Route("api/financial-records")]
[Authorize]
public class FinancialRecordsController : ControllerBase
{
    private readonly IFinancialRecordService _financialRecordService;
    private readonly IDashboardNotifier _dashboardNotifier;

    public FinancialRecordsController(IFinancialRecordService financialRecordService, IDashboardNotifier dashboardNotifier)
    {
        _financialRecordService = financialRecordService;
        _dashboardNotifier = dashboardNotifier;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? type, [FromQuery] string? category, [FromQuery] int? page, [FromQuery] int? pageSize)
    {
        var records = await _financialRecordService.GetAllAsync(from, to, type, category, page, pageSize);
        return Ok(ApiResponse<PagedResponse<FinancialRecordResponse>>.Ok(records));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var record = await _financialRecordService.GetByIdAsync(id);
        if (record is null)
            return NotFound(ApiResponse<FinancialRecordResponse>.Fail("Registro financiero no encontrado"));

        return Ok(ApiResponse<FinancialRecordResponse>.Ok(record));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateFinancialRecordRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var record = await _financialRecordService.CreateAsync(request, userId);
        await _dashboardNotifier.NotifyAllAsync();
        return CreatedAtAction(nameof(GetById), new { id = record.Id },
            ApiResponse<FinancialRecordResponse>.Ok(record, "Registro financiero creado exitosamente"));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateFinancialRecordRequest request)
    {
        var record = await _financialRecordService.UpdateAsync(id, request);
        if (record is null)
            return NotFound(ApiResponse<FinancialRecordResponse>.Fail("Registro financiero no encontrado"));

        await _dashboardNotifier.NotifyAllAsync();
        return Ok(ApiResponse<FinancialRecordResponse>.Ok(record, "Registro financiero actualizado exitosamente"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _financialRecordService.DeleteAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<object>.Fail("Registro financiero no encontrado"));

        return NoContent();
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var summary = await _financialRecordService.GetSummaryAsync(from, to);
        return Ok(ApiResponse<FinancialSummaryResponse>.Ok(summary));
    }

    [HttpGet("by-category")]
    public async Task<IActionResult> GetByCategory([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var categories = await _financialRecordService.GetByCategoryAsync(from, to);
        return Ok(ApiResponse<List<CategorySummaryResponse>>.Ok(categories));
    }
}
