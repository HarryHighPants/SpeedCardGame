namespace Server.Services;

using Hubs;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

public class GameHubGameUpdateHandler : IGameUpdateHandler
{
    private readonly IHubContext<GameHub> hubContext;

    public GameHubGameUpdateHandler(IHubContext<GameHub> hubContext)
    {
        this.hubContext = hubContext;
    }

    public async Task HandleGameUpdate(string roomId, GameStateDto gameStateDto)
    {
        await hubContext.Clients.Group(roomId).SendAsync("UpdateGameState", gameStateDto);
    }

    public async Task HandleLobbyUpdate(string roomId, LobbyStateDto lobbyStateDto)
    {
        await hubContext.Clients.Group(roomId).SendAsync("UpdateLobbyState", lobbyStateDto);
    }
}
