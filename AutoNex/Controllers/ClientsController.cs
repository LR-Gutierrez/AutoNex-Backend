using AutoNex.DTOs;
using AutoNex.DTOs.Clients;
using AutoNex.Helpers;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoNex.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int? page, [FromQuery] int? pageSize)
    {
        var clients = await _clientService.GetAllAsync(search, page, pageSize);
        return Ok(ApiResponse<PagedResponse<ClientResponse>>.Ok(clients));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var client = await _clientService.GetByIdAsync(id);
        if (client is null)
            return NotFound(ApiResponse<ClientResponse>.Fail("Cliente no encontrado"));

        return Ok(ApiResponse<ClientResponse>.Ok(client));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClientRequest request)
    {
        var client = await _clientService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = client.Id },
            ApiResponse<ClientResponse>.Ok(client, "Cliente creado exitosamente"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClientRequest request)
    {
        var client = await _clientService.UpdateAsync(id, request);
        if (client is null)
            return NotFound(ApiResponse<ClientResponse>.Fail("Cliente no encontrado"));

        return Ok(ApiResponse<ClientResponse>.Ok(client, "Cliente actualizado exitosamente"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _clientService.DeleteAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<object>.Fail("Cliente no encontrado"));

        return NoContent();
    }
}
