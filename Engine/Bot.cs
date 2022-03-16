using Engine.CliHelpers;

namespace Engine;

public class Bot
{
    public static (GameState? updatedGameState, string? moveMade, string? errorMessage) MakeMove(
        GameState gameState, Player player)
    {
        Player? winner = GameEngine.TryGetWinner(gameState);
        if (winner != null)
        {
            return (null, null, $"can't move when {winner.Name} has won already!");
        }

        // See if we can play any cards
        (bool canPlay, Card? card, int? centerPile) playCard = GameEngine.PlayerHasPlay(gameState, player);
        if (playCard.canPlay)
        {
            (GameState? updatedGameState, string? errorMessage) play =
                GameEngine.TryPlayCard(gameState, playCard.card!, (int) playCard.centerPile!);
            if (play.updatedGameState != null)
            {
                return (play.updatedGameState, $"played card {CliGameUtils.CardToString(playCard.card!)}",
                    null);
            }
        }

        // See if we can pickup
        (GameState? updatedGameState, Card? pickedUpCard, string? errorMessage) pickupFromKitty =
            GameEngine.TryPickupFromKitty(gameState, player);
        if (pickupFromKitty.pickedUpCard != null)
        {
            return (pickupFromKitty.updatedGameState,
                $"picked up card {CliGameUtils.CardToString(pickupFromKitty.pickedUpCard!)}",
                null);
        }

        // Request Top up
        (GameState? updatedGameState, bool immediateTopUp, bool readyToTopUp, string? errorMessage) topUp =
            GameEngine.TryRequestTopUp(gameState, player);
        if (topUp.updatedGameState != null)
        {
            return (topUp.updatedGameState,
                "is ready to top up",
                null);
        }


        return (null, null, "Bot can't do anything");
    }
}