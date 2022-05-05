namespace Server;

using System.Collections.Concurrent;

public class Room
{
	public List<Connection> Connections = new();
	public WebGame? Game;
	public string RoomId;
	public bool IsBotGame;
	public int? CustomGameSeed;
}
