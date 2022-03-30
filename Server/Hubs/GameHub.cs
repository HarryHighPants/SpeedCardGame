namespace Server.Hubs;

using Engine.Helpers;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Services;

public class GameHub : Hub
{
    // Todo: update to interface
    private readonly IGameService gameService;

    public GameHub(IGameService gameService)
    {
        this.gameService = gameService;
    }

    // private string UserIdentifier => Context.UserIdentifier!;
    private string UserConnectionId => Context.ConnectionId;

    public override Task OnConnectedAsync() => base.OnConnectedAsync();

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var roomId = gameService.GetConnectionsRoomId(UserConnectionId);
        if (!string.IsNullOrEmpty(roomId))
        {
            await LeaveRoom(roomId);
        }

        await base.OnDisconnectedAsync(exception);
    }


    public async Task JoinRoom(string roomId)
    {
        // Remove the connection from any previous room
        var previousRoom = gameService.GetConnectionsRoomId(UserConnectionId);
        if (roomId == previousRoom)
        {
            return;
        }
        if (!string.IsNullOrEmpty(previousRoom))
        {
            await LeaveRoom(roomId);
        }

        // Add connection to the group with roomId
        await Groups.AddToGroupAsync(UserConnectionId, roomId);

        // Add the player to the room
        gameService.JoinRoom(roomId, UserConnectionId);

        // Send gameState for roomId
        SendGameState(roomId);
        SendLobbyState(roomId);
    }

    public void UpdateName(string name)
    {
        gameService.UpdateName(name, UserConnectionId);
        SendLobbyState(gameService.GetConnectionsRoomId(UserConnectionId));
    }

    public async Task LeaveRoom(string roomId)
    {
        // Remove connection to the group
        await Groups.RemoveFromGroupAsync(UserConnectionId, roomId);

        // Remove the player to the room
        gameService.LeaveRoom(roomId, UserConnectionId);
    }

    public async Task StartGame()
    {
        var startGameResult = gameService.StartGame(UserConnectionId);
        if (startGameResult is IErrorResult startGameError)
        {
            throw new Exception(startGameError.Message);
        }

        // Send the connections their playerId
        foreach (var connectedPlayer in startGameResult.Data)
        {
            await Clients.Client(connectedPlayer.connectionId).SendAsync("UpdatePlayerId", connectedPlayer.playerId);
        }


        SendGameState(gameService.GetConnectionsRoomId(UserConnectionId));
    }

    public async Task TryPlayCard(int cardId, int centerPileId)
    {
        // todo
    }


    public void SendGameState(string roomId)
    {
        if (!gameService.GameStarted(roomId))
        {
            return;
        }

        // Get the gameState from the gameService
        var gameStateResult = gameService.GetGameStateDto(roomId);

        // Check it's valid
        if (gameStateResult is IErrorResult gameStateError)
        {
            throw new Exception(gameStateError.Message);
        }

        // Send the gameState to the roomId
        var jsonData = JsonConvert.SerializeObject(gameStateResult.Data);
        Clients.Group(roomId).SendAsync("UpdateGameState", jsonData);
    }

    public void SendLobbyState(string roomId)
    {
        // Get the gameState from the gameService
        var lobbyStateResult = gameService.GetLobbyStateDto(roomId);

        // Check it's valid
        if (lobbyStateResult is IErrorResult lobbyStateError)
        {
            throw new Exception(lobbyStateError.Message);
        }

        // Send the gameState to the roomId
        var jsonData = JsonConvert.SerializeObject(lobbyStateResult.Data);
        Clients.Group(roomId).SendAsync("UpdateLobbyState", jsonData);
    }
}
