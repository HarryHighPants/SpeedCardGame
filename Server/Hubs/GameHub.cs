namespace Server.Hubs;

using System.Dynamic;
using Engine.Helpers;
using Engine.Models;
using Microsoft.AspNetCore.SignalR;
using Models.Database;
using Newtonsoft.Json;
using Services;
using Player = Engine.Models.Player;

public class GameHub : Hub
{
	private readonly IGameService gameService;
	private readonly IHubContext<GameHub> hubContext;
	private readonly IBotService botService;
	private readonly GameResultContext gameResultContext;

	public GameHub(GameResultContext gameResultContext, IGameService gameService, IHubContext<GameHub> hubContext,
		IBotService botService)
	{
		this.gameResultContext = gameResultContext;
		this.gameService = gameService;
		this.hubContext = hubContext;
		this.botService = botService;
	}

	// private string UserIdentifier => Context.UserIdentifier!;
	private string UserConnectionId => Context.ConnectionId;

	public override Task OnConnectedAsync() => base.OnConnectedAsync();

	public override async Task OnDisconnectedAsync(Exception? exception)
	{
		Console.WriteLine($"{UserConnectionId} has disconnected");
		var roomId = gameService.GetConnectionsRoomId(UserConnectionId);
		if (!string.IsNullOrEmpty(roomId))
		{
			await LeaveRoom(roomId);
		}

		Console.WriteLine($"{UserConnectionId} has disconnected");

		await base.OnDisconnectedAsync(exception);
	}


	public async Task JoinRoom(string roomId, Guid persistentPlayerId, bool botGame = false, BotType botType = 0)
	{
		// Remove the connection from any previous room
		var previousRoom = gameService.GetConnectionsRoomId(UserConnectionId);
		Console.WriteLine($"Join room, {UserConnectionId}  room: {roomId}");

		if (roomId == previousRoom)
		{
			Console.WriteLine($"Already in room, {UserConnectionId}  room: {roomId}");
			return;
		}

		if (!string.IsNullOrEmpty(previousRoom))
		{
			Console.WriteLine($"leaving old room, {UserConnectionId}  room: {previousRoom}");
			await LeaveRoom(roomId);
		}

		// Add connection to the group with roomId
		await Groups.AddToGroupAsync(UserConnectionId, roomId);

		// Add the player to the room
		gameService.JoinRoom(roomId, UserConnectionId, persistentPlayerId);

		if (botGame)
		{
			botService.AddBotToRoom(roomId, botType);
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

	public async Task LeaveRoom(string roomId)
	{
		// Remove connection to the group
		await Groups.RemoveFromGroupAsync(UserConnectionId, roomId);

		// Remove the connection from the room
		gameService.LeaveRoom(roomId, UserConnectionId);
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

		// Get the gameState from the gameService
		var gameStateResult = gameService.GetGameStateDto(roomId);

		// Check it's valid
		if (gameStateResult is IErrorResult gameStateError)
		{
			throw new HubException(gameStateError.Message, new ApplicationException(gameStateError.Message));
		}

		var bot = botService.GetBotsInRoom(roomId).FirstOrDefault();

		var gameState = gameService.GetGame(roomId).State;
		var players = gameService.ConnectionsInRoom(roomId);
		var winner = players.Single(p => p.PlayerId != null && p.ConnectionId == gameStateResult.Data.WinnerId);
		var loser = players.Single(p => p.PlayerId != null && p.ConnectionId != gameStateResult.Data.WinnerId);

		var dbWinner = await gameResultContext.Players.FindAsync(winner.PersistentPlayerId);
		if (dbWinner == null)
		{
			dbWinner = new PlayerDao
			{
				Id = winner.PersistentPlayerId,
				Name = winner.Name,
				DailyWins = 0,
				DailyLosses = 0,
				Elo = 500
			};
			gameResultContext.Players.Add(dbWinner);
			await gameResultContext.SaveChangesAsync();
		}

		dbWinner.Name = winner.Name;

		var dbLoser = await gameResultContext.Players.FindAsync(loser.PersistentPlayerId);
		if (dbLoser == null)
		{
			dbLoser = new PlayerDao {Id = loser.PersistentPlayerId, Name = loser.Name, Elo = 500};
			gameResultContext.Players.Add(dbLoser);
			await gameResultContext.SaveChangesAsync();
		}

		dbLoser.Name = loser.Name;

		if (bot is {Data.Type: BotType.Daily})
		{
			dbWinner.DailyWins += 1;
		}

		var eloDifference = (int)(Math.Abs(dbLoser.Elo - dbWinner.Elo) * 0.3);
		var eloWinnerPoints = (int)(1 / Math.Log(eloDifference) * 150);
		if (dbWinner.Elo < dbLoser.Elo)
		{
			dbWinner.Elo += eloDifference;
			dbLoser.Elo -= eloDifference;
		}

		dbWinner.Elo += eloWinnerPoints;
		dbWinner.Elo -= eloWinnerPoints;

		gameResultContext.GameResults.Add(new GameResultDao
		{
			Id = Guid.NewGuid(),
			Turns = gameState.MoveHistory.Count,
			LostBy = 1,
			Winner = dbWinner,
			Loser = dbLoser
		});

		await gameResultContext.SaveChangesAsync();
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

	private async Task SendConnectionMessage(string message) =>
		await Clients.Client(UserConnectionId).SendAsync("Message", message);
}
