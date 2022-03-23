namespace Engine.UnitTests;

using System.Collections.Generic;
using System.Linq;
using Xunit;

public class TopUpTests
{
    [Theory]
    [InlineData(5, 6, false)]
    [InlineData(5, 7, true)]
    public void RequestTopUp_Theory(int centerCard, int player1Card, bool expectedCanRequestTopUp)
    {
        // Arrange
        var gameState = ModelGenerator.CreateGameBasic(centerCard, player1Card: player1Card);

        // Act
        // See if we can request top up
        var canRequestTopUpResult = GameEngine.TryRequestTopUp(gameState, 0);

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
        var nullableCenterPile1Cards = centerPile1.Select(i => new int?(i)).ToList();
        var nullableCenterPile2Cards = centerPile2.Select(i => new int?(i)).ToList();
        var nullablePlayer1TopUps = player1TopUps.Select(i => new int?(i)).ToList();
        var nullablePlayer2TopUps = player2TopUps.Select(i => new int?(i)).ToList();

        // Arrange
        var gameState = ModelGenerator.CreateGameCustom(
            nullableCenterPile1Cards, nullableCenterPile2Cards,
            player1TopUps: nullablePlayer1TopUps, player1RequestingTopup: true,
            player2TopUps: nullablePlayer2TopUps, player2RequestingTopup: false);

        // Act
        var topUpResult = GameEngine.TryRequestTopUp(gameState, 1);

        // Assertion
        Assert.Equal(expectedCanTopUp, topUpResult.Success);
        Assert.Equal(!expectedCanTopUp, topUpResult.Data.Players[0].RequestingTopUp);
        Assert.Equal(!expectedCanTopUp, topUpResult.Data.Players[1].RequestingTopUp);

        if (expectedCanTopUp && player1TopUps.Length > 1)
        {
            // Check the last top up cards have been added to the center piles
            Assert.Equal((CardValue)player1TopUps.Last(), topUpResult.Data.CenterPiles[0].Cards.Last().CardValue);
            Assert.Equal((CardValue)player2TopUps.Last(), topUpResult.Data.CenterPiles[1].Cards.Last().CardValue);

            // Check they have been removed from the top up piles
            Assert.DoesNotContain(topUpResult.Data.Players[0].TopUpCards,
                card => card.CardValue == (CardValue)player1TopUps.Last());
            Assert.DoesNotContain(topUpResult.Data.Players[1].TopUpCards,
                card => card.CardValue == (CardValue)player2TopUps.Last());
        }
    }

    [Fact]
    public void Replenish()
    {
        // Arrange
        var gameState = ModelGenerator.CreateGameCustom(
            new List<int?> {1, 2, 3}, new List<int?> {4, 5, 6},
            player1RequestingTopup: true,
            player2RequestingTopup: false);

        // Act
        var replenishedResult = GameEngine.TryRequestTopUp(gameState, 1);

        // Assertion
        Assert.True(replenishedResult.Success);
        // Check the players have their top up piles replenished
        var totalCenterPileCards = 6;
        var numberOfCenterPiles = 2;
        Assert.Equal(totalCenterPileCards - numberOfCenterPiles,
            replenishedResult.Data.Players[0].TopUpCards.Count +
            replenishedResult.Data.Players[1].TopUpCards.Count);

        // Check the players have topped up the center piles
        Assert.Single(replenishedResult.Data.CenterPiles[0].Cards);
        Assert.Single(replenishedResult.Data.CenterPiles[1].Cards);
    }

    // Once a move takes place if a player requesting top up can now move then they should no longer be requesting top up
    [Theory]
    [InlineData(new[] {1}, new[] {2, 9}, new[] {3}, false)]
    [InlineData(new[] {1}, new[] {2, 9}, new[] {4}, true)]
    public void RequestTopUp_ResetsAfterMove_Theory(int[] centerPile, int[] player1, int[] player2,
        bool expectedPlayer2Requesting)
    {
        // Convert inline data types to usable array
        var centerPileNullable = centerPile.Select(i => new int?(i)).ToList();
        var player1Nullable = player1.Select(i => new int?(i)).ToList();
        var player2Nullable = player2.Select(i => new int?(i)).ToList();

        // Arrange
        var gameState = ModelGenerator.CreateGameCustom(
            centerPileNullable,
            player1Cards: player1Nullable,
            player2Cards: player2Nullable,
            player1RequestingTopup: false,
            player2RequestingTopup: true);

        // Act
        // Player 1 moves
        var playResult = GameEngine.TryPlayCard(gameState,
            0, gameState.Players[0].HandCards[0], 0);

        // Assertion
        Assert.Equal(expectedPlayer2Requesting, playResult.Data.Players[1].RequestingTopUp);
    }
}
