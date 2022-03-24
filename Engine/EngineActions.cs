namespace Engine;

using System.Collections.Immutable;
using Helpers;
using Models;

public class EngineActions
{
    public GameState UpdateLastMove(GameState gameState, Move data)
    {
        var newMoveHistory = gameState.MoveHistory.Add(data);
        var newGameState = gameState with {MoveHistory = newMoveHistory};
        newGameState = newGameState with
        {
            LastMove = data.GetDescription(newGameState,
                gameState.Settings.MinifiedCardStrings,
                gameState.Settings.IncludeSuitInCardStrings)
        };
        return newGameState;
    }

    public Result<GameState> ReplenishTopUpCards(GameState gameState)
    {
        // Combine center piles
        var combinedCenterPiles = gameState.CenterPiles.SelectMany(cp => cp.Cards.ToList()).ToList();

        if (combinedCenterPiles.Count < 1)
        {
            return Result.Error<GameState>("Can't replenish top up piles without any center cards");
        }

        // Shuffle them
        combinedCenterPiles.Shuffle(gameState.Settings?.RandomSeed);

        // Split all but center piles count into top up piles
        var topUpPileSize = combinedCenterPiles.Count / gameState.Players.Count;
        var newPlayers =
            gameState.Players.Select(player =>
                    player with {TopUpCards = combinedCenterPiles.PopRange(topUpPileSize).ToImmutableList()})
                .ToImmutableList();

        // Reset the center piles
        var newCenterPiles = new List<CenterPile>();
        for (var i = 0; i < gameState.CenterPiles.Count; i++)
        {
            newCenterPiles.Add(new CenterPile());
        }

        return Result.Successful(gameState with {Players = newPlayers, CenterPiles = newCenterPiles.ToImmutableList()});
    }

    public Result<GameState> PlayCard(GameState gameState, Card card, int centerPileIndex)
    {
        var newGameState = gameState;

        // Add the card being played to the center pile
        var newCenterPiles = newGameState.CenterPiles.ReplaceElementAt(
                centerPileIndex,
                new CenterPile {Cards = gameState.CenterPiles[centerPileIndex].Cards.Append(card).ToImmutableList()})
            .ToImmutableList();
        newGameState = newGameState with {CenterPiles = newCenterPiles};

        // Remove the played card from the players hand
        var (playerWithCard, playerIndexWithCard) =
            newGameState.Players.IndexTuples().First(p => p.item.HandCards.Contains(card));
        var newHandCards = playerWithCard.HandCards.Where(c => c != card).ToImmutableList();
        var newPlayer = playerWithCard with {HandCards = newHandCards};
        var newPlayers = newGameState.Players.ReplaceElementAt(playerIndexWithCard, newPlayer).ToImmutableList();
        newGameState = newGameState with {Players = newPlayers};

        // Add the move history
        newGameState = this.UpdateLastMove(newGameState,
            new Move
            {
                Type = MoveType.PlayCard,
                CardId = card.Id,
                PlayerId = playerWithCard.Id,
                CenterPileIndex = centerPileIndex
            });

        return Result.Successful(newGameState);
    }
}
