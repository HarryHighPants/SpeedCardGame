namespace Server;

using Engine.Helpers;
using Engine.Models;

public interface IGameService
{
    public Task JoinRoom(string roomId, Guid persistentPlayerId, BotType? botType);
    public Task LeaveRoom(string roomId, Guid persistentPlayerId);
    public Task UpdateName(string updatedName, Guid persistentPlayerId);

    public Task<Result> StartGame(string roomId, Guid persistentPlayerId);
    public Task<Result> TryPickupFromKitty(string roomId, Guid persistentPlayerId);
    public Task<Result> TryRequestTopUp(string roomId, Guid persistentPlayerId);
    public Task<Result> TryPlayCard(
        string roomId,
        Guid persistentPlayerId,
        int cardId,
        int centerPilIndex
    );
}
