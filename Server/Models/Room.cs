namespace Server;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

public class Room
{
	public List<GameParticipant> Connections { get; set; } = new();
	public CancellationTokenSource? StopBots {get; set;}
	public WebGame? Game {get; set;}
	public BotType? BotType {get; set;}
	public string RoomId  {get; set;}
	public int? CustomGameSeed  {get; set;}
	[MemberNotNullWhen(true, nameof(BotType))]
	public bool IsBotGame => BotType != null;

	[MemberNotNullWhen(true, nameof(Game))]
	public bool HasGameStarted => Game != null;
}
