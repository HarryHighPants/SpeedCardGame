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
    // todo
    public GameStateDto(GameState gameState)
    {
    }
}
