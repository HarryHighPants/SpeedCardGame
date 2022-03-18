using System.Collections.Generic;
using System.Linq;
using Engine.Helpers;
using Xunit;

namespace Engine.UnitTests;

public class TopUpTests
{
    [Theory]
    [InlineData(5, 6, false)]
    [InlineData(5, 7, true)]
    public void RequestTopUp_Theory(int centerCard, int player1Card, bool expectedCanRequestTopUp)
    {
        // Arrange
        GameState gameState = ScenarioHelper.CreateGameBasic(centerCard, player1Card: player1Card);

        // Act
        // See if we can request top up
        Result<(GameState updatedGameState, bool couldTopUp)> canRequestTopUpResult =
            GameEngine.TryRequestTopUp(gameState, gameState.Players[0], false);

        // Assertion
        Assert.Equal(expectedCanRequestTopUp, canRequestTopUpResult.Success);
    }

    [Theory]
    [InlineData(new[] {1, 2, 3}, new[] {4, 5, 6}, new[] {7}, new[] {8}, true)]
    [InlineData(new[] {1, 2, 3}, new[] {4, 5, 6}, new int[] { }, new int[] { }, true)]
    public void TopUp_Theory(int[] centerPile1, int[] centerPile2, int[] player1TopUps, int[] player2TopUps,
        bool expectedCanTopUp)
    {
        // Convert inline data types to usable array
        List<int?> nullableCenterPile1Cards = centerPile1.Select(i => new int?(i)).ToList();
        List<int?> nullableCenterPile2Cards = centerPile2.Select(i => new int?(i)).ToList();
        List<int?> nullablePlayer1TopUps = player1TopUps.Select(i => new int?(i)).ToList();
        List<int?> nullablePlayer2TopUps = player2TopUps.Select(i => new int?(i)).ToList();

        // Arrange
        GameState gameState = ScenarioHelper.CreateGameCustom(
            nullableCenterPile1Cards, nullableCenterPile2Cards,
            player1TopUps: nullablePlayer1TopUps, player1RequestingTopup: true,
            player2TopUps: nullablePlayer2TopUps, player2RequestingTopup: true);

        // Act
        Result<GameState> topUpResult = GameEngine.TryTopUp(gameState);

        // Assertion
        Assert.Equal(expectedCanTopUp, topUpResult.Success);
        if (expectedCanTopUp && player1TopUps.Length > 1)
        {
            // Check the last top up cards have been added to the center piles
            Assert.Equal(player1TopUps.Last(), topUpResult.Data.CenterPiles[0].Last().Value);
            Assert.Equal(player2TopUps.Last(), topUpResult.Data.CenterPiles[1].Last().Value);

            // Check they have been removed from the top up piles
            Assert.DoesNotContain(topUpResult.Data.Players[0].TopUpCards, card => card.Value == player1TopUps.Last());
            Assert.DoesNotContain(topUpResult.Data.Players[1].TopUpCards, card => card.Value == player2TopUps.Last());
        }
    }

    [Theory]
    [InlineData(new[] {1, 2, 3}, new[] {4, 5, 6}, true)]
    [InlineData(null, null, false)]
    public void Replenish_Theory(int[]? centerPile1, int[]? centerPile2,
        bool expectedCanReplenish)
    {
        // Convert inline data types to usable array
        List<int?> nullableCenterPile1Cards =
            centerPile1 == null ? new List<int?>() : centerPile1.Select(i => new int?(i)).ToList();
        List<int?> nullableCenterPile2Cards =
            centerPile2 == null ? new List<int?>() : centerPile2.Select(i => new int?(i)).ToList();

        // Arrange
        GameState gameState = ScenarioHelper.CreateGameCustom(
            nullableCenterPile1Cards, nullableCenterPile2Cards,
            player1RequestingTopup: true,
            player2RequestingTopup: true);

        // Act
        Result<GameState> replenishedResult = GameEngine.ReplenishTopUpCards(gameState);

        // Assertion
        Assert.Equal(expectedCanReplenish, replenishedResult.Success);
        if (expectedCanReplenish)
        {
            // Check the players have had the center piles moved into their top up decks
            int totalCenterPileCards = centerPile1.Length + centerPile2.Length;
            Assert.Equal(totalCenterPileCards,
                replenishedResult.Data.Players[0].TopUpCards.Count +
                replenishedResult.Data.Players[0].TopUpCards.Count);

            // Check the center piles are now empty
            Assert.Empty(replenishedResult.Data.CenterPiles[0]);
            Assert.Empty(replenishedResult.Data.CenterPiles[1]);
        }
    }
}