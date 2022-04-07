namespace Server.Hubs;

using Engine.Helpers;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Services;

public class GameHub : Hub
{
    private readonly IGameService gameService;
    private readonly IHubContext<GameHub> hubContext;
    private readonly IBotService botService;

    public GameHub(IGameService gameService, IHubContext<GameHub> hubContext, IBotService botService)
    {
        this.gameService = gameService;
        this.hubContext = hubContext;
        this.botService = botService;
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
        await SendGameState(roomId);
        await SendLobbyState(roomId);
    }

    public async Task UpdateName(string name)
    {
        gameService.UpdateName(name, UserConnectionId);
        await SendLobbyState(gameService.GetConnectionsRoomId(UserConnectionId));
    }

    public async Task LeaveRoom(string roomId)
    {
        // Remove connection to the group
        await Groups.RemoveFromGroupAsync(UserConnectionId, roomId);

        // Remove the connection from the room
        gameService.LeaveRoom(roomId, UserConnectionId);

        if (botService.BotsInRoomCount(roomId) == gameService.ConnectionsInRoomCount(roomId))
        {
	        botService.RemoveBotsFromRoom(roomId);
        }

        // Send gameState for roomId
        await SendGameState(roomId);
        await SendLobbyState(roomId);
    }

    public async Task StartGame(bool botGame = false, BotDifficulty botDifficulty = 0)
    {
	    var roomId = gameService.GetConnectionsRoomId(UserConnectionId);

	    if (botGame)
	    {
		    botService.AddBotToRoom(roomId, botDifficulty);
	    }

        var startGameResult = gameService.StartGame(UserConnectionId);
        if (startGameResult is IErrorResult startGameError)
        {
            throw new HubException(startGameError.Message, new UnauthorizedAccessException(startGameError.Message));
        }

        botService.RunBotsInRoom(roomId);
        await SendGameState(roomId);
    }

    public async Task TryPlayCard(int cardId, int centerPileId)
    {
        var tryPlayCardResult = gameService.TryPlayCard(UserConnectionId, cardId, centerPileId);
        await SendGameState(gameService.GetConnectionsRoomId(UserConnectionId));
        if (tryPlayCardResult is IErrorResult tryPlayCardResultError)
        {
	        throw new HubException(tryPlayCardResultError.Message, new UnauthorizedAccessException(tryPlayCardResultError.Message));
        }
    }

    public async Task TryPickupFromKitty()
    {
	    var pickupResult = gameService.TryPickupFromKitty(UserConnectionId);
	    await SendGameState(gameService.GetConnectionsRoomId(UserConnectionId));
	    if (pickupResult is IErrorResult pickupResultError)
	    {
		    throw new HubException(pickupResultError.Message, new UnauthorizedAccessException(pickupResultError.Message));
	    }
    }

    public async Task TryRequestTopUp()
    {
	    var topUpResult = gameService.TryRequestTopUp(UserConnectionId);
	    await SendGameState(gameService.GetConnectionsRoomId(UserConnectionId));
	    if (topUpResult is IErrorResult topUpResultError)
	    {
		    throw new HubException(topUpResultError.Message, new UnauthorizedAccessException(topUpResultError.Message));
	    }
    }

    public async Task UpdateMovingCard(UpdateMovingCardData updateMovingCard)
    {
        // Check connection owns the card
        if (!gameService.ConnectionOwnsCard(UserConnectionId, updateMovingCard.CardId))
        {
            throw new HubException("Connection can't move that card", new UnauthorizedAccessException());
        }

        // Send update to the group
        var roomId = gameService.GetConnectionsRoomId(UserConnectionId);
        var jsonData = JsonConvert.SerializeObject(updateMovingCard);
        await Clients.Group(roomId).SendAsync("CardMoved", jsonData);
    }


    private async Task SendGameState(string roomId)
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
            throw new HubException(gameStateError.Message, new ApplicationException(gameStateError.Message));
        }

        // Send the gameState to the roomId
        var jsonData = JsonConvert.SerializeObject(gameStateResult.Data);
        await hubContext.Clients.Group(roomId).SendAsync("UpdateGameState", jsonData);
    }

    private async Task SendLobbyState(string roomId)
    {
        // Get the gameState from the gameService
        var lobbyStateResult = gameService.GetLobbyStateDto(roomId);

        // Check it's valid
        if (lobbyStateResult is IErrorResult lobbyStateError)
        {
            throw new HubException(lobbyStateError.Message, new ApplicationException(lobbyStateError.Message));
        }

        // Send the gameState to the roomId
        var jsonData = JsonConvert.SerializeObject(lobbyStateResult.Data);
        await Clients.Group(roomId).SendAsync("UpdateLobbyState", jsonData);
    }
}
