namespace Server;

public class Connection
{
	public Guid PersistentPlayerId;
	public string ConnectionId;
	public string Name;
	// GameEngine Player index
	public int? PlayerId;
	public Rank? Rank;
}