namespace Server;

public class LobbyStateDto
{
	public List<ConnectionDto> Connections;
	public bool IsBotGame;
	public bool GameStarted;

	public LobbyStateDto(List<Connection> connections, bool isBotGame, bool gameStarted)
	{
		this.IsBotGame = isBotGame;
		this.Connections = connections.Select(c=>new ConnectionDto(c)).ToList();
		this.GameStarted = gameStarted;
	}
}