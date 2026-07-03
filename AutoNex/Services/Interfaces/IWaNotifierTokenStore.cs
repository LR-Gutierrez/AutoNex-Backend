namespace AutoNex.Services.Interfaces;

public interface IWaNotifierTokenStore
{
    Task<string> GetTokenAsync(CancellationToken cancellationToken = default);
}
