namespace Server.Hubs;

using Engine.Helpers;
using Microsoft.AspNetCore.SignalR;
using Services;

public class GameHub : Hub
{
    // Todo: update to interface
    private readonly InMemoryGameService gameService;


    public GameHub(InMemoryGameService gameService) => this.gameService = gameService;

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

    public async Task SendConnectionId(string connectionId) => await Clients.All.SendAsync("setClientMessage",
        "A connection with ID '" + connectionId + "' has just connected");

    public async Task JoinGame(string roomId, string name)
    {
        // Add connection to the group with roomId
        await Groups.AddToGroupAsync(UserConnectionId, roomId);

        // Add the player to the room
        gameService.JoinRoom(roomId, name, UserConnectionId);

        // Send gameState for roomId
        SendGameState(roomId);
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
        // Get the gameState from the gameService
        var gameStateResult = gameService.GetGameStateDto(roomId);

        // Check it's valid
        if (gameStateResult is IErrorResult gameStateError)
        {
            throw new Exception(gameStateError.Message);
        }

        // Send the gameState to the roomId
        Clients.Group(roomId).SendAsync("UpdateGameState", gameStateResult.Data);
    }
}
