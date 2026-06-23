using AutoNex.DTOs;
using AutoNex.DTOs.Tools;
using AutoNex.Helpers;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoNex.Controllers;

[ApiController]
[Route("api/tools")]
[Authorize]
public class ToolsController : ControllerBase
{
    private readonly IToolService _toolService;

    public ToolsController(IToolService toolService)
    {
        _toolService = toolService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? category, [FromQuery] string? status, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken cancellationToken)
    {
        var tools = await _toolService.GetAllAsync(search, category, status, page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResponse<ToolResponse>>.Ok(tools));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var tool = await _toolService.GetByIdAsync(id, cancellationToken);
        if (tool is null)
            return NotFound(ApiResponse<ToolResponse>.Fail("Herramienta no encontrada"));

        return Ok(ApiResponse<ToolResponse>.Ok(tool));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateToolRequest request, CancellationToken cancellationToken)
    {
        var tool = await _toolService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = tool.Id },
            ApiResponse<ToolResponse>.Ok(tool, "Herramienta creada exitosamente"));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateToolRequest request, CancellationToken cancellationToken)
    {
        var tool = await _toolService.UpdateAsync(id, request, cancellationToken);
        if (tool is null)
            return NotFound(ApiResponse<ToolResponse>.Fail("Herramienta no encontrada"));

        return Ok(ApiResponse<ToolResponse>.Ok(tool, "Herramienta actualizada exitosamente"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _toolService.DeleteAsync(id, cancellationToken);
        if (!deleted)
            return NotFound(ApiResponse<object>.Fail("Herramienta no encontrada"));

        return NoContent();
    }
}
