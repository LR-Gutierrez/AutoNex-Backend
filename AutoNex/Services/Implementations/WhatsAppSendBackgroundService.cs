using AutoNex.Services.Interfaces;

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

        await foreach (var work in _queue.DequeueAsync(stoppingToken))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                await work(scope.ServiceProvider, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar envío de WhatsApp en segundo plano");
            }
        }
    }
}
