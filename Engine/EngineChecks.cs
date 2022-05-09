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

    public bool CanPlayerRequestTopUp(GameState gameState, int playerIndex) =>
        gameState.MustTopUp && gameState.Players[playerIndex].RequestingTopUp;

    public bool MustTopUp(GameState gameState) =>
        !gameState.Players.Any(
            p =>
                PlayerHasPlay(gameState, p.Id).Success
                && CanPickupFromKitty(gameState, p.Id).Success
        );

    public bool AllPlayersRequestingTopUp(GameState gameState) =>
        gameState.Players.All(p => p.RequestingTopUp);

    public Result<(Card card, int centerPile)> PlayerHasPlay(
        GameState gameState,
        int playerId,
        List<bool>? centerPilesAvailable = null
    )
    {
        var availablePiles = centerPilesAvailable ?? new List<bool> { true, true };
        var player = gameState.GetPlayer(playerId);

        foreach (var card in player.HandCards)
        {
            var cardHasPlayResult = CardHasPlay(gameState, card, availablePiles);
            if (cardHasPlayResult.Success)
            {
                return Result.Successful((card, cardHasPlayResult.Data));
            }
        }

        return Result.Error<(Card card, int centerPile)>("No valid play for player");
    }

    public Result PlayersMoveValid(
        GameState gameState,
        int playerId,
        int cardId,
        int centerPileIndex
    )
    {
        if (centerPileIndex >= gameState.CenterPiles.Count)
        {
            return Result.Error<GameState>($"No center pile found at index {centerPileIndex}");
        }

        var card = gameState.GetCard(cardId);
        var cardLocationResult = card.Location(gameState);

        if (cardLocationResult.PlayerId != playerId)
        {
            return Result.Error<GameState>(
                $"{card.ToString(gameState.Settings)} not in players hand"
            );
        }

        switch (cardLocationResult.PileName)
        {
            case CardPileName.Hand:
                // Check that the card can be played onto the relevant center piles top card
                if (!ValidPlay(card, gameState.CenterPiles[centerPileIndex].Cards.Last()))
                {
                    return Result.Error<GameState>(
                        $"Can't play {card.ToString(gameState.Settings)} onto {gameState.CenterPiles[centerPileIndex].Cards.Last().ToString(gameState.Settings)}"
                    );
                }

                return Result.Successful();

            case CardPileName.Kitty:
            case CardPileName.TopUp:
            case CardPileName.Center:
                return Result.Error<GameState>(
                    $"Can't play a card in the {cardLocationResult.PileName} pile"
                );

            case CardPileName.Undefined:
            default:
                return Result.Error<GameState>("Card not found in gameState");
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="gameState"></param>
    /// <param name="card"></param>
    /// <returns>Result with CenterPileindex as data</returns>
    public Result<int> CardHasPlay(
        GameState gameState,
        Card card,
        List<bool>? centerPilesAvailable = null
    )
    {
        var pilesAvailable = centerPilesAvailable ?? new List<bool> { true, true };
        for (var i = 0; i < gameState.CenterPiles.Count; i++)
        {
            if (!pilesAvailable[i] || gameState.CenterPiles[i].Cards.Count < 1)
            {
                continue;
            }

            var pileCard = gameState.CenterPiles[i].Cards.Last();
            if (ValidPlay(card, pileCard))
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
            return Result.Error($"Players hand is full");
        }

        // Check there is enough cards from players kitty
        if (player?.KittyCards.Count < 1)
        {
            return Result.Error($"No cards left to pickup");
        }

        return new SuccessResult();
    }
}
