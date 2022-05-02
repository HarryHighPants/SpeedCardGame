namespace Server;

using System.Collections.Concurrent;

public class Room
{
	public ConcurrentDictionary<string, Connection> Connections = new();
	public WebGame? Game;
	public string RoomId;
	public bool IsBotGame;
}