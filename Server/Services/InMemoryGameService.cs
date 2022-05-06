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

	public void JoinRoom(string roomId, string connectionId, Guid persistentPlayerId, int? customGameSeed = null)
	{
		// Create the room if we don't have one yet
		if (!rooms.ContainsKey(roomId))
		{
			rooms.TryAdd(roomId, new Room {RoomId = roomId, CustomGameSeed = customGameSeed});
		}

		// Add the connection to the room
		var room = rooms[roomId];
		if (!room.Connections.Any(c=>c.ConnectionId == connectionId))
		{
			// See if the player has a rank
			using var scope = scopeFactory.CreateScope();
			var gameResultContext = scope.ServiceProvider.GetRequiredService<GameResultContext>();
			var playerDao = gameResultContext.Players.Find(persistentPlayerId);
			room.Connections.Add(
				new Connection
				{
					ConnectionId = connectionId,
					Name = "Player",
					PersistentPlayerId = persistentPlayerId,
					Rank = GetRank(playerDao?.Elo ?? 500)
				});
		}

		if (GameStarted(roomId))
		{
			UpdateRoomsPlayerIds(roomId);
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

	public void LeaveRoom(string roomId, string connectionId)
	{
		// Check we have a room with that Id
		if (!rooms.ContainsKey(roomId))
		{
			return;
		}

		// Remove the connection to the room
		var room = rooms[roomId];
		room.Connections.RemoveAll(c=>c.ConnectionId == connectionId);

		if (room.Connections.Count <= 0)
		{
			rooms.TryRemove(roomId, out _);
		}
		else
		{
			UpdateRoomsPlayerIds(roomId);
		}
	}

	public void UpdateName(string updatedName, string connectionId) =>
		GetConnectionInfo(connectionId).Name = updatedName;

	public Result StartGame(string connectionId)
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

		return Result.Successful();
	}

	public Result TryPlayCard(string connectionId, int cardId, int centerPilIndex)
	{
		var gameResult = GetConnectionsGame(connectionId);
		if (gameResult is IErrorResult gameResultError)
		{
			return Result.Error(gameResultError.Message);
		}

		var game = gameResult.Data;

		return game.TryPlayCard(GetConnectionInfo(connectionId).PlayerId.Value, cardId, centerPilIndex);
	}

	public Result TryPickupFromKitty(string connectionId)
	{
		var gameResult = GetConnectionsGame(connectionId);
		if (gameResult is IErrorResult gameResultError)
		{
			return Result.Error(gameResultError.Message);
		}

		var game = gameResult.Data;

		return game.TryPickupFromKitty(GetConnectionInfo(connectionId).PlayerId.Value);
	}

	public Result TryRequestTopUp(string connectionId)
	{
		var gameResult = GetConnectionsGame(connectionId);
		if (gameResult is IErrorResult gameResultError)
		{
			return Result.Error(gameResultError.Message);
		}

		var game = gameResult.Data;

		return game.TryRequestTopUp(GetConnectionInfo(connectionId).PlayerId.Value);
	}

	public WebGame GetGame(string roomId)
	{
		return rooms[roomId].Game;
	}

	private Result<Room> GetConnectionsRoom(string connectionId)
	{
		var roomId = GetConnectionsRoomId(connectionId);
		if (string.IsNullOrEmpty(roomId))
		{
			return Result.Error<Room>("Connection not found in any room");
		}

		return Result.Successful(rooms[roomId]);
	}

	private Result<WebGame> GetConnectionsGame(string connectionId)
	{
		var roomResult = GetConnectionsRoom(connectionId);
		if (roomResult is IErrorResult roomError)
		{
			return Result.Error<WebGame>(roomError.Message);
		}

		var room = roomResult.Data;

		if (!GameStarted(room.RoomId))
		{
			return Result.Error<WebGame>("Game hasn't started yet");
		}

		return Result.Successful(room.Game!);
	}

	public Connection GetConnectionInfo(string connectionId) =>
		rooms[GetConnectionsRoomId(connectionId)].Connections.Single(c=>c.ConnectionId == connectionId);

	public string GetConnectionsRoomId(string connectionId) =>
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
		if (!GameStarted(roomId))
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

	public bool GameStarted(string roomId)
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

	public bool ConnectionOwnsCard(string connectionId, int cardId)
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

	public CardLocation? GetCardLocation(string connectionId, int cardId)
	{
		var gameResult = GetConnectionsGame(connectionId);
		var card = gameResult.Data.State.GetCard(cardId);
		return card?.Location(gameResult.Data.State);
	}

	public static Rank GetRank(int elo) =>
		elo switch
		{
			<= 500 => Rank.EagerStarter,
			<= 1000 => Rank.DedicatedRival,
			<= 1500 => Rank.CertifiedRacer,
			<= 2000 => Rank.BossAthlete,
			<= 2500 => Rank.Acetronaut,
			> 2500 => Rank.SpeedDemon
		};
}
