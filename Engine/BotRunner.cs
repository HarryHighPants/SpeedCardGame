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

public class BotRunner
{
    public static Result<GameState> MakeMove(
        GameState gameState, int playerIndex)
    {
        var winnerResult = GameEngine.TryGetWinner(gameState);
        if (winnerResult.Success)
        {
            return Result.Error<GameState>(
                $"can't move when {gameState.Players[winnerResult.Data].Name} has won already!");
        }

        // See if we can play any cards
        var playCardResult = GameEngine.PlayerHasPlay(gameState, playerIndex);
        if (playCardResult.Success)
        {
            var playResult =
                GameEngine.TryPlayCard(gameState, playerIndex, playCardResult.Data.card,
                    playCardResult.Data.centerPile);
            if (playResult.Success)
            {
                return Result.Successful(playResult.Data);
            }
        }

        // See if we can pickup
        var pickupFromKittyResult = GameEngine.TryPickupFromKitty(gameState, playerIndex);
        if (pickupFromKittyResult.Success)
        {
            return Result.Successful(pickupFromKittyResult.Data.updatedGameState);
        }

        // See if we can request Top up
        var topUpResult = GameEngine.TryRequestTopUp(gameState, playerIndex);
        if (topUpResult.Success)
        {
            return Result.Successful(topUpResult.Data);
        }

        return Result.Error<GameState>("No moves for bot to make");
    }
}
