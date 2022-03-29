namespace Server;

using System.Collections.Concurrent;
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
}

public class Room
{
    public ConcurrentDictionary<string, Connection> connections = new();
    public WebGame? game;
}

public class Connection
{
    public string connectionId;
    public string name;
    public int playerId;
}

public class LobbyStateDto
{
    public List<Connection> connections;

    public LobbyStateDto(List<Connection> connections) => this.connections = connections;
}

public class GameStateDto
{
    public List<PlayerDto> Players;
    public List<CenterPile> CenterPiles;
    public string lastMove;

    public GameStateDto(GameState gameState, ConcurrentDictionary<string, Connection> connections)
    {
        Players = gameState.Players.Select(p => new PlayerDto(p)).ToList();
        // Replace the playerId with the players connectionId
        foreach (var player in Players)
        {
            var connection = connections.FirstOrDefault(c => c.Value.playerId.ToString() == player.Id).Value;
            player.Id = connection.connectionId;
        }
        CenterPiles = gameState.CenterPiles.ToList();
        lastMove = gameState.LastMove;
    }
}

public record PlayerDto
{
    public string Id { get; set; }
    public string Name { get; init; } = "";
    public List<Card> HandCards { get; init; }
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
