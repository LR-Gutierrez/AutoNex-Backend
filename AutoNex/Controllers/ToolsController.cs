using AutoNex.DTOs.Tools;
using AutoNex.Helpers;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoNex.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ToolsController : ControllerBase
{
    private readonly IToolService _toolService;

    public ToolsController(IToolService toolService)
    {
        _toolService = toolService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category, [FromQuery] string? status)
    {
        var tools = await _toolService.GetAllAsync(category, status);
        return Ok(ApiResponse<List<ToolResponse>>.Ok(tools));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var tool = await _toolService.GetByIdAsync(id);
        if (tool is null)
            return NotFound(ApiResponse<ToolResponse>.Fail("Herramienta no encontrada"));

        return Ok(ApiResponse<ToolResponse>.Ok(tool));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateToolRequest request)
    {
        var tool = await _toolService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = tool.Id },
            ApiResponse<ToolResponse>.Ok(tool, "Herramienta creada exitosamente"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateToolRequest request)
    {
        var tool = await _toolService.UpdateAsync(id, request);
        if (tool is null)
            return NotFound(ApiResponse<ToolResponse>.Fail("Herramienta no encontrada"));

        return Ok(ApiResponse<ToolResponse>.Ok(tool, "Herramienta actualizada exitosamente"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _toolService.DeleteAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<object>.Fail("Herramienta no encontrada"));

        return Ok(ApiResponse<object>.Ok(null!, "Herramienta eliminada exitosamente"));
    }
}
