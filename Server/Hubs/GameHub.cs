using Microsoft.AspNetCore.SignalR;

namespace Server.Hubs;

public class GameHub : Hub
{
    public GameHub()
    {
        
    }

    public override Task OnConnectedAsync()
    {
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        return base.OnDisconnectedAsync(exception);
    }
}