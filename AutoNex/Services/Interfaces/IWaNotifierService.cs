namespace AutoNex.Services.Interfaces;

public interface IWaNotifierService
{
    Task<bool> SendWhatsAppAsync(string to, string message, CancellationToken cancellationToken = default);
    Task<string?> GetQrAsync(CancellationToken cancellationToken = default);
    Task<object?> GetStatusAsync(CancellationToken cancellationToken = default);
    Task<bool> LogoutAsync(CancellationToken cancellationToken = default);
    Task<bool> RestartAsync(CancellationToken cancellationToken = default);
}
