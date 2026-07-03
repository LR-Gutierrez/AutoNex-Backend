using AutoNex.Hubs;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AutoNex.Services.Implementations;

public class WhatsAppSendBackgroundService : BackgroundService
{
    private readonly WhatsAppSendQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<WhatsAppHub> _hubContext;
    private readonly ILogger<WhatsAppSendBackgroundService> _logger;

    public WhatsAppSendBackgroundService(
        WhatsAppSendQueue queue,
        IServiceProvider serviceProvider,
        IHubContext<WhatsAppHub> hubContext,
        ILogger<WhatsAppSendBackgroundService> logger)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("WhatsAppSendBackgroundService iniciado");

        var messageTask = ProcessMessagesAsync(stoppingToken);
        var workTask = ProcessWorkAsync(stoppingToken);

        await Task.WhenAll(messageTask, workTask).ConfigureAwait(false);
    }

    private async Task ProcessMessagesAsync(CancellationToken ct)
    {
        await foreach (var msg in _queue.DequeueMessagesAsync(ct))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var waNotifier = scope.ServiceProvider.GetRequiredService<IWaNotifierService>();

                var success = await waNotifier.SendWhatsAppAsync(msg.Phone, msg.Message, msg.Source, msg.SentBy, null, ct).ConfigureAwait(false);

                await NotifyAsync(msg.Id, msg.Phone, success, null, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar envío de WhatsApp en segundo plano");

                await NotifyAsync(msg.Id, msg.Phone, false, ex.Message, ct).ConfigureAwait(false);
            }
        }
    }

    private async Task ProcessWorkAsync(CancellationToken ct)
    {
        await foreach (var work in _queue.DequeueWorkAsync(ct))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                await work(scope.ServiceProvider, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar trabajo en segundo plano de WhatsApp");
            }
        }
    }

    private async Task NotifyAsync(string messageId, string phone, bool success, string? error, CancellationToken ct)
    {
        try
        {
            await _hubContext.Clients
                .Group("whatsapp")
                .SendAsync("MessageSent", new
                {
                    messageId,
                    success,
                    phone,
                    error,
                }, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error notificando resultado de envío por SignalR");
        }
    }
}