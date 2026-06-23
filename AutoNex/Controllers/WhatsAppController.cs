using AutoNex.DTOs.WhatsApp;
using AutoNex.Helpers;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoNex.Controllers;

[ApiController]
[Route("api/whatsapp")]
[Authorize]
public class WhatsAppController : ControllerBase
{
    private readonly IWaNotifierService _waNotifierService;

    public WhatsAppController(IWaNotifierService waNotifierService)
    {
        _waNotifierService = waNotifierService;
    }

    [HttpGet("qr")]
    public async Task<ActionResult<ApiResponse<object>>> GetQr(CancellationToken cancellationToken)
    {
        var qr = await _waNotifierService.GetQrAsync(cancellationToken).ConfigureAwait(false);
        return Ok(ApiResponse<object>.Ok(new { qr }));
    }

    [HttpGet("status")]
    public async Task<ActionResult<ApiResponse<object>>> GetStatus(CancellationToken cancellationToken)
    {
        var status = await _waNotifierService.GetStatusAsync(cancellationToken).ConfigureAwait(false);
        if (status is null)
            return StatusCode(502, ApiResponse<object>.Fail("No se pudo conectar con wa-notifier"));

        return Ok(ApiResponse<object>.Ok(status));
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<object>>> Logout(CancellationToken cancellationToken)
    {
        var success = await _waNotifierService.LogoutAsync(cancellationToken).ConfigureAwait(false);
        if (!success)
            return StatusCode(502, ApiResponse<object>.Fail("No se pudo cerrar la sesión de WhatsApp"));

        return Ok(ApiResponse<object>.Ok(new { success = true }));
    }

    [HttpPost("restart")]
    public async Task<ActionResult<ApiResponse<object>>> Restart(CancellationToken cancellationToken)
    {
        var success = await _waNotifierService.RestartAsync(cancellationToken).ConfigureAwait(false);
        if (!success)
            return StatusCode(502, ApiResponse<object>.Fail("No se pudo reiniciar el cliente de WhatsApp"));

        return Ok(ApiResponse<object>.Ok(new { success = true }));
    }

    [HttpPost("test-send")]
    public async Task<ActionResult<ApiResponse<object>>> TestSend(
        [FromBody] TestSendRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(ApiResponse<object>.Fail("Teléfono y mensaje son requeridos"));

        var sent = await _waNotifierService
            .SendWhatsAppAsync(request.Phone, request.Message, cancellationToken)
            .ConfigureAwait(false);

        if (!sent)
            return StatusCode(502, ApiResponse<object>.Fail("Error al enviar el mensaje"));

        return Ok(ApiResponse<object>.Ok(new { success = true }, "Mensaje enviado correctamente"));
    }
}
