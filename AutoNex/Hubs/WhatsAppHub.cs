using AutoNex.Data;
using AutoNex.Enums;
using AutoNex.Helpers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AutoNex.Hubs;

public class WhatsAppHub : Hub
{
    private const string WaNotifierSecretKey = "WaNotifierSecret";
    private readonly IServiceScopeFactory _scopeFactory;

    public WhatsAppHub(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var secret = httpContext?.Request.Query["secret"].FirstOrDefault();

        if (!string.IsNullOrEmpty(secret))
        {
            Context.Items[WaNotifierSecretKey] = secret;
        }
        else
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "whatsapp").ConfigureAwait(false);
        }

        await base.OnConnectedAsync().ConfigureAwait(false);
    }

    public async Task NotifyStatus(object status)
    {
        if (!IsWaNotifier()) return;
        await Clients.Others.SendAsync("StatusChanged", status);
    }

    public async Task NotifyQr(string? qr)
    {
        if (!IsWaNotifier()) return;
        await Clients.Others.SendAsync("QrChanged", new { qr });
    }

    public async Task NotifyDelivery(string? correlationId, bool success, string? error)
    {
        if (!IsWaNotifier()) return;
        await Clients.Others.SendAsync("MessageDelivery", new { correlationId, success, error });

        if (!int.TryParse(correlationId, out var notificationId)) return;

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var sp = scope.ServiceProvider;
            var context = sp.GetRequiredService<AppDbContext>();
            var notification = await context.Notifications
                .Include(n => n.Client)
                .Include(n => n.Vehicle)
                .FirstOrDefaultAsync(n => n.Id == notificationId);
            if (notification is not null && notification.Status == NotificationStatus.Pending)
            {
                notification.Status = success ? NotificationStatus.Sent : NotificationStatus.Failed;
                notification.SentAt = success ? DateTime.UtcNow : null;
                await context.SaveChangesAsync();

                var notificationsHub = sp.GetRequiredService<IHubContext<NotificationsHub>>();
                await notificationsHub.Clients.Group("all").SendAsync("newNotification", notification.ToResponse());
            }
        }
        catch (Exception ex)
        {
            var logger = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ILogger<WhatsAppHub>>();
            logger.LogError(ex, "Error updating notification delivery status for correlation {CorrelationId}", correlationId);
        }
    }

    private bool IsWaNotifier()
    {
        var config = Context.GetHttpContext()?.RequestServices
            .GetRequiredService<Microsoft.Extensions.Options.IOptions<AutoNex.Services.WaNotifierSettings>>()
            .Value;

        if (config?.WebSocketSecret is null) return true; // no secret configured = allow all

        var secret = Context.Items[WaNotifierSecretKey] as string;
        return secret == config.WebSocketSecret;
    }
}
