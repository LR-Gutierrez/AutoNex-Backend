namespace AutoNex.Services.Interfaces;

public interface ITwilioService
{
    Task<bool> SendWhatsAppAsync(string to, string message, CancellationToken cancellationToken = default);
    bool IsConfigured { get; }
}
