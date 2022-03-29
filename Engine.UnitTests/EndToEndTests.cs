namespace Engine.UnitTests;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Helpers;
using Models;
using Xunit;
using Xunit.Abstractions;

public class EndToEndTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public EndToEndTests(ITestOutputHelper testOutputHelper) => _testOutputHelper = testOutputHelper;

    [Fact]
    public void BotGame()
    {
        var game = new Game();

        var movesMade = 0;
        while (game.TryGetWinner().Failure && movesMade < 1000)
        {
            for (var i = 0; i < game.State.Players.Count; i++)
            {
                var move = BotRunner.MakeMove(game, i);
                if (move is IErrorResult)
                {
                    continue;
                }

                _testOutputHelper.WriteLine(game.State.LastMove);
                movesMade++;
            }
        }

        var winnerResult = game.TryGetWinner();
        _testOutputHelper.WriteLine(
            $"Bot game complete with {winnerResult.Data.Name} winning in {movesMade} moves");
        Assert.True(game.TryGetWinner().Success);
    }

    [Fact]
    public void TopUpExample()
    {
        // Arrange
        var gameState =
            ModelGenerator.CreateGameCustom(
                new List<int?> {1},
                new List<int?> {1},
                new List<int?> {2},
                new List<int?> {6},
                new List<int?> {4},
                player2Cards: new List<int?> {5, 5},
                player2TopUps: new List<int?> {4}
            );

        var gameEngine = new GameEngine();
        // Act
        gameState = gameEngine.TryRequestTopUp(gameState, 1).Data;
        gameState = gameEngine.TryPlayCard(gameState, 0, gameState.Players[0].HandCards[0].Id, 0).Data;
        Assert.Equal((CardValue)2, gameState.CenterPiles[0].Cards.Last().CardValue);

        gameState = gameEngine.TryPickupFromKitty(gameState, 0).Data;
        gameState = gameEngine.TryRequestTopUp(gameState, 0).Data;
        Assert.Equal((CardValue)4, gameState.CenterPiles[0].Cards.Last().CardValue);

        gameState = gameEngine.TryPlayCard(gameState, 1, gameState.Players[1].HandCards[0].Id, 0).Data;
        gameState = gameEngine.TryPlayCard(gameState, 0, gameState.Players[0].HandCards[0].Id, 0).Data;
        var winnerResult = gameEngine.TryGetWinner(gameState);

        // Assertion
        Assert.Equal(0, winnerResult.Data);
    }

    [Fact]
    public void ReplenishExample()
    {
        // Arrange
        var gameState =
            ModelGenerator.CreateGameCustom(
                new List<int?> {6, 6, 1},
                new List<int?> {6, 6, 1},
                new List<int?> {5},
                player2Cards: new List<int?> {5}
            );

        var player1 = gameState.Players[0];
        var player2 = gameState.Players[1];

        // Act
        var gameEngine = new GameEngine();
        gameState = gameEngine.TryRequestTopUp(gameState, 1).Data;
        gameState = gameEngine.TryRequestTopUp(gameState, 0).Data;
        Assert.Single(gameState.CenterPiles[0].Cards);

        // Manually add a 6 to ensure the 6 "was" shuffled in first
        var newCenterPile =
            new CenterPile {Cards = gameState.CenterPiles[0].Cards.Add(ModelGenerator.CreateBasicCard(6))};
        var newCenterPiles = gameState.CenterPiles.ReplaceElementAt(0, newCenterPile);
        gameState = gameState with {CenterPiles = newCenterPiles.ToImmutableList()};

        // Try to play a card
        gameState = gameEngine.TryPlayCard(gameState, 0, player1.HandCards[0].Id, 0).Data;
        var winnerResult = gameEngine.TryGetWinner(gameState);

        // Assertion
        Assert.Equal(0, winnerResult.Data);
    }
}
