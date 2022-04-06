namespace Engine;

using Helpers;
using Models;

public struct BotData
{
	public string Name;
	public string? CustomIntroMessage;
	public string? CustomWinMessage;
	public string? CustomLoseMessage;
	public int QuickestResponseTimeMs;
	public int SlowestResponseTimeMs;
}

public static class BotRunner
{
	// public BotData data { get; init; }
	// public BotRunner(BotData data) => this.data = data;

	// public static Result GetMove(

	// todo: get list of available moves
	// todo: bot chooses one
	// todo: Make EngineChecks accessible from
	// todo: input EngineChecks

	public static Result MakeMove(Game game, int playerId)
	{
		var winnerResult = game.TryGetWinner();
		return game.TryGetWinner().Map(winningPlayer => Result.Error(
				$"can't move when {winnerResult.Data.Name} has won already!"),
			_ => game.gameEngine.Checks.PlayerHasPlay(game.State, playerId).Map(
				play => game.TryPlayCard(playerId, play.card.Id, play.centerPile),
				_ => game.TryPickupFromKitty(playerId).MapError(
					_ => game.TryRequestTopUp(playerId).MapError(
						_ => Result.Error("No moves for bot to make"))
				)
			)
		);
	}
}
