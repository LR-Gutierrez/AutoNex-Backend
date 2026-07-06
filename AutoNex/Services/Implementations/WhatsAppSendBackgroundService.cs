using AutoNex.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace AutoNex.Services.Implementations;

public class WhatsAppSendBackgroundService : BackgroundService
{
    private readonly WhatsAppSendQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WhatsAppSendBackgroundService> _logger;

    public WhatsAppSendBackgroundService(
        WhatsAppSendQueue queue,
        IServiceProvider serviceProvider,
        ILogger<WhatsAppSendBackgroundService> logger)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
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

                await waNotifier.SendWhatsAppAsync(msg.Phone, msg.Message, msg.Source, msg.SentBy, msg.CorrelationId, msg.LogId, ct).ConfigureAwait(false);

                _logger.LogDebug("Message {MessageId} sent to wa-notifier — awaiting delivery callback", msg.Id);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al encolar envío de WhatsApp en segundo plano");
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
}