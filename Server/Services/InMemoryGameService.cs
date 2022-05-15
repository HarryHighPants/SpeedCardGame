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
    private readonly IGameUpdateHandler gameUpdateHandler;
    private readonly IBotService botService;
    private readonly StatService statService;
    private readonly GameEngine gameEngine;

    public InMemoryGameService(
        IServiceScopeFactory scopeFactory,
        IGameUpdateHandler gameUpdateHandler,
        IBotService botService,
        StatService statService,
        GameEngine gameEngine
    )
    {
        this.gameEngine = gameEngine;
        this.botService = botService;
        this.scopeFactory = scopeFactory;
        this.gameUpdateHandler = gameUpdateHandler;
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

    public async Task<Result> JoinRoom(string roomId, Guid persistentPlayerId, BotType? botType)
    {
        int? customGameSeed = botType == BotType.Daily ? GetDayIndex() : null;

        // Create the room if we don't have one yet
        if (!rooms.ContainsKey(roomId))
        {
            rooms.TryAdd(roomId, new Room {RoomId = roomId, CustomGameSeed = customGameSeed, BotType = botType});
        }

        // Add the connection to the room
        var room = rooms[roomId];
        if (room.Connections.All(c => c.PersistentPlayerId != persistentPlayerId))
        {
            // See if the player has a rank
            using var scope = scopeFactory.CreateScope();
            var gameResultContext = scope.ServiceProvider.GetRequiredService<GameResultContext>();
            var playerDao = await gameResultContext.Players.FindAsync(persistentPlayerId);
            room.Connections.Add(
                new GameParticipant
                {
                    Name = playerDao?.Name ?? "Player",
                    PersistentPlayerId = persistentPlayerId,
                    Rank = EloService.GetRank(playerDao?.Elo ?? EloService.StartingElo),
                    PlayerIndex = -1
                }
            );
        }

        if (room.HasGameStarted)
        {
            UpdateRoomsPlayerIds(roomId);
            await SendCurrentGameState(roomId);
        }
        else
        {
            if (botType != null)
            {
                await botService.SeedBot(botType.Value);
                await JoinRoom(roomId, BotConfigurations.GetBot(botType.Value).PersistentId, null);
            }
        }

        await SendCurrentLobbyState(roomId);
        return Result.Successful();
    }

    public async Task<Result> LeaveRoom(string roomId, Guid persistentPlayerId)
    {
        // Check we have a room with that Id
        if (!rooms.ContainsKey(roomId))
        {
            return Result.Successful();
        }

        // Remove the connection to the room
        var room = rooms[roomId];
        room.Connections.RemoveAll(c => c.PersistentPlayerId == persistentPlayerId);

        if (room.Connections.Count <= 0 || room.IsBotGame)
        {
            room.StopBots?.Cancel();
            rooms.TryRemove(roomId, out _);
        }
        else
        {
            UpdateRoomsPlayerIds(roomId);
            await SendCurrentLobbyState(roomId);
            if (room.HasGameStarted)
            {
                await SendCurrentGameState(roomId);
            }
        }

        return Result.Successful();
    }

    public async Task<Result> LeaveAllRooms(Guid persistentPlayerId)
    {
        var roomsContainingPlayer =
            rooms.Where(r => r.Value.Connections.Any(c => c.PersistentPlayerId == persistentPlayerId));
        foreach ((string? key, Room? room) in roomsContainingPlayer)
        {
            await LeaveRoom(room.RoomId, persistentPlayerId);
        }

        return Result.Successful();
    }

    public async Task<Result> UpdateName(string roomId, string updatedName, Guid persistentPlayerId)
    {
        var result = GetGameParticipant(roomId, persistentPlayerId)
            .Map(
                player =>
                {
                    player.Name = updatedName;
                    return Result.Successful();
                },
                err => Result.Error(err.Message)
            )
            .Map(
                () => GetConnectionsRoom(roomId, persistentPlayerId),
                err => Result.Error<Room>(err.Message)
            );
        if (result.Failure)
        {
            return result;
        }

        var room = result.Data;
        await SendCurrentLobbyState(roomId);
        return Result.Successful();
    }

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
            new Settings {RandomSeed = room.CustomGameSeed},
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
                    () => GetConnectionsGame(roomId, bot.PersistentId).Data.Game.State,
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

        await Task.WhenAll(SendCurrentGameState(roomId), SendCurrentLobbyState(roomId));

        return Result.Successful();
    }

    public async Task<Result> TryPlayCard(
        string roomId,
        Guid persistentPlayerId,
        int cardId,
        int centerPilIndex
    )
    {
        try
        {
            var gameResult = GetConnectionsGame(roomId, persistentPlayerId);
            if (gameResult.Failure)
            {
                return gameResult;
            }

            var (room, game) = gameResult.Data;
            var gameParticipantResult = GetGameParticipant(roomId, persistentPlayerId);
            if (gameParticipantResult.Failure)
            {
                return gameParticipantResult;
            }

            var playerId = gameParticipantResult.Data.PlayerIndex;
            var playCardResult = game.TryPlayCard(playerId, cardId, centerPilIndex);
            if (playCardResult.Failure)
            {
                return playCardResult;
            }

            // Check if the game has just ended
            if (playCardResult.Data.WinnerIndex != null)
            {
                await HandleGameOver(game.State, rooms[roomId]);
            }

            return Result.Successful();
        }
        finally
        {
            await SendCurrentGameState(roomId);
        }
    }

    public Task<Result<GameStateDto>> GetGameState(string roomId)
    {
        var room = rooms[roomId];
        return Task.FromResult(Result.Successful(GetGameStateDto(room)));
    }

    public async Task<Result> TryPickupFromKitty(string roomId, Guid persistentPlayerId)
    {
        try
        {
            var gameResult = GetConnectionsGame(roomId, persistentPlayerId);
            if (gameResult.Failure)
            {
                return gameResult;
            }

            var (_, game) = gameResult.Data;

            var gameParticipantResult = GetGameParticipant(roomId, persistentPlayerId);
            if (gameParticipantResult.Failure)
            {
                return gameParticipantResult;
            }

            return game.TryPickupFromKitty(gameParticipantResult.Data.PlayerIndex);
        }
        finally
        {
            await SendCurrentGameState(roomId);
        }
    }

    public async Task<Result> TryRequestTopUp(string roomId, Guid persistentPlayerId)
    {
        try
        {
            var gameResult = GetConnectionsGame(roomId, persistentPlayerId);
            if (gameResult.Failure)
            {
                return gameResult;
            }

            var (_, game) = gameResult.Data;

            var gameParticipantResult = GetGameParticipant(roomId, persistentPlayerId);
            if (gameParticipantResult.Failure)
            {
                return gameParticipantResult;
            }

            return game.TryRequestTopUp(gameParticipantResult.Data.PlayerIndex);
        }
        finally
        {
            await SendCurrentGameState(roomId);
        }
    }

    private async Task HandleGameOver(GameState finalGameState, Room room)
    {
        Console.WriteLine($"{room.RoomId} has ended");
        var dailyGame = room.BotType == BotType.Daily;
        await statService.UpdateGameOverStats(finalGameState, room.Connections, dailyGame);
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

    private Result<(Room Room, WebGame Game)> GetConnectionsGame(
        string roomId,
        Guid persistentPlayerId
    )
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

    private Result<GameParticipant> GetGameParticipant(string roomId, Guid persistentPlayerId)
    {
        return GetConnectionsRoom(roomId, persistentPlayerId)
            .Map(
                room =>
                    Result.Successful(
                        room.Connections.Single(pl => pl.PersistentPlayerId == persistentPlayerId)
                    ),
                err => Result.Error<GameParticipant>(err.Message)
            );
    }

    private Task SendCurrentGameState(string roomId)
    {
        if (!rooms.TryGetValue(roomId, out var room))
        {
            throw new Exception("Trying to send a game state for a room that does not exist");
        }

        return gameUpdateHandler.HandleGameUpdate(roomId, GetGameStateDto(room));
    }

    private Task SendCurrentLobbyState(string roomId)
    {
        if (!rooms.TryGetValue(roomId, out var room))
        {
            throw new Exception("Trying to send a game state for a room that does not exist");
        }

        return gameUpdateHandler.HandleLobbyUpdate(roomId, GetLobbyStateDto(room));
    }

    private LobbyStateDto GetLobbyStateDto(Room room)
    {
        var LobbyStateDto = new LobbyStateDto(
            room.Connections.ToList(),
            room.IsBotGame,
            room.HasGameStarted
        );
        return LobbyStateDto;
    }

    private GameStateDto GetGameStateDto(Room room)
    {
        if (!room.HasGameStarted)
        {
            throw new Exception("Game has not started");
        }

        return new GameStateDto(room.Game.State, room.Connections);
    }

    private void UpdateRoomsPlayerIds(string roomId)
    {
        var room = rooms[roomId];
        var assignedPlayerIds = room.Connections.Where(p=>p.PlayerIndex != -1).Select(p=>p.PlayerIndex);
        var unassignedPlayers = room.Connections.Where(p=>p.PlayerIndex == -1);

        // Get a list of assignable playerIds
        var playerIdsToAssign = Enumerable
            .Range(0, GameEngine.PlayersPerGame)
            .Except(assignedPlayerIds)
            .ToList();
        
        foreach (var player in unassignedPlayers)
        {
            player.PlayerIndex = playerIdsToAssign.Pop();
        }
    }
}