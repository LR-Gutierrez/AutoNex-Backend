using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AutoNex.Data;
using AutoNex.Models;
using AutoNex.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutoNex.Services.Implementations;

public class WaNotifierService : IWaNotifierService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WaNotifierService> _logger;
    private readonly AppDbContext _context;
    private readonly IWaNotifierTokenStore _tokenStore;

    public WaNotifierService(
        HttpClient httpClient,
        ILogger<WaNotifierService> logger,
        AppDbContext context,
        IWaNotifierTokenStore tokenStore)
    {
        _httpClient = httpClient;
        _logger = logger;
        _context = context;
        _tokenStore = tokenStore;
    }

    public async Task<bool> SendWhatsAppAsync(string to, string message, string? source = null, string? sentBy = null, string? correlationId = null, CancellationToken cancellationToken = default)
    {
        var phone = NormalizePhone(to);
        var log = new WhatsAppMessageLog
        {
            Phone = phone,
            Message = message,
            Type = source is not null && source.Equals("Reminder", StringComparison.OrdinalIgnoreCase) ? "Reminder" : "Test",
            SentBy = sentBy ?? "System",
        };

        try
        {
            var token = await _tokenStore.GetTokenAsync(cancellationToken).ConfigureAwait(false);

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/messages/send")
            {
                Content = JsonContent.Create(new { phone, message, correlationId })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            log.Success = true;
            _logger.LogInformation("WhatsApp message sent to {Phone} via wa-notifier", phone);
            return true;
        }
        catch (Exception ex)
        {
            log.Success = false;
            log.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Error sending WhatsApp via wa-notifier to {To}", to);
            return false;
        }
        finally
        {
            _context.WhatsAppMessageLogs.Add(log);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
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

    private static string NormalizePhone(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        // Venezuelan numbers stored as (0412) 123-4567 → normalize to 584121234567
        if (digits.StartsWith('0') && digits.Length > 1)
            return "58" + digits[1..];
        return digits;
    }
}
