namespace Server.Services;

using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using Engine;
using Engine.Helpers;
using Engine.Models;
using Models.Database;

public class InMemoryGameService : IGameService
{
	private readonly ConcurrentDictionary<string, Room> rooms = new();
	private readonly IServiceScopeFactory scopeFactory;

	public InMemoryGameService(IServiceScopeFactory scopeFactory)
	{
		this.scopeFactory = scopeFactory;
	}

	public static int GetDayIndex()
	{
		var eastern = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
			DateTime.UtcNow, "AUS Eastern Standard Time");
		return DateOnly.FromDateTime(eastern).DayNumber;
	}

	public void JoinRoom(string roomId, Guid persistentPlayerId, BotType? botType)
	{
		int? customGameSeed = botType == BotType.Daily ? GetDayIndex() : null;

		// Create the room if we don't have one yet
		if (!rooms.ContainsKey(roomId))
		{
			// If the game doesn't exist yet, check it's not a daily one that's already been played
			if (botType != null)
			{
				if (botType == BotType.Daily)
				{
					// todo: check if todays daily game has already been played by this player
					// throw new Exception("Daily game already played today");
				}
			}

			rooms.TryAdd(roomId, new Room {RoomId = roomId, CustomGameSeed = customGameSeed});
		}

		// Add the connection to the room
		var room = rooms[roomId];
		if (room.Connections.All(c => c.PersistentPlayerId != persistentPlayerId))
		{
			// See if the player has a rank
			using var scope = scopeFactory.CreateScope();
			var gameResultContext = scope.ServiceProvider.GetRequiredService<GameResultContext>();
			var playerDao = gameResultContext.Players.Find(persistentPlayerId);
			room.Connections.Add(
				new Connection
				{
					Name = playerDao?.Name ?? "Player",
					PersistentPlayerId = persistentPlayerId,
					Rank = EloService.GetRank(playerDao?.Elo ?? EloService.StartingElo)
				});
		}


		if (HasGameStarted(roomId))
		{
			UpdateRoomsPlayerIds(roomId);
		}
		else
		{
			if (botType != null)
			{
				if (botType == BotType.Daily)
				{
					var connection = GetConnectionInfo(persistentPlayerId);
					// See if we have already played the daily game
					if (await SendDailyResult(connection))
					{
						return;
					}
				}

				await botService.AddBotToRoom(roomId, botType);
			}
		}

	}

	public int ConnectionsInRoomCount(string roomId)
	{
		if (!rooms.ContainsKey(roomId))
		{
			return 0;
		}

		return rooms[roomId].Connections.Count;
	}

	public List<Connection> ConnectionsInRoom(string roomId)
	{
		if (!rooms.ContainsKey(roomId))
		{
			return new List<Connection>();
		}

		return rooms[roomId].Connections;
	}

	public void LeaveRoom(string roomId, Guid persistentPlayerId)
	{
		// Check we have a room with that Id
		if (!rooms.ContainsKey(roomId))
		{
			return;
		}

		// Remove the connection to the room
		var room = rooms[roomId];
		room.Connections.RemoveAll(c=>c.ConnectionId == connectionId);

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


		if (room.Connections.Count <= 0)
		{
			rooms.TryRemove(roomId, out _);
		}
		else
		{
			UpdateRoomsPlayerIds(roomId);
		}
	}

	public void UpdateName(string updatedName, Guid persistentPlayerId) =>
		GetConnectionInfo(connectionId).Name = updatedName;

	public Result StartGame(string roomId, Guid persistentPlayerId)
	{
		var roomResult = GetConnectionsRoom(connectionId);
		if (roomResult is IErrorResult roomError)
		{
			return Result.Error(roomError.Message);
		}

		var room = roomResult.Data;

		if (room.Connections.Count < GameEngine.PlayersPerGame)
		{
			return Result.Error("Must have a minimum of two players to start the game");
		}

		if (room.Game != null)
		{
			return Result.Error("Game already exists");
		}

		var connectionsPlaying = room.Connections.Take(GameEngine.PlayersPerGame).ToList();
		// Create the game with the players
		room.Game = new WebGame(connectionsPlaying, new Settings{RandomSeed = room.CustomGameSeed});

		// Update the connections with their playerId
		for (var i = 0; i < connectionsPlaying.Count; i++)
		{
			var connection = connectionsPlaying[i];
			room.Connections.Single(c=>c.ConnectionId == connection.ConnectionId).PlayerId = room.Game.State.Players[i].Id;
		}

		botService.RunBotsInRoom(roomId);


		return Result.Successful();
	}

	public Result TryPlayCard(string roomId, Guid persistentPlayerId, int cardId, int centerPilIndex)
	{
		var gameResult = GetConnectionsGame(connectionId);
		if (gameResult is IErrorResult gameResultError)
		{
			return Result.Error(gameResultError.Message);
		}

		var game = gameResult.Data;

		// Check if the game has just ended
		var updatedGameState = gameService.GetGameStateDto(roomId);
		if (updatedGameState.Success && updatedGameState.Data.WinnerId != null)
		{
			var a = 1;
		}
		if (initialGameState.Success && initialGameState.Data.WinnerId == null && updatedGameState.Success &&
		    updatedGameState.Data.WinnerId != null)
		{
			await HandleGameOver(roomId);
		}

		return game.TryPlayCard(GetConnectionInfo(connectionId).PlayerId.Value, cardId, centerPilIndex);
	}

	public Result TryPickupFromKitty(string roomId, Guid persistentPlayerId)
	{
		var gameResult = GetConnectionsGame(connectionId);
		if (gameResult is IErrorResult gameResultError)
		{
			return Result.Error(gameResultError.Message);
		}

		var game = gameResult.Data;

		return game.TryPickupFromKitty(GetConnectionInfo(connectionId).PlayerId.Value);
	}

	public Result TryRequestTopUp(string roomId, Guid persistentPlayerId)
	{
		var gameResult = GetConnectionsGame(connectionId);
		if (gameResult is IErrorResult gameResultError)
		{
			return Result.Error(gameResultError.Message);
		}

		var game = gameResult.Data;

		return game.TryRequestTopUp(GetConnectionInfo(connectionId).PlayerId.Value);
	}

	private async Task HandleGameOver(string roomId)
	{
		Console.WriteLine($"{roomId} has ended");

		await statService.HandleGameOver(roomId);

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

	public WebGame GetGame(string roomId)
	{
		return rooms[roomId].Game;
	}

	private Result<Room> GetConnectionsRoom(Guid persistentPlayerId)
	{
		var roomId = GetConnectionsRoomId(connectionId);
		if (string.IsNullOrEmpty(roomId))
		{
			return Result.Error<Room>("Connection not found in any room");
		}

		return Result.Successful(rooms[roomId]);
	}

	private Result<WebGame> GetConnectionsGame(Guid persistentPlayerId)
	{
		var roomResult = GetConnectionsRoom(connectionId);
		if (roomResult is IErrorResult roomError)
		{
			return Result.Error<WebGame>(roomError.Message);
		}

		var room = roomResult.Data;

		if (!HasGameStarted(room.RoomId))
		{
			return Result.Error<WebGame>("Game hasn't started yet");
		}

		return Result.Successful(room.Game!);
	}

	public Connection GetConnectionInfo(Guid persistentPlayerId) =>
		rooms[GetConnectionsRoomId(connectionId)].Connections.Single(c=>c.ConnectionId == connectionId);

	public string GetConnectionsRoomId(Guid persistentPlayerId) =>
		rooms.SingleOrDefault(room => room.Value.Connections.Any(c=>c.ConnectionId == connectionId)).Key;

	public string GetPlayersRoomId(Guid persistentPlayerId) =>
		rooms.SingleOrDefault(room => room.Value.Connections.Any(c=>c.PersistentPlayerId == persistentPlayerId)).Key;

	public string GetPlayersConnectionId(Guid persistentPlayerId) =>
		rooms.SelectMany(room => room.Value.Connections)
			.Single(c => c.PersistentPlayerId == persistentPlayerId)
			.ConnectionId;


	public Result<GameStateDto> GetGameStateDto(string roomId)
	{
		// Check we have a room with that Id
		if (!rooms.ContainsKey(roomId))
		{
			return Result.Error<GameStateDto>("Room not found");
		}

		// Check we have a game in the room
		var room = rooms[roomId];
		if (!HasGameStarted(roomId))
		{
			return Result.Error<GameStateDto>("No game in room yet");
		}

		return Result.Successful(new GameStateDto(room.Game.State, room.Connections, room.Game.gameEngine));
	}

	public Result<LobbyStateDto> GetLobbyStateDto(string roomId)
	{
		// Check we have a room with that Id
		if (!rooms.ContainsKey(roomId))
		{
			return Result.Error<LobbyStateDto>("Room not found");
		}

		var room = rooms[roomId];
		var LobbyStateDto = new LobbyStateDto(room.Connections.ToList(), room.IsBotGame, room.Game != null);
		return Result.Successful(LobbyStateDto);
	}

	public bool HasGameStarted(string roomId)
	{
		if (!rooms.ContainsKey(roomId))
		{
			return false;
		}

		return rooms[roomId].Game != null;
	}

	private void UpdateRoomsPlayerIds(string roomId)
	{
		var room = rooms[roomId];
		var assignedPlayers = room.Connections.Where(c => c.PlayerId != null).ToList();
		if (assignedPlayers.Count < GameEngine.PlayersPerGame && room.Connections.Count >= GameEngine.PlayersPerGame)
		{
			// At least 1 connection can be assigned to a player
			var assignedPlayerIds = assignedPlayers.Select(p => p.PlayerId!.Value).ToList();

			// Get a list of assignable playerIds
			var playerIdsToAssign = Enumerable.Range(0, GameEngine.PlayersPerGame).ToList().Except(assignedPlayerIds)
				.ToList();

			var unassignedPlayers = room.Connections.Where(c => c.PlayerId == null).ToList();

			for (var i = 0; i < unassignedPlayers.Count; i++)
			{
				if (playerIdsToAssign.Count <= 0)
				{
					break;
				}

				room.Connections.Single(c=>c.ConnectionId == unassignedPlayers[i].ConnectionId).PlayerId = playerIdsToAssign.Pop();
			}
		}
	}

	public bool ConnectionOwnsCard(Guid persistentPlayerId, int cardId)
	{
		var gameResult = GetConnectionsGame(connectionId);
		if (gameResult is IErrorResult gameError)
		{
			return false;
		}

		var game = gameResult.Data;

		var connectionsPlayerId = GetConnectionInfo(connectionId).PlayerId;
		if (connectionsPlayerId == null)
		{
			return false;
		}

		var player = game.State.Players[connectionsPlayerId.Value];
		if (player.HandCards.Any(c => c.Id == cardId))
		{
			return true;
		}

		if (player.KittyCards.Count > 0 && player.KittyCards.Last().Id == cardId)
		{
			return true;
		}

		return false;
	}

	public CardLocation? GetCardLocation(Guid persistentPlayerId, int cardId)
	{
		var gameResult = GetConnectionsGame(connectionId);
		var card = gameResult.Data.State.GetCard(cardId);
		return card?.Location(gameResult.Data.State);
	}
}
