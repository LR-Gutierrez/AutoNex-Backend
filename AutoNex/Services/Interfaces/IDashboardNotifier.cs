namespace AutoNex.Services.Interfaces;

public interface IDashboardNotifier
{
    Task NotifyAllAsync(CancellationToken cancellationToken = default);
}
