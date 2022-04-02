namespace Server;

using System.Collections.Concurrent;
using System.Linq;
using Engine.Helpers;
using Engine.Models;

public interface IGameService
{
    public void JoinRoom(string roomId, string connectionId);

    public void LeaveRoom(string roomId, string connectionId);

    public void UpdateName(string updatedName, string connectionId);

    public Result<List<Connection>> StartGame(string connectionId);

    public Connection GetConnectionsPlayer(string connectionId);

    public string GetConnectionsRoomId(string connectionId);

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
    public LobbyStateDto(List<Connection> connections) => this.connections = connections;
}

public class GameStateDto
{
    public List<PlayerDto> Players;
    public List<Card> CenterPiles;
    public string lastMove;

    public GameStateDto(GameState gameState, ConcurrentDictionary<string, Connection> connections)
    {
        Players = gameState.Players.Select(p => new PlayerDto(p)).ToList();
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
        lastMove = gameState.LastMove;
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

    public PlayerDto(Player player)
    {
        Id = player.Id.ToString();
        Name = player.Name;
        HandCards = player.HandCards.ToList();
        KittyCardsCount = player.KittyCards.Count;
        RequestingTopUp = player.RequestingTopUp;
    }
}
