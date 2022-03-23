namespace Server.Hubs;

using Microsoft.AspNetCore.SignalR;

public class GameHub : Hub
{
    public override Task OnConnectedAsync() => base.OnConnectedAsync();

    public override Task OnDisconnectedAsync(Exception? exception) => base.OnDisconnectedAsync(exception);
}
