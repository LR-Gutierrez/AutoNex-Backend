using AutoNex.DTOs;
using AutoNex.DTOs.ToolCategories;
using AutoNex.Helpers;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoNex.Controllers;

[ApiController]
[Route("api/tool-categories")]
[Authorize]
public class ToolCategoriesController : ControllerBase
{
    private readonly IToolCategoryService _toolCategoryService;

    public ToolCategoriesController(IToolCategoryService toolCategoryService)
    {
        _toolCategoryService = toolCategoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? page, [FromQuery] int? pageSize)
    {
        var categories = await _toolCategoryService.GetAllAsync(page, pageSize);
        return Ok(ApiResponse<PagedResponse<ToolCategoryResponse>>.Ok(categories));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _toolCategoryService.GetByIdAsync(id);
        if (category is null)
            return NotFound(ApiResponse<ToolCategoryResponse>.Fail("Categoría no encontrada"));

        return Ok(ApiResponse<ToolCategoryResponse>.Ok(category));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateToolCategoryRequest request)
    {
        var category = await _toolCategoryService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = category.Id },
            ApiResponse<ToolCategoryResponse>.Ok(category, "Categoría creada exitosamente"));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateToolCategoryRequest request)
    {
        var category = await _toolCategoryService.UpdateAsync(id, request);
        if (category is null)
            return NotFound(ApiResponse<ToolCategoryResponse>.Fail("Categoría no encontrada"));

        return Ok(ApiResponse<ToolCategoryResponse>.Ok(category, "Categoría actualizada exitosamente"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _toolCategoryService.DeleteAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<object>.Fail("Categoría no encontrada"));

        return NoContent();
    }
}
