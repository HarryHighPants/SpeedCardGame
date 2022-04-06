namespace Server;

using System.Collections.Concurrent;
using System.Linq;
using Engine;
using Engine.Helpers;
using Engine.Models;

public interface IGameService
{

	public void GetRoom(string roomId);

	public void JoinRoom(string roomId, string connectionId);

	public void LeaveRoom(string roomId, string connectionId);

	public void UpdateName(string updatedName, string connectionId);

	public Result StartGame(string connectionId, bool botGame, int botDifficulty);

	public Connection GetConnectionsPlayer(string connectionId);

	public string GetConnectionsRoomId(string connectionId);

	public Result TryPickupFromKitty(string connectionId);

	public Result TryRequestTopUp(string connectionId);

	public Result TryPlayCard(string connectionId, int cardId, int centerPilIndex);

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
	public ConcurrentDictionary<string, Connection> connections = new();
	public WebGame? game;
	public string roomId;
	public bool isBotGame; //todo see if I need this
	public int botDifficulty;
}

public class Connection
{
	public string connectionId;
	public string name;
	public int? playerId;
}

public class LobbyStateDto
{
	public List<Connection> connections;
	public bool isBotGame;
	public bool gameStarted;

	public LobbyStateDto(List<Connection> connections, bool isBotGame, bool gameStarted)
	{
		this.isBotGame = isBotGame;
		this.connections = connections;
		this.gameStarted = gameStarted;
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
			var connection = connections.FirstOrDefault(c => c.Value.playerId.ToString() == player.Id).Value;
			if (connection != null)
			{
				player.Id = connection.connectionId;
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
	public int TopKittyCardId { get; init; }
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
		TopKittyCardId = player.KittyCards.Last().Id;
		CanRequestTopUp = canRequestTopUp;
		LastMove = player.LastMove;
	}
}
