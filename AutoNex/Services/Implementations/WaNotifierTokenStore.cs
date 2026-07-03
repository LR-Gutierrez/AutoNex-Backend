using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutoNex.Services.Implementations;

public class WaNotifierTokenStore : IWaNotifierTokenStore
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WaNotifierSettings _settings;
    private readonly ILogger<WaNotifierTokenStore> _logger;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public WaNotifierTokenStore(
        IHttpClientFactory httpClientFactory,
        IOptions<WaNotifierSettings> settings,
        ILogger<WaNotifierTokenStore> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry)
            return _cachedToken;

        await _tokenLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry)
                return _cachedToken;

            var client = _httpClientFactory.CreateClient(nameof(WaNotifierTokenStore));
            client.BaseAddress = new Uri(_settings.BaseUrl);

            var response = await client.PostAsJsonAsync(
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
            _cachedToken = null;
            _tokenExpiry = DateTime.MinValue;
            _logger.LogError(ex, "Failed to obtain wa-notifier JWT token");
            throw;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private sealed record TokenResponse([property: JsonPropertyName("accessToken")] string AccessToken);
}
