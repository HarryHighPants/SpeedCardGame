using Engine.CliHelpers;
using Engine.Helpers;

namespace Engine;

public class Bot
{
    public static Result<(GameState updatedGameState, string moveMade)> MakeMove(
        GameState gameState, Player player)
    {
        Result<Player> winnerResult = GameEngine.TryGetWinner(gameState);
        if (winnerResult.Success)
        {
            return new ErrorResult<(GameState updatedGameState, string moveMade)>(
                $"can't move when {winnerResult.Data.Name} has won already!");
        }

        // See if we can play any cards
        Result<(Card card, int centerPile)> playCardResult = GameEngine.PlayerHasPlay(gameState, player);
        if (playCardResult.Success)
        {
            Result<GameState> playResult =
                GameEngine.TryPlayCard(gameState, player, playCardResult.Data.card, playCardResult.Data.centerPile);
            if (playResult.Success)
            {
                return new SuccessResult<(GameState updatedGameState, string moveMade)>((playResult.Data,
                    $"played card {CliGameUtils.CardToString(playCardResult.Data.card)}"));
            }
        }

        // See if we can pickup
        Result<(GameState updatedGameState, Card pickedUpCard)> pickupFromKittyResult =
            GameEngine.TryPickupFromKitty(gameState, player);
        if (pickupFromKittyResult.Success)
        {
            return new SuccessResult<(GameState updatedGameState, string moveMade)>((
                pickupFromKittyResult.Data.updatedGameState,
                $"picked up card {CliGameUtils.CardToString(pickupFromKittyResult.Data.pickedUpCard)}"));
        }

        // Request Top up
        Result<(GameState updatedGameState, bool immediateTopUp, bool readyToTopUp)> topUpResult =
            GameEngine.TryRequestTopUp(gameState, player);
        if (topUpResult.Success)
        {
            return new SuccessResult<(GameState updatedGameState, string moveMade)>((
                topUpResult.Data.updatedGameState,
                "is ready to top up"));
        }


        return new ErrorResult<(GameState updatedGameState, string moveMade)>("Bot can't do anything");
    }
}