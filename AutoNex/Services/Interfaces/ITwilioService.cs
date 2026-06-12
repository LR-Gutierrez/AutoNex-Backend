namespace AutoNex.Services.Interfaces;

public interface ITwilioService
{
    Task<bool> SendWhatsAppAsync(string to, string message);
    bool IsConfigured { get; }
}
