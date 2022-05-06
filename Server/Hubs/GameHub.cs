namespace Server.Hubs;

using Engine.Helpers;
using Engine.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Models.Database;
using Newtonsoft.Json;
using Services;

public class GameHub : Hub
{
	private readonly IGameService gameService;
	private readonly IHubContext<GameHub> hubContext;
	private readonly IBotService botService;
	private readonly GameResultContext gameResultContext;
	private readonly StatsService statsService;

	public GameHub(GameResultContext gameResultContext, IGameService gameService, IHubContext<GameHub> hubContext, StatsService statsService,
		IBotService botService)
	{
		this.gameResultContext = gameResultContext;
		this.gameService = gameService;
		this.hubContext = hubContext;
		this.botService = botService;
		this.statsService = statsService;
	}

	public static int GetDayIndex()
	{
		var eastern = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
			DateTime.UtcNow, "AUS Eastern Standard Time");
		return DateOnly.FromDateTime(eastern).DayNumber;
	}

	private string UserConnectionId => Context.ConnectionId;

	public override Task OnConnectedAsync() => base.OnConnectedAsync();

	public override async Task OnDisconnectedAsync(Exception? exception)
	{
		Console.WriteLine($"{UserConnectionId} has disconnected");
		var roomId = gameService.GetConnectionsRoomId(UserConnectionId);
		if (!string.IsNullOrEmpty(roomId))
		{
			await LeaveRoom(roomId, UserConnectionId);
		}

		Console.WriteLine($"{UserConnectionId} has disconnected");

		await base.OnDisconnectedAsync(exception);
	}


	public async Task JoinRoom(string roomId, Guid persistentPlayerId, bool botGame = false, BotType botType = 0)
	{
		// Remove the connection from any previous room
		var playersPreviousRoom = gameService.GetPlayersRoomId(persistentPlayerId);
		Console.WriteLine($"Join room, {UserConnectionId}  room: {roomId}");

		if (roomId == playersPreviousRoom)
		{
			Console.WriteLine($"Already in room, {UserConnectionId}  room: {roomId}");

			// get the previous connection to leave the room
			var existingConnectionToRoom = gameService.GetPlayersConnectionId(persistentPlayerId);
			Console.WriteLine($"previous connection leaving room, {existingConnectionToRoom}  room: {roomId}");
			await LeaveRoom(roomId, existingConnectionToRoom);
		}

		// Add connection to the group with roomId
		await Groups.AddToGroupAsync(UserConnectionId, roomId);

		// Add the player to the room
		gameService.JoinRoom(roomId, UserConnectionId, persistentPlayerId,
			botType == BotType.Daily ? GetDayIndex() : null);

		if (botGame)
		{
			if (botType == BotType.Daily)
			{
				var connection = gameService.GetConnectionInfo(UserConnectionId);
				// See if we have already played the daily game
				if (await SendDailyResult(connection))
				{
					return;
				}
			}

			await botService.AddBotToRoom(roomId, botType);
		}

		// Send gameState for roomId
		await SendGameState(roomId);
		await SendLobbyState(roomId);
	}

	public async Task UpdateName(string name)
	{
		gameService.UpdateName(name, UserConnectionId);
		await SendLobbyState(gameService.GetConnectionsRoomId(UserConnectionId));
	}

	public async Task LeaveRoom(string roomId, string connectionId)
	{
		// Remove connection to the group
		await Groups.RemoveFromGroupAsync(connectionId, roomId);

		// Remove the connection from the room
		gameService.LeaveRoom(roomId, connectionId);
		Console.WriteLine($"They were in room id: {roomId}");

		var connectionsInRoom = gameService.ConnectionsInRoomCount(roomId);
		var botsInRoom = botService.BotsInRoomCount(roomId);
		if (botsInRoom > 0 && botsInRoom == connectionsInRoom)
		{
			Console.WriteLine(
				$"RemoveBotsFromRoom, {botService.BotsInRoomCount(roomId)} connections: {gameService.ConnectionsInRoomCount(roomId)}");
			botService.RemoveBotsFromRoom(roomId);
			return;
		}

		Console.WriteLine(
			$"Bots, {botService.BotsInRoomCount(roomId)} connections: {gameService.ConnectionsInRoomCount(roomId)}");

		if (connectionsInRoom > 0)
		{
			// Send gameState for roomId
			await SendGameState(roomId);
			await SendLobbyState(roomId);
		}
	}

	public async Task StartGame()
	{
		var roomId = gameService.GetConnectionsRoomId(UserConnectionId);

		var startGameResult = gameService.StartGame(UserConnectionId);
		if (startGameResult is IErrorResult startGameError)
		{
			await SendConnectionMessage(startGameError.Message);
			throw new HubException(startGameError.Message, new UnauthorizedAccessException(startGameError.Message));
		}

		botService.RunBotsInRoom(roomId);
		await SendLobbyState(roomId);
		await SendGameState(roomId);
	}

	public async Task TryPlayCard(int cardId, int centerPileId)
	{
		var roomId = gameService.GetConnectionsRoomId(UserConnectionId);
		var initialGameState = gameService.GetGameStateDto(roomId);
		var tryPlayCardResult = gameService.TryPlayCard(UserConnectionId, cardId, centerPileId);
		await SendGameState(roomId);
		if (tryPlayCardResult is IErrorResult tryPlayCardResultError)
		{
			await SendConnectionMessage(tryPlayCardResultError.Message);
			throw new HubException(tryPlayCardResultError.Message,
				new UnauthorizedAccessException(tryPlayCardResultError.Message));
		}

		// Check if the game has just ended
		var updatedGameState = gameService.GetGameStateDto(roomId);
		if (initialGameState.Success && initialGameState.Data.WinnerId == null && updatedGameState.Success &&
		    updatedGameState.Data.WinnerId != null)
		{
			await HandleGameOver(roomId);
		}
	}

	public async Task TryPickupFromKitty()
	{
		var pickupResult = gameService.TryPickupFromKitty(UserConnectionId);
		await SendGameState(gameService.GetConnectionsRoomId(UserConnectionId));
		if (pickupResult is IErrorResult pickupResultError)
		{
			await SendConnectionMessage(pickupResultError.Message);
			throw new HubException(pickupResultError.Message,
				new UnauthorizedAccessException(pickupResultError.Message));
		}
	}

	public async Task TryRequestTopUp()
	{
		var topUpResult = gameService.TryRequestTopUp(UserConnectionId);
		await SendGameState(gameService.GetConnectionsRoomId(UserConnectionId));
		if (topUpResult is IErrorResult topUpResultError)
		{
			await SendConnectionMessage(topUpResultError.Message);
			throw new HubException(topUpResultError.Message, new UnauthorizedAccessException(topUpResultError.Message));
		}
	}

	public async Task UpdateMovingCard(UpdateMovingCardData updateMovingCard)
	{
		// Check connection owns the card
		var cardLocation = gameService.GetCardLocation(UserConnectionId, updateMovingCard.CardId);
		if (cardLocation == null)
		{
			await SendConnectionMessage("No card found with that Id");
			throw new HubException("No card found with that Id",
				new UnauthorizedAccessException("No card found with that Id"));
		}

		var authorisedUpdate = updateMovingCard;
		var authorised = gameService.ConnectionOwnsCard(UserConnectionId, updateMovingCard.CardId);
		if (!authorised)
		{
			authorisedUpdate = authorisedUpdate with {Pos = null};
		}

		authorisedUpdate.Location = (int)cardLocation.Value.PileName;
		authorisedUpdate.Index = cardLocation.Value.PileName == CardPileName.Hand
			? cardLocation.Value.PileIndex ?? -1
			: cardLocation.Value.CenterIndex ?? -1;

		// Send update to the others in the group
		var roomId = gameService.GetConnectionsRoomId(UserConnectionId);
		var jsonData = JsonConvert.SerializeObject(authorisedUpdate);
		await Clients.OthersInGroup(roomId).SendAsync("MovingCardUpdated", jsonData);
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

	private async Task HandleGameOver(string roomId)
	{
		Console.WriteLine($"{roomId} has ended");

		await statsService.HandleGameOver(roomId);

		var bot = botService.GetBotsInRoom(roomId).FirstOrDefault();
		var dailyGame = bot is {Data.Type: BotType.Daily};
		var players = gameService.ConnectionsInRoom(roomId);
		if (dailyGame)
		{
			var player = players.Single(p => p.PlayerId != null && p.ConnectionId != bot?.ConnectionId);
			await SendDailyResult(player);
		}

		foreach (var player in players)
		{
			await SendEloInfo(player, roomId);
		}
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

	private async Task<bool> SendDailyResult(Connection connection)
	{
		var dailyResult = statsService.GetDailyResultDto(connection.PersistentPlayerId);

		// Send the SendDailyResult to the player
		var jsonData = JsonConvert.SerializeObject(dailyResult);
		await Clients.Client(UserConnectionId).SendAsync("UpdateDailyResults", jsonData);
		return true;
	}

	private async Task SendEloInfo(Connection connection, string roomId)
	{
		var eloInfo = await statsService.GetRankingStats(connection.PersistentPlayerId, roomId);

		// Send the SendDailyResult to the player
		var jsonData = JsonConvert.SerializeObject(eloInfo);
		await Clients.Client(UserConnectionId).SendAsync("UpdateEloInfo", jsonData);
	}

	private async Task SendConnectionMessage(string message) =>
		await Clients.Client(UserConnectionId).SendAsync("Message", message);
}
