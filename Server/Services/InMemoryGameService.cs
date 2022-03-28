namespace Server.Services;

using System.Collections.Concurrent;
using Engine;
using Engine.Helpers;
using Engine.Models;

public class InMemoryGameService : IGameService
{
    private readonly ConcurrentDictionary<string, Room> rooms = new();


    public void JoinRoom(string roomId, string name, string connectionId)
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
            room.connections.TryAdd(connectionId, new Connection {name = name});
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

    public Result<List<ConnectedPlayer>> StartGame(string connectionId)
    {
        var groupId = GetConnectionsRoomId(connectionId);
        if (string.IsNullOrEmpty(groupId))
        {
            Result.Error("Connection not found in any room");
        }

        var room = rooms[groupId];
        if (room.connections.Count < GameEngine.PlayersPerGame)
        {
            Result.Error("Must have a minimum of two players to start the game");
        }

        if (room.game != null)
        {
            Result.Error("Game already exists");
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
                new ConnectedPlayer {connectionId = c.Key, playerId = c.Value.playerId})
            .ToList();

        return Result.Successful(connectedPlayers);
    }

    public string GetConnectionsRoomId(string connectionId) =>
        rooms.FirstOrDefault(g => g.Value.connections.ContainsKey(connectionId)).Key;

    public Result<GameStateDto> GetGameStateDto(string roomId)
    {
        // Check we have a room with that Id
        if (!rooms.ContainsKey(roomId))
        {
            Result.Error("Connection not found in any room");
        }

        // Check we have a game in the room
        var room = rooms[roomId];
        if (room.game == null)
        {
            Result.Error("No game in room yet");
        }

        return Result.Successful(new GameStateDto(room.game?.State!));
    }

    public class Room
    {
        public ConcurrentDictionary<string, Connection> connections;
        public WebGame? game;
    }

    public class Connection
    {
        public string name;
        public int playerId;
    }

    public struct ConnectedPlayer
    {
        public string connectionId;
        public int playerId;
    }
}

public class GameStateDto
{
    // todo
    public GameStateDto(GameState gameState)
    {
    }
}
