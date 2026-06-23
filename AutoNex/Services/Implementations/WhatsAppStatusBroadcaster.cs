using System.Text.Json;
using AutoNex.Hubs;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AutoNex.Services.Implementations;

public class WhatsAppStatusBroadcaster : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<WhatsAppHub> _hubContext;
    private readonly ILogger<WhatsAppStatusBroadcaster> _logger;

    private string? _previousStatus;
    private string? _previousQr;

    public WhatsAppStatusBroadcaster(
        IServiceScopeFactory scopeFactory,
        IHubContext<WhatsAppHub> hubContext,
        ILogger<WhatsAppStatusBroadcaster> logger)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var waNotifier = scope.ServiceProvider.GetRequiredService<IWaNotifierService>();

                await CheckStatusAsync(waNotifier, stoppingToken).ConfigureAwait(false);
                await CheckQrAsync(waNotifier, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error polling wa-notifier status");
            }

            await Task.Delay(5000, stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task CheckStatusAsync(IWaNotifierService waNotifier, CancellationToken ct)
    {
        var status = await waNotifier.GetStatusAsync(ct).ConfigureAwait(false);

        string? currentStatus;
        object? payload;

        if (status is JsonElement json && json.TryGetProperty("status", out var statusProp))
        {
            currentStatus = statusProp.GetString();
            payload = status;
        }
        else
        {
            currentStatus = "disconnected";
            payload = new { ready = false, status = "disconnected" };
        }

        if (currentStatus == _previousStatus) return;

        _previousStatus = currentStatus;
        _logger.LogInformation("WhatsApp status changed to {Status}", currentStatus);

        await _hubContext.Clients
            .Group("whatsapp")
            .SendAsync("StatusChanged", payload, ct)
            .ConfigureAwait(false);
    }

    private async Task CheckQrAsync(IWaNotifierService waNotifier, CancellationToken ct)
    {
        var qr = await waNotifier.GetQrAsync(ct).ConfigureAwait(false);
        if (qr == _previousQr) return;

        _previousQr = qr;
        _logger.LogInformation("WhatsApp QR changed (available: {HasQr})", qr is not null);

        await _hubContext.Clients
            .Group("whatsapp")
            .SendAsync("QrChanged", new { qr }, ct)
            .ConfigureAwait(false);
    }
}
