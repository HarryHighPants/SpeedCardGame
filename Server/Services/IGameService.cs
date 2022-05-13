using Engine.Models;

namespace Server;

using Engine.Helpers;

public interface IGameService
{
    public Task<Result> JoinRoom(string roomId, Guid persistentPlayerId, BotType? botType);
    public Task<Result> LeaveRoom(string roomId, Guid persistentPlayerId);
    public Task<Result> LeaveAllRooms(Guid persistentPlayerId);
    public Task<Result> UpdateName(string roomId, string updatedName, Guid persistentPlayerId);

    public Task<Result> StartGame(string roomId, Guid persistentPlayerId);
    public Task<Result> TryPickupFromKitty(string roomId, Guid persistentPlayerId);
    public Task<Result> TryRequestTopUp(string roomId, Guid persistentPlayerId);

    public Task<Result> TryPlayCard(
        string roomId,
        Guid persistentPlayerId,
        int cardId,
        int centerPilIndex
    );

    public Task<Result<GameStateDto>> GetGameState(string roomId);
}