using AutoNex.DTOs;
using AutoNex.DTOs.MessageTemplates;
using AutoNex.Helpers;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoNex.Controllers;

[ApiController]
[Route("api/message-templates")]
[Authorize(Roles = "Admin")]
public class MessageTemplatesController : ControllerBase
{
    private readonly IMessageTemplateService _messageTemplateService;

    public MessageTemplatesController(IMessageTemplateService messageTemplateService)
    {
        _messageTemplateService = messageTemplateService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? page, [FromQuery] int? pageSize, [FromQuery] string? search, CancellationToken cancellationToken)
    {
        var templates = await _messageTemplateService.GetAllAsync(page, pageSize, search, cancellationToken);
        return Ok(ApiResponse<PagedResponse<MessageTemplateResponse>>.Ok(templates));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var template = await _messageTemplateService.GetByIdAsync(id, cancellationToken);
        if (template is null)
            return NotFound(ApiResponse<MessageTemplateResponse>.Fail("Template no encontrado"));

        return Ok(ApiResponse<MessageTemplateResponse>.Ok(template));
    }

    [HttpGet("by-key/{key}")]
    public async Task<IActionResult> GetByKey(string key, CancellationToken cancellationToken)
    {
        var template = await _messageTemplateService.GetByKeyAsync(key, cancellationToken);
        if (template is null)
            return NotFound(ApiResponse<MessageTemplateResponse>.Fail("Template no encontrado"));

        return Ok(ApiResponse<MessageTemplateResponse>.Ok(template));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMessageTemplateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var template = await _messageTemplateService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = template.Id },
                ApiResponse<MessageTemplateResponse>.Ok(template, "Template creado exitosamente"));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMessageTemplateRequest request, CancellationToken cancellationToken)
    {
        var template = await _messageTemplateService.UpdateAsync(id, request, cancellationToken);
        if (template is null)
            return NotFound(ApiResponse<MessageTemplateResponse>.Fail("Template no encontrado"));

        return Ok(ApiResponse<MessageTemplateResponse>.Ok(template, "Template actualizado exitosamente"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _messageTemplateService.DeleteAsync(id, cancellationToken);
        if (!deleted)
            return NotFound(ApiResponse<object>.Fail("Template no encontrado"));

        return NoContent();
    }

    [HttpPost("{id}/activate")]
    public async Task<IActionResult> Activate(int id, CancellationToken cancellationToken)
    {
        var success = await _messageTemplateService.SetActiveAsync(id, cancellationToken);
        if (!success)
            return NotFound(ApiResponse<object>.Fail("Template no encontrado"));

        var template = await _messageTemplateService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<MessageTemplateResponse>.Ok(template!, "Template activado exitosamente"));
    }
}
