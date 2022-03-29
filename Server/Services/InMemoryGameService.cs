namespace Server.Services;

using System.Collections.Concurrent;
using Engine;
using Engine.Helpers;
using Engine.Models;

public class InMemoryGameService : IGameService
{
    private static readonly ConcurrentDictionary<string, Room> rooms = new();

    public InMemoryGameService()
    {

    }

    public void JoinRoom(string roomId, string connectionId)
    {
        // Create the room if we don't have one yet
        if (!rooms.ContainsKey(roomId))
        {
            rooms.TryAdd(roomId, new Room());
        }

        // Add the connection to the room
        var room = rooms[roomId];
        if (!room.connections.ContainsKey(connectionId))
        {
            room.connections.TryAdd(connectionId, new Connection {connectionId = connectionId, name = "Player"});
        }
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
        if (room.connections.ContainsKey(connectionId))
        {
            room.connections.Remove(connectionId, out _);
        }
    }

    public void UpdateName(string updatedName, string connectionId) =>
        GetConnectionsPlayer(connectionId).name = updatedName;

    public Result<List<Connection>> StartGame(string connectionId)
    {
        var groupId = GetConnectionsRoomId(connectionId);
        if (string.IsNullOrEmpty(groupId))
        {
            return Result.Error<List<Connection>>("Connection not found in any room");
        }

        var room = rooms[groupId];
        if (room.connections.Count < GameEngine.PlayersPerGame)
        {
            return Result.Error<List<Connection>>("Must have a minimum of two players to start the game");
        }

        if (room.game != null)
        {
            return Result.Error<List<Connection>>("Game already exists");
        }

        // Get the players for the game
        var players = room.connections.Take(GameEngine.PlayersPerGame).ToList();

        // Create the game with the players names
        room.game = new WebGame(players.Select(p => p.Value.name).ToList(), new Settings());

        // Update the connections with their playerId
        for (var i = 0; i < players.Count; i++)
        {
            var player = players[i];
            room.connections[player.Key].playerId = room.game.State.Players[i].Id;
        }

        // Get the connections playerId
        var connectedPlayers = room.connections.Select(c =>
                new Connection {connectionId = c.Key, playerId = c.Value.playerId})
            .ToList();

        return Result.Successful(connectedPlayers);
    }

    public Connection GetConnectionsPlayer(string connectionId) =>
        rooms[GetConnectionsRoomId(connectionId)].connections[connectionId];

    public string GetConnectionsRoomId(string connectionId) =>
        rooms.FirstOrDefault(g => g.Value.connections.ContainsKey(connectionId)).Key;

    public Result<GameStateDto> GetGameStateDto(string roomId)
    {
        // Check we have a room with that Id
        if (!rooms.ContainsKey(roomId))
        {
            return Result.Error<GameStateDto>("Connection not found in any room");
        }

        // Check we have a game in the room
        var room = rooms[roomId];
        if (room?.game?.State == null)
        {
            return Result.Error<GameStateDto>("No game in room yet");
        }

        return Result.Successful(new GameStateDto(room.game.State, room.connections));
    }

    public Result<LobbyStateDto> GetLobbyStateDto(string roomId)
    {
        // Check we have a room with that Id
        if (!rooms.ContainsKey(roomId))
        {
            Result.Error("Connection not found in any room");
        }

        var room = rooms[roomId];
        var LobbyStateDto = new LobbyStateDto(room.connections.Values.ToList());
        return Result.Successful(LobbyStateDto);
    }

    public bool GameStarted(string roomId)
    {
        if (!rooms.ContainsKey(roomId))
        {
            return false;
        }

        return rooms[roomId].game != null;
    }
}


