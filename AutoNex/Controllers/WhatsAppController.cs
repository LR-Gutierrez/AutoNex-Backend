using System.Security.Claims;
using AutoNex.Data;
using AutoNex.DTOs.WhatsApp;
using AutoNex.Helpers;
using AutoNex.Services;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoNex.Controllers;

[ApiController]
[Route("api/whatsapp")]
[Authorize]
public class WhatsAppController : ControllerBase
{
    private readonly IWaNotifierService _waNotifierService;
    private readonly AppDbContext _context;
    private readonly WhatsAppSendQueue _sendQueue;

    public WhatsAppController(IWaNotifierService waNotifierService, AppDbContext context, WhatsAppSendQueue sendQueue)
    {
        _waNotifierService = waNotifierService;
        _context = context;
        _sendQueue = sendQueue;
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

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            .ConfigureAwait(false);
        var sentBy = user?.FullName ?? "Unknown";

        var phone = request.Phone;
        var message = request.Message;
        _sendQueue.Enqueue(async (sp, ct) =>
        {
            var waNotifier = sp.GetRequiredService<IWaNotifierService>();
            await waNotifier.SendWhatsAppAsync(phone, message, "Test", sentBy, ct).ConfigureAwait(false);
        });

        return Ok(ApiResponse<object>.Ok(new { success = true }, "Mensaje encolado para envío en segundo plano"));
    }

    [HttpGet("logs")]
    public async Task<ActionResult<ApiResponse<object>>> GetLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _context.WhatsAppMessageLogs
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt);

        var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new WhatsAppMessageLogResponse(
                x.Id,
                x.Phone,
                x.Message,
                x.Type,
                x.Success,
                x.ErrorMessage,
                x.SentBy,
                x.CreatedAt
            ))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Ok(ApiResponse<object>.Ok(new
        {
            items,
            total,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling((double)total / pageSize),
        }));
    }
}
