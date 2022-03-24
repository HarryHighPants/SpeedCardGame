namespace Engine;

using Helpers;
using Models;

public class EngineChecks
{
    /// <summary>
    /// </summary>
    /// <param name="gameState"></param>
    /// <returns>Player Index</returns>
    public Result<int> TryGetWinner(GameState gameState)
    {
        // Check if a player has no cards in their hand or kitty
        foreach (var (player, i) in gameState.Players.IndexTuples())
        {
            if (player.HandCards.Count <= 0 && player.KittyCards.Count <= 0)
            {
                // Return any player that does
                return Result.Successful(i);
            }
        }

        return Result.Error<int>("No winner yet!");
    }

    public Result CanTopUp(GameState gameState)
    {
        // Check all players are requesting top up
        var allPlayersRequestingTopUp = this.AllPlayersRequestingTopUp(gameState);
        if (allPlayersRequestingTopUp is ErrorResult allPlayersRequestingTopUpError)
        {
            return Result.Error(allPlayersRequestingTopUpError.Message);
        }

        return new SuccessResult();
    }

    public Result CanRequestTopUp(GameState gameState, int playerIndex)
    {
        // Check player isn't already topped up
        if (gameState.Players[playerIndex].RequestingTopUp)
        {
            return Result.Error("Already requesting to top up");
        }

        // Check the player can't play
        var hasPlayResult = this.PlayerHasPlay(gameState, playerIndex);
        if (hasPlayResult.Success)
        {
            return Result.Error("Player can play a card");
        }

        // Check the player can't pickup
        var pickupFromKittyResult = this.CanPickupFromKitty(gameState, playerIndex);
        if (pickupFromKittyResult.Success)
        {
            return Result.Error("Player can pickup from kitty");
        }

        // We can't do anything else so top up is valid
        return new SuccessResult();
    }

    private Result AllPlayersRequestingTopUp(GameState gameState) =>
        // Check all players are requesting top up
        !gameState.Players.All(p => p.RequestingTopUp)
            ? Result.Error("Not all players are requesting top up")
            : new SuccessResult();

    public Result<(Card card, int centerPile)> PlayerHasPlay(GameState gameState, int playerId)
    {
        var player = gameState.GetPlayer(playerId);

        foreach (var card in player.HandCards)
        {
            var cardHasPlayResult = this.CardHasPlay(gameState, card);
            if (cardHasPlayResult.Success)
            {
                return Result.Successful((card, cardHasPlayResult.Data));
            }
        }

        return Result.Error<(Card card, int centerPile)>("No valid play for player");
    }

    /// <summary>
    /// </summary>
    /// <param name="gameState"></param>
    /// <param name="card"></param>
    /// <returns>Result with CenterPileindex as data</returns>
    public Result<int> CardHasPlay(GameState gameState, Card card)
    {
        for (var i = 0; i < gameState.CenterPiles.Count; i++)
        {
            if (gameState.CenterPiles[i].Cards.Count < 1)
            {
                continue;
            }

            var pileCard = gameState.CenterPiles[i].Cards.Last();
            if (this.ValidPlay(card, pileCard))
            {
                return Result.Successful(i);
            }
        }

        return Result.Error<int>("Card can't be played onto any center pile");
    }

    public bool ValidPlay(Card? topCard, Card? bottomCard)
    {
        if (topCard == null || bottomCard == null)
        {
            return false;
        }

        if (topCard.CardValue < 0 || bottomCard.CardValue < 0)
        {
            return false;
        }

        var valueDiff = Math.Abs(topCard.CardValue - bottomCard.CardValue);
        return valueDiff is 1 or GameEngine.CardsPerSuit - 1;
    }

    public Result CanPickupFromKitty(GameState gameState, int playerId)
    {
        var player = gameState.GetPlayer(playerId);

        // Check the player has room in their hand
        if (player?.HandCards.Count >= gameState.Settings.MaxHandCards)
        {
            return Result.Error($"Player {player?.Name} hand is full");
        }

        // Check there is enough cards from players kitty
        if (player?.KittyCards.Count < 1)
        {
            return Result.Error($"No cards left in {player.Name} kitty to pickup");
        }

        return new SuccessResult();
    }
}
