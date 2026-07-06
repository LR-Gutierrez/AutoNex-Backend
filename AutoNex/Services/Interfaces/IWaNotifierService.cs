using AutoNex.Models;

namespace AutoNex.Services.Interfaces;

public record WaSendResult(bool Success, int LogId);

public interface IWaNotifierService
{
    Task<WaSendResult> SendWhatsAppAsync(string to, string message, string? source = null, string? sentBy = null, string? correlationId = null, int? logId = null, CancellationToken cancellationToken = default);
    Task<string?> GetQrAsync(CancellationToken cancellationToken = default);
    Task<object?> GetStatusAsync(CancellationToken cancellationToken = default);
    Task<bool> LogoutAsync(CancellationToken cancellationToken = default);
    Task<bool> RestartAsync(CancellationToken cancellationToken = default);
}
