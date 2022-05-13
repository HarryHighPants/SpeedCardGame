namespace Server.Services;

using Hubs;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

public class GameHubGameUpdateHandler : IGameUpdateHandler
{
    private readonly IHubContext<GameHub, IGameHubClient> hubContext;

    public GameHubGameUpdateHandler(IHubContext<GameHub, IGameHubClient> hubContext)
    {
        this.hubContext = hubContext;
    }

    public async Task HandleGameUpdate(string roomId, GameStateDto gameStateDto)
    {
        Console.WriteLine("Sending gamestate: " + gameStateDto.LastMove);
        await hubContext.Clients.Group(roomId).UpdateGameState(gameStateDto);
    }

    public async Task HandleLobbyUpdate(string roomId, LobbyStateDto lobbyStateDto)
    {
        await hubContext.Clients.Group(roomId).UpdateLobbyState(lobbyStateDto);
    }
}
