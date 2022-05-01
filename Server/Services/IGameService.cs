namespace Server;

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using Engine;
using Engine.Helpers;
using Engine.Models;
using Models.Database;

public interface IGameService
{
	public void JoinRoom(string roomId, string connectionId, Guid persistentPlayerId);
	public int ConnectionsInRoomCount(string roomId);
	public List<Connection> ConnectionsInRoom(string roomId);

	public void LeaveRoom(string roomId, string connectionId);

	public void UpdateName(string updatedName, string connectionId);

	public Result StartGame(string connectionId);

	public Connection GetConnectionsPlayer(string connectionId);

	public string GetConnectionsRoomId(string connectionId);

	public Result TryPickupFromKitty(string connectionId);

	public Result TryRequestTopUp(string connectionId);

	public Result TryPlayCard(string connectionId, int cardId, int centerPilIndex);

	public WebGame GetGame(string roomId);

	public Result<GameStateDto> GetGameStateDto(string roomId);

	public Result<LobbyStateDto> GetLobbyStateDto(string roomId);
	public bool GameStarted(string roomId);
	public bool ConnectionOwnsCard(string connectionId, int cardId);
	public CardLocation? GetCardLocation(string connectionId, int cardId);
}

public record UpdateMovingCardData
{
	public int CardId {get; set;}
	public Pos? Pos {get; set;}
	public int Location {get; set;}
	public int Index { get; set; }
}

public record Pos
{
	public float X {get; set;}
	public float Y {get; set;}
}

public class Room
{
	public ConcurrentDictionary<string, Connection> Connections = new();
	public WebGame? Game;
	public string RoomId;
	public bool IsBotGame;
}

public class Connection
{
	public Guid PersistentPlayerId;
	public string ConnectionId;
	public string Name;
	// GameEngine Player index
	public int? PlayerId;
	public Rank? Rank;
}

public class ConnectionDto
{
	public string ConnectionId;
	public string Name;
	public Rank Rank;

	public ConnectionDto(Connection connection)
	{
		ConnectionId = connection.ConnectionId;
		Name = connection.Name;
		Rank = connection.Rank ?? Rank.BabyCardShark;
	}
}

public enum Rank
{
	BabyCardShark,
	CardSlinger,
	Acetronaut,
	Speedster,
	SpeedDemon
}

public class DailyResultDto
{
	public string BotName;
	public bool PlayerWon;
	public int LostBy;

	public int DailyWinStreak;
	public int MaxDailyWinStreak;
	public int DailyWins;
	public int DailyLosses;
	public DailyResultDto(Guid playerId, GameResultDao game)
	{
		PlayerWon = game.Winner.Id == playerId;

		var player = PlayerWon ? game.Winner : game.Loser;
		LostBy = game.LostBy;
		DailyWinStreak = player.DailyWinStreak;
		MaxDailyWinStreak = player.MaxDailyWinStreak;
		DailyWins = player.DailyWins;
		DailyLosses = player.DailyLosses;

		var bot = PlayerWon ? game.Loser : game.Winner;
		BotName = bot.Name;
	}
}

public class LobbyStateDto
{
	public List<ConnectionDto> Connections;
	public bool IsBotGame;
	public bool GameStarted;

	public LobbyStateDto(List<Connection> connections, bool isBotGame, bool gameStarted)
	{
		this.IsBotGame = isBotGame;
		this.Connections = connections.Select(c=>new ConnectionDto(c)).ToList();
		this.GameStarted = gameStarted;
	}
}

public class GameStateDto
{
	public List<PlayerDto> Players;
	public List<CenterPile> CenterPiles;
	public string LastMove;
	public string? WinnerId;
	public bool MustTopUp;

	public GameStateDto(GameState gameState, ConcurrentDictionary<string, Connection> connections, GameEngine gameEngine)
	{
		Players = gameState.Players
			.Select((p, i) => new PlayerDto(p, gameEngine.Checks.CanRequestTopUp(gameState, i).Success))
			.ToList();

		MustTopUp = Players.All(p => p.CanRequestTopUp || p.RequestingTopUp);


		// Replace the playerId with the players connectionId
		foreach (var player in Players)
		{
			var connection = connections.FirstOrDefault(c => c.Value.PlayerId.ToString() == player.Id).Value;
			if (connection != null)
			{
				player.Id = connection.ConnectionId;
			}
		}

		// Only send the top 3
		CenterPiles = gameState.CenterPiles.Select((pile, i) => new CenterPile{Cards = pile.Cards.TakeLast(3).ToImmutableList()} ).ToList();
		LastMove = gameState.LastMove;

		var winnerResult = gameEngine.Checks.TryGetWinner(gameState);
		WinnerId = gameEngine.Checks.TryGetWinner(gameState).Map(x =>
		{
			return connections.FirstOrDefault(c => c.Value.PlayerId == x).Value.ConnectionId;
		}, _ => null);
	}
}

public record PlayerDto
{
	public string Id { get; set; }
	public string Name { get; init; } = "";
	public List<Card> HandCards { get; init; }
	public int? TopKittyCardId { get; init; }
	public int KittyCardsCount { get; init; }
	public bool RequestingTopUp { get; init; }
	public bool CanRequestTopUp { get; init; }
	public string LastMove { get; init; } = "";

	public PlayerDto(Player player, bool canRequestTopUp)
	{
		Id = player.Id.ToString();
		Name = player.Name;
		HandCards = player.HandCards.ToList();
		KittyCardsCount = player.KittyCards.Count;
		RequestingTopUp = player.RequestingTopUp;
		TopKittyCardId = player.KittyCards.Count >= 1 ? player.KittyCards.Last().Id : null;
		CanRequestTopUp = canRequestTopUp;
		LastMove = player.LastMove;
	}
}
