namespace Server.Services;

public interface IGameUpdateHandler
{
	public Task HandleGameUpdate(string roomId, GameStateDto gameStateDto);
	public Task HandleLobbyUpdate(string roomId, LobbyStateDto lobbyStateDto);
}
