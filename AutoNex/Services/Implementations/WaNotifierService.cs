using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AutoNex.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutoNex.Services.Implementations;

public class WaNotifierService : IWaNotifierService
{
    private readonly HttpClient _httpClient;
    private readonly WaNotifierSettings _settings;
    private readonly ILogger<WaNotifierService> _logger;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public WaNotifierService(
        HttpClient httpClient,
        IOptions<WaNotifierSettings> settings,
        ILogger<WaNotifierService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<bool> SendWhatsAppAsync(string to, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var phone = new string(to.Where(char.IsDigit).ToArray());
            var token = await GetTokenAsync(cancellationToken).ConfigureAwait(false);

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/messages/send")
            {
                Content = JsonContent.Create(new { phone, message })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("WhatsApp message sent to {Phone} via wa-notifier", phone);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending WhatsApp via wa-notifier to {To}", to);
            return false;
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
            var token = await GetTokenAsync(cancellationToken).ConfigureAwait(false);

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
            var token = await GetTokenAsync(cancellationToken).ConfigureAwait(false);

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
            var token = await GetTokenAsync(cancellationToken).ConfigureAwait(false);

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

    private async Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry)
            return _cachedToken;

        await _tokenLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry)
                return _cachedToken;

            var response = await _httpClient.PostAsJsonAsync(
                "/api/auth/token",
                new { apiKey = _settings.ApiKey },
                cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            var result = await response.Content
                .ReadFromJsonAsync<TokenResponse>(cancellationToken)
                .ConfigureAwait(false);

            _cachedToken = result!.AccessToken;
            _tokenExpiry = DateTime.UtcNow.AddMinutes(55);

            _logger.LogDebug("wa-notifier JWT token obtained, expires at {Expiry}", _tokenExpiry);
            return _cachedToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to obtain wa-notifier JWT token");
            throw;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private sealed record TokenResponse([property: JsonPropertyName("accessToken")] string AccessToken);
    private sealed record QrResponse([property: JsonPropertyName("qr")] string? Qr);
}
