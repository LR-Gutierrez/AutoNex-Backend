using Microsoft.AspNetCore.SignalR;

namespace AutoNex.Hubs;

public class WhatsAppHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "whatsapp").ConfigureAwait(false);
        await base.OnConnectedAsync().ConfigureAwait(false);
    }
}
