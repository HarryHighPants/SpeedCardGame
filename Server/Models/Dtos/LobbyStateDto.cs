namespace Server;

public class LobbyStateDto
{
	public List<ConnectionDto> Connections { get; set; }
	public bool IsBotGame { get; set; }
	public bool GameStarted { get; set; }

	public LobbyStateDto(List<GameParticipant> connections, bool isBotGame, bool gameStarted)
	{
		this.IsBotGame = isBotGame;
		this.Connections = connections.Select(c => new ConnectionDto(c)).ToList();
		this.GameStarted = gameStarted;
	}
}
