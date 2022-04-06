namespace Server;

using Engine;
using Engine.Models;

public record WebGameState : GameState
{
}

public record WebCard : Card
{
}

public record WebPlayer : Player
{
	public bool IsBot;
}


public record BotPlayer : WebPlayer
{
	public BotData Data;
	// public BotPlayer(BotData data) => Bot = new BotRunner(data);
}
