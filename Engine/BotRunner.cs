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

	public static Result MakeMove(
        Game game, int playerId)
    {
        var winnerResult = game.TryGetWinner();
        if (winnerResult.Success)
        {
            return Result.Error<GameState>(
                $"can't move when {winnerResult.Data.Name} has won already!");
        }

        // See if we can play any cards
        var playCardResult = game.gameEngine.Checks.PlayerHasPlay(game.State, playerId);
        if (playCardResult.Success)
        {
            var playResult = game.TryPlayCard(playerId, playCardResult.Data.card.Id, playCardResult.Data.centerPile);
            if (playResult.Success)
            {
                return Result.Successful();
            }
        }

        // See if we can pickup
        var pickupFromKittyResult = game.TryPickupFromKitty(playerId);
        if (pickupFromKittyResult.Success)
        {
            return Result.Successful();
        }

        // See if we can request Top up
        var topUpResult = game.TryRequestTopUp(playerId);
        if (topUpResult.Success)
        {
            return Result.Successful();
        }

        return Result.Error<GameState>("No moves for bot to make");
    }
}
