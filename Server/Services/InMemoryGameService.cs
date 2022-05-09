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
    private readonly IBotService botService;
    private readonly StatService statService;
    private readonly GameEngine gameEngine;

    public InMemoryGameService(
        IServiceScopeFactory scopeFactory,
        IBotService botService,
        StatService statService,
        GameEngine gameEngine
    )
    {
        this.gameEngine = gameEngine;
        this.botService = botService;
        this.scopeFactory = scopeFactory;
        this.statService = statService;
    }

    public static int GetDayIndex()
    {
        var eastern = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
            DateTime.UtcNow,
            "AUS Eastern Standard Time"
        );
        return DateOnly.FromDateTime(eastern).DayNumber;
    }

    public async Task JoinRoom(string roomId, Guid persistentPlayerId, BotType? botType)
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

            rooms.TryAdd(roomId, new Room { RoomId = roomId, CustomGameSeed = customGameSeed });
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
                new GameParticipant
                {
                    Name = playerDao?.Name ?? "Player",
                    PersistentPlayerId = persistentPlayerId,
                    Rank = EloService.GetRank(playerDao?.Elo ?? EloService.StartingElo)
                }
            );
        }

        if (room.HasGameStarted)
        {
            UpdateRoomsPlayerIds(roomId);
        }
        else
        {
            if (botType != null)
            {
                await JoinRoom(roomId, BotConfigurations.GetBot(botType.Value).PersistentId, null);
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

    public List<GameParticipant> ConnectionsInRoom(string roomId)
    {
        if (!rooms.ContainsKey(roomId))
        {
            return new List<GameParticipant>();
        }

        return rooms[roomId].Connections;
    }

    public async Task LeaveRoom(string roomId, Guid persistentPlayerId)
    {
        // Check we have a room with that Id
        if (!rooms.ContainsKey(roomId))
        {
            return;
        }

        // Remove the connection to the room
        var room = rooms[roomId];
        room.Connections.RemoveAll(c => c.PersistentPlayerId == persistentPlayerId);
        room.StopBots?.Cancel();

        if (room.Connections.Count <= 0)
        {
            rooms.TryRemove(roomId, out _);
        }
        else
        {
            UpdateRoomsPlayerIds(roomId);
        }
    }

    public async Task UpdateName(string updatedName, Guid persistentPlayerId) =>
        GetConnectionInfo(persistentPlayerId).Name = updatedName;

    public async Task<Result> StartGame(string roomId, Guid persistentPlayerId)
    {
        var roomResult = GetConnectionsRoom(roomId, persistentPlayerId);
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
        room.Game = new WebGame(
            connectionsPlaying,
            new Settings { RandomSeed = room.CustomGameSeed },
            gameEngine
        );

        // Update the connections with their playerId
        for (var i = 0; i < connectionsPlaying.Count; i++)
        {
            var connection = connectionsPlaying[i];
            room.Connections.Single(
                c => c.PersistentPlayerId == connection.PersistentPlayerId
            ).PlayerIndex = room.Game.State.Players[i].Id;
        }

        room.StopBots = new CancellationTokenSource();

        if (room.IsBotGame)
        {
            var bot = BotConfigurations.GetBot(room.BotType.Value);
            await botService.RunBot(
                1,
                room.BotType.Value,
                new BotActionHandler(
                    () => GetGame(roomId).State,
                    (PlayCardRequest playCardRequest) =>
                        TryPlayCard(
                            roomId,
                            bot.PersistentId,
                            playCardRequest.CardId,
                            playCardRequest.CenterPilIndex
                        ),
                    () => TryPickupFromKitty(roomId, bot.PersistentId),
                    () => TryRequestTopUp(roomId, bot.PersistentId)
                ),
                room.StopBots.Token
            );
        }

        return Result.Successful();
    }

    public async Task<Result> TryPlayCard(
        string roomId,
        Guid persistentPlayerId,
        int cardId,
        int centerPilIndex
    )
    {
        var gameResult = GetConnectionsGame(roomId, persistentPlayerId);
        if (gameResult.Failure)
        {
            return gameResult;
        }

        var (room, game) = gameResult.Data;
        var playerId = GetConnectionInfo(persistentPlayerId).PlayerIndex;

        var initialGameState = new GameStateDto(game.State, room.Connections, game.gameEngine);

        var playCardResult = game.TryPlayCard(playerId, cardId, centerPilIndex);
        if (playCardResult.Failure)
        {
            return playCardResult;
        }

        // Check if the game has just ended
        var updatedGameState = new GameStateDto(
            playCardResult.Data,
            room.Connections,
            updatedGame.gameEngine
        );
        if (
            initialGameState.Success
            && initialGameState.Data.WinnerId == null
            && updatedGameState.Success
            && updatedGameState.Data.WinnerId != null
        )
        {
            await HandleGameOver(game.State, rooms[roomId]);
        }

        return Result.Successful();
    }

    public async Task<Result> TryPickupFromKitty(string roomId, Guid persistentPlayerId)
    {
        var gameResult = GetConnectionsGame(roomId, persistentPlayerId);
        if (gameResult.Failure)
        {
            return gameResult;
        }

        var game = gameResult.Data;

        return game.TryPickupFromKitty(GetConnectionInfo(persistentPlayerId).PlayerIndex);
    }

    public async Task<Result> TryRequestTopUp(string roomId, Guid persistentPlayerId)
    {
        var gameResult = GetConnectionsGame(roomId, persistentPlayerId);
        if (gameResult.Failure)
        {
            return gameResult;
        }

        var game = gameResult.Data;

        return game.TryRequestTopUp(GetConnectionInfo(persistentPlayerId).PlayerIndex);
    }

    private async Task HandleGameOver(GameState finalGameState, Room room)
    {
        Console.WriteLine($"{room.RoomId} has ended");
        var dailyGame = room.BotType == BotType.Daily;
        await statService.UpdateGameOverStats(finalGameState, room.Connections, dailyGame);
    }

    public WebGame GetGame(string roomId)
    {
        return rooms[roomId].Game;
    }

    private Result<Room> GetConnectionsRoom(string roomId, Guid persistentPlayerId)
    {
        if (string.IsNullOrEmpty(roomId))
        {
            return Result.Error<Room>("GameParticipant not found in any room");
        }

        var room = rooms[roomId];
        if (room.Connections.All(c => c.PersistentPlayerId != persistentPlayerId))
        {
            return Result.Error<Room>("Player not in room");
        }

        return Result.Successful(room);
    }

    private Result<(Room, WebGame)> GetConnectionsGame(string roomId, Guid persistentPlayerId)
    {
        var roomResult = GetConnectionsRoom(roomId, persistentPlayerId);
        if (roomResult is IErrorResult roomResultError)
        {
            return new ErrorResult<(Room, WebGame)>(roomResultError.Message);
        }

        if (!roomResult.Data.HasGameStarted)
        {
            return Result.Error<(Room, WebGame)>("Game hasn't started yet");
        }

        return Result.Successful((roomResult.Data, roomResult.Data.Game));
    }

    public GameParticipant GetConnectionInfo(Guid persistentPlayerId) =>
        rooms[GetConnectionsRoomId(persistentPlayerId)].Connections.Single(
            c => c.PersistentPlayerId == persistentPlayerId
        );

    public string GetConnectionsRoomId(Guid persistentPlayerId) =>
        rooms.SingleOrDefault(
            room => room.Value.Connections.Any(c => c.PersistentPlayerId == persistentPlayerId)
        ).Key;

    public Result<LobbyStateDto> GetLobbyStateDto(string roomId)
    {
        // Check we have a room with that Id
        if (!rooms.ContainsKey(roomId))
        {
            return Result.Error<LobbyStateDto>("Room not found");
        }

        var room = rooms[roomId];
        var LobbyStateDto = new LobbyStateDto(
            room.Connections.ToList(),
            room.IsBotGame,
            room.Game != null
        );
        return Result.Successful(LobbyStateDto);
    }

    private void UpdateRoomsPlayerIds(string roomId)
    {
        var room = rooms[roomId];
        var assignedPlayers = room.Connections.Where(c => c.PlayerIndex != null).ToList();
        if (
            assignedPlayers.Count < GameEngine.PlayersPerGame
            && room.Connections.Count >= GameEngine.PlayersPerGame
        )
        {
            // At least 1 connection can be assigned to a player
            var assignedPlayerIds = assignedPlayers.Select(p => p.PlayerIndex!.Value).ToList();

            // Get a list of assignable playerIds
            var playerIdsToAssign = Enumerable
                .Range(0, GameEngine.PlayersPerGame)
                .ToList()
                .Except(assignedPlayerIds)
                .ToList();

            var unassignedPlayers = room.Connections.Where(c => c.PlayerIndex == null).ToList();

            for (var i = 0; i < unassignedPlayers.Count; i++)
            {
                if (playerIdsToAssign.Count <= 0)
                {
                    break;
                }

                room.Connections.Single(
                    c => c.ConnectionId == unassignedPlayers[i].ConnectionId
                ).PlayerIndex = playerIdsToAssign.Pop();
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

        var connectionsPlayerId = GetConnectionInfo(connectionId).PlayerIndex;
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
