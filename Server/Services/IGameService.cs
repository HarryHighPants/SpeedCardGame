namespace Server;

using System.Collections.Concurrent;
using System.Linq;
using Engine;
using Engine.Helpers;
using Engine.Models;

public interface IGameService
{
	public void JoinRoom(string roomId, string connectionId);
	public int ConnectionsInRoomCount(string roomId);

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
}

public class UpdateMovingCardData
{
	public int CardId;
	public Pos? Pos;
}

public class Pos
{
	public float X;
	public float Y;
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
	public string ConnectionId;
	public string Name;
	public int? PlayerId;
}

public class LobbyStateDto
{
	public List<Connection> Connections;
	public bool IsBotGame;
	public bool GameStarted;

	public LobbyStateDto(List<Connection> connections, bool isBotGame, bool gameStarted)
	{
		this.IsBotGame = isBotGame;
		this.Connections = connections;
		this.GameStarted = gameStarted;
	}
}

public class GameStateDto
{
	public List<PlayerDto> Players;
	public List<Card> CenterPiles;
	public string LastMove;
	public int? WinnerId;

	public GameStateDto(GameState gameState, ConcurrentDictionary<string, Connection> connections, GameEngine gameEngine)
	{
		Players = gameState.Players
			.Select((p, i) => new PlayerDto(p, gameEngine.Checks.CanRequestTopUp(gameState, i).Success))
			.ToList();

		// Replace the playerId with the players connectionId
		foreach (var player in Players)
		{
			var connection = connections.FirstOrDefault(c => c.Value.PlayerId.ToString() == player.Id).Value;
			if (connection != null)
			{
				player.Id = connection.ConnectionId;
			}
		}

		CenterPiles = gameState.CenterPiles.Select((pile, i) => pile.Cards.Last()).ToList();
		LastMove = gameState.LastMove;

		var winnerResult = gameEngine.Checks.TryGetWinner(gameState);
		WinnerId = gameEngine.Checks.TryGetWinner(gameState).Map<int?>(x => x, _ => null);
		// WinnerId = winnerResult.Success ? winnerResult.Data : null;
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
