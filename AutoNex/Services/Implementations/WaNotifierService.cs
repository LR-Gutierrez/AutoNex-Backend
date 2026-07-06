using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AutoNex.Data;
using AutoNex.Hubs;
using AutoNex.Models;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutoNex.Services.Implementations;

public class WaNotifierService : IWaNotifierService
{
    private readonly HttpClient _httpClient;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WaNotifierService> _logger;
    private readonly AppDbContext _context;
    private readonly IWaNotifierTokenStore _tokenStore;

    public WaNotifierService(
        HttpClient httpClient,
        IServiceScopeFactory scopeFactory,
        ILogger<WaNotifierService> logger,
        AppDbContext context,
        IWaNotifierTokenStore tokenStore)
    {
        _httpClient = httpClient;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _context = context;
        _tokenStore = tokenStore;
    }

    public async Task<WaSendResult> SendWhatsAppAsync(string to, string message, string? source = null, string? sentBy = null, string? correlationId = null, int? logId = null, CancellationToken cancellationToken = default)
    {
        var phone = NormalizePhone(to);

        WhatsAppMessageLog log;

        if (logId.HasValue)
        {
            log = await _context.WhatsAppMessageLogs.FindAsync([logId.Value], cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException($"Log with Id {logId} not found");
        }
        else
        {
            log = new WhatsAppMessageLog
            {
                Phone = phone,
                Message = message,
                Type = source is not null && source.Equals("Reminder", StringComparison.OrdinalIgnoreCase) ? "Reminder" : "Test",
                SentBy = sentBy ?? "System",
                Status = "Sending"
            };

            _context.WhatsAppMessageLogs.Add(log);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        var logIdValue = log.Id;

        var deliveryId = correlationId != null
            ? $"notif:{correlationId}:log:{logIdValue}"
            : $"log:{logIdValue}";

        // Fire HTTP asynchronously — no bloquea la cola
        _ = HandleSendResponseAsync(phone, message, deliveryId, logIdValue);

        return new WaSendResult(true, logIdValue);
    }

    private async Task HandleSendResponseAsync(string phone, string message, string deliveryId, int logId)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var sp = scope.ServiceProvider;
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var tokenStore = sp.GetRequiredService<IWaNotifierTokenStore>();
            var hubContext = sp.GetRequiredService<IHubContext<WhatsAppHub>>();
            var dbContext = sp.GetRequiredService<AppDbContext>();

            var httpClient = httpClientFactory.CreateClient("WaNotifier");
            var token = await tokenStore.GetTokenAsync(CancellationToken.None).ConfigureAwait(false);

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/messages/send")
            {
                Content = JsonContent.Create(new { phone, message, correlationId = deliveryId })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.SendAsync(request, CancellationToken.None).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<SendResponse>(CancellationToken.None).ConfigureAwait(false);

            // Solo actualiza si NotifyDelivery no lo hizo ya
            var log = await dbContext.WhatsAppMessageLogs.FindAsync(logId).ConfigureAwait(false);
            if (log is not null && log.Status == "Sending")
            {
                log.Status = result?.Success == true ? "Sent" : "Failed";
                log.ErrorMessage = log.Status == "Failed" ? result?.Message : null;
                await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
                _logger.LogInformation("Log {LogId} updated to {Status} via HTTP response fallback", logId, log.Status);
            }

            // Emit MessageSent (si NotifyDelivery ya lo emitió, es idempotente para el frontend)
            await hubContext.Clients.Group("whatsapp").SendAsync("MessageSent", new
            {
                messageId = (string?)null,
                logId,
                success = result?.Success == true,
                status = result?.Success == true ? "Sent" : "Failed",
                phone = (string?)null,
                error = result?.Success == true ? null : result?.Message,
            }, CancellationToken.None).ConfigureAwait(false);

            _logger.LogInformation("WhatsApp message processed for {Phone} via HTTP response", phone);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "HTTP response handler for log {LogId} failed — relying on NotifyDelivery callback", logId);
        }
    }

    public async Task<string?> GetQrAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/whatsapp/qr", cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<QrResponse>(cancellationToken).ConfigureAwait(false);
            return result?.Qr;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching WhatsApp QR from wa-notifier");
            return null;
        }
    }

    public async Task<object?> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await _tokenStore.GetTokenAsync(cancellationToken).ConfigureAwait(false);

            var request = new HttpRequestMessage(HttpMethod.Get, "/api/whatsapp/status");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<object>(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching WhatsApp status from wa-notifier");
            return null;
        }
    }

    public async Task<bool> LogoutAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await _tokenStore.GetTokenAsync(cancellationToken).ConfigureAwait(false);

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/whatsapp/logout");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("WhatsApp session logged out");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging out WhatsApp via wa-notifier");
            return false;
        }
    }

    public async Task<bool> RestartAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await _tokenStore.GetTokenAsync(cancellationToken).ConfigureAwait(false);

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/whatsapp/restart");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("WhatsApp client restart requested");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restarting WhatsApp via wa-notifier");
            return false;
        }
    }

    private sealed record QrResponse([property: JsonPropertyName("qr")] string? Qr);
    private sealed record SendResponse([property: JsonPropertyName("success")] bool Success, [property: JsonPropertyName("message")] string? Message);

    private static string NormalizePhone(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.StartsWith('0') && digits.Length > 1)
            return "58" + digits[1..];
        return digits;
    }
}
