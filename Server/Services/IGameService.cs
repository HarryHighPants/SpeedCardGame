namespace Server;

using Engine.Helpers;
using Engine.Models;

public interface IGameService
{
	public void JoinRoom(string roomId, Guid persistentPlayerId, BotType? botType);
	public void LeaveRoom(string roomId, Guid persistentPlayerId);
	public void UpdateName(string updatedName, Guid persistentPlayerId);
	public Result<LobbyStateDto> GetLobbyStateDto(string roomId);


	public Result StartGame(string roomId, Guid persistentPlayerId);
	public Result TryPickupFromKitty(string roomId, Guid persistentPlayerId);
	public Result TryRequestTopUp(string roomId, Guid persistentPlayerId);
	public Result TryPlayCard(string roomId, Guid persistentPlayerId, int cardId, int centerPilIndex);
	public Result<GameStateDto> GetGameStateDto(string roomId);
}
