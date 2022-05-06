namespace Server;

using Engine.Helpers;
using Engine.Models;

public interface IGameService
{
	public void JoinRoom(string roomId, string connectionId, Guid persistentPlayerId, int? customGameSeed = null);
	public int ConnectionsInRoomCount(string roomId);
	public List<Connection> ConnectionsInRoom(string roomId);

	public void LeaveRoom(string roomId, string connectionId);

	public void UpdateName(string updatedName, string connectionId);

	public Result StartGame(string connectionId);

	public Connection GetConnectionInfo(string connectionId);

	public string GetConnectionsRoomId(string connectionId);
	public string GetPlayersRoomId(Guid persistentPlayerId);
	public string GetPlayersConnectionId(Guid persistentPlayerId);


	public Result TryPickupFromKitty(string connectionId);

	public Result TryRequestTopUp(string connectionId);

	public Result TryPlayCard(string connectionId, int cardId, int centerPilIndex);

	public WebGame GetGame(string roomId);

	public Result<GameStateDto> GetGameStateDto(string roomId);

	public Result<LobbyStateDto> GetLobbyStateDto(string roomId);
	public bool GameStarted(string roomId);
	public bool ConnectionOwnsCard(string connectionId, int cardId);
	public CardLocation? GetCardLocation(string connectionId, int cardId);
}
