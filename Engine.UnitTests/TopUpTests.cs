namespace Engine.UnitTests;

using System.Collections.Generic;
using System.Linq;
using Models;
using Xunit;

public class TopUpTests
{

    [Theory]
    [InlineData(new[] { 1, 2, 3 }, new[] { 4, 5, 6 }, new[] { 7 }, new[] { 8 }, true)]
    [InlineData(new[] { 1, 2, 3 }, new[] { 4, 5, 6 }, new int[] { }, new int[] { }, true)]
    public void TopUp_Theory(
        int[] centerPile1,
        int[] centerPile2,
        int[] player1TopUps,
        int[] player2TopUps,
        bool expectedCanTopUp
    )
    {
        // Convert inline data types to usable array
        var nullableCenterPile1Cards = centerPile1.Select(i => new int?(i)).ToList();
        var nullableCenterPile2Cards = centerPile2.Select(i => new int?(i)).ToList();
        var nullablePlayer1TopUps = player1TopUps.Select(i => new int?(i)).ToList();
        var nullablePlayer2TopUps = player2TopUps.Select(i => new int?(i)).ToList();

        // Arrange
        var gameState = ModelGenerator.CreateGameCustom(
            nullableCenterPile1Cards,
            nullableCenterPile2Cards,
            player1TopUps: nullablePlayer1TopUps,
            player1RequestingTopup: true,
            player2TopUps: nullablePlayer2TopUps,
            player2RequestingTopup: false,
            mustTopUp:true
        );

        // Act
        var gameEngine = new GameEngine(new EngineChecks(), new EngineActions());
        var topUpResult = gameEngine.TryRequestTopUp(gameState, 1);

        // Assertion
        Assert.Equal(expectedCanTopUp, topUpResult.Success);
        Assert.Equal(!expectedCanTopUp, topUpResult.Data.Players[0].RequestingTopUp);
        Assert.Equal(!expectedCanTopUp, topUpResult.Data.Players[1].RequestingTopUp);

        if (expectedCanTopUp && player1TopUps.Length > 1)
        {
            // Check the last top up cards have been added to the center piles
            Assert.Equal(
                (CardValue)player1TopUps.Last(),
                topUpResult.Data.CenterPiles[0].Cards.Last().CardValue
            );
            Assert.Equal(
                (CardValue)player2TopUps.Last(),
                topUpResult.Data.CenterPiles[1].Cards.Last().CardValue
            );

            // Check they have been removed from the top up piles
            Assert.DoesNotContain(
                topUpResult.Data.Players[0].TopUpCards,
                card => card.CardValue == (CardValue)player1TopUps.Last()
            );
            Assert.DoesNotContain(
                topUpResult.Data.Players[1].TopUpCards,
                card => card.CardValue == (CardValue)player2TopUps.Last()
            );
        }
    }

    // Once a move takes place if a player requesting top up can now move then they should no longer be requesting top up
    [Theory]
    [InlineData(new[] { 1 }, new[] { 2, 9 }, new[] { 3 }, false)]
    [InlineData(new[] { 1 }, new[] { 2, 9 }, new[] { 4 }, true)]
    public void RequestTopUp_ResetsAfterMove_Theory(
        int[] centerPile,
        int[] player1,
        int[] player2,
        bool expectedPlayer2Requesting
    )
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
            player2RequestingTopup: true
        );

        // Act
        // Player 1 moves
        var gameEngine = new GameEngine(new EngineChecks(), new EngineActions());
        var playResult = gameEngine.TryPlayCard(
            gameState,
            0,
            gameState.Players[0].HandCards[0].Id,
            0
        );

        // Assertion
        Assert.Equal(expectedPlayer2Requesting, playResult.Data.Players[1].RequestingTopUp);
    }
}
