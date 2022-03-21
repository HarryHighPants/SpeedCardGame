using Engine.CliHelpers;
using Engine.Helpers;

namespace Engine;

public class BotRunner
{
    public static Result<(GameState updatedGameState, string moveMade)> MakeMove(
        GameState gameState, int playerIndex)
    {
        var winnerResult = GameEngine.TryGetWinner(gameState);
        if (winnerResult.Success)
        {
            return Result.Error<(GameState updatedGameState, string moveMade)>(
                $"can't move when {gameState.Players[winnerResult.Data].Name} has won already!");
        }

        // See if we can play any cards
        Result<(Card card, int centerPile)> playCardResult = GameEngine.PlayerHasPlay(gameState, playerIndex);
        if (playCardResult.Success)
        {
            Result<GameState> playResult =
                GameEngine.TryPlayCard(gameState, playerIndex, playCardResult.Data.card, playCardResult.Data.centerPile);
            if (playResult.Success)
            {
                return Result.Successful((playResult.Data,
                    $"played card {CliGameUtils.CardToString(playCardResult.Data.card)} onto pile {playCardResult.Data.centerPile + 1}"));
            }
        }

        // See if we can pickup
        Result<(GameState updatedGameState, Card pickedUpCard)> pickupFromKittyResult =
            GameEngine.TryPickupFromKitty(gameState, playerIndex);
        if (pickupFromKittyResult.Success)
        {
            return Result.Successful((
                pickupFromKittyResult.Data.updatedGameState,
                $"picked up card {CliGameUtils.CardToString(pickupFromKittyResult.Data.pickedUpCard)}"));
        }

        // Request Top up
        if (!gameState.Players[playerIndex].RequestingTopUp)
        {
            Result<(GameState updatedGameState, bool couldTopUp)> topUpResult =
                GameEngine.TryRequestTopUp(gameState, playerIndex);
            if (topUpResult.Success)
            {
                return Result.Successful((
                    topUpResult.Data.updatedGameState,
                    topUpResult.Data.couldTopUp ? "topped up the center piles" : "is ready to top up"));
            }
        }

        return Result.Successful((gameState, ""));
    }
}