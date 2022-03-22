using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Engine.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Engine.UnitTests;

public class EndToEndTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public EndToEndTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void BotGame()
    {
        GameState gameState = GameEngine.NewGame();

        var movesMade = 0;
        while (GameEngine.TryGetWinner(gameState).Failure && movesMade < 1000)
        {
            for (int i = 0; i < gameState.Players.Count; i++)
            {
                var move = BotRunner.MakeMove(gameState, i);
                gameState = move.Success ? move.Data : gameState;
                if (move is IErrorResult) continue;
                
                _testOutputHelper.WriteLine(GameEngine.ReadableLastMove(gameState));
                movesMade++;
            }
        }

        var winnerResult = GameEngine.TryGetWinner(gameState);
        _testOutputHelper.WriteLine($"Bot game complete with {gameState.Players[winnerResult.Data].Name} winning in {movesMade} moves");
        Assert.True(GameEngine.TryGetWinner(gameState).Success);
    }

    [Fact]
    public void TopUpExample()
    {
        // Arrange
        GameState gameState =
            ModelGenerator.CreateGameCustom(
                new List<int?> {1},
                new List<int?> {1},
                new List<int?> {2},
                new List<int?> {6},
                new List<int?> {4},
                player2Cards: new List<int?> {5, 5},
                player2TopUps: new List<int?> {4}
            );

        // Act
        gameState = GameEngine.TryRequestTopUp(gameState, 1).Data;
        gameState = GameEngine.TryPlayCard(gameState, 0, gameState.Players[0].HandCards[0], 0).Data;
        Assert.Equal((CardValue) 2, gameState.CenterPiles[0].Last().CardValue);
        
        gameState = GameEngine.TryPickupFromKitty(gameState, 0).Data.updatedGameState;
        gameState = GameEngine.TryRequestTopUp(gameState, 0).Data;
        Assert.Equal((CardValue) 4, gameState.CenterPiles[0].Last().CardValue);
        
        gameState = GameEngine.TryPlayCard(gameState, 1, gameState.Players[1].HandCards[0], 0).Data;
        gameState = GameEngine.TryPlayCard(gameState, 0, gameState.Players[0].HandCards[0], 0).Data;
        var winnerResult = GameEngine.TryGetWinner(gameState);

        // Assertion
        Assert.Equal(0, winnerResult.Data);
    }

    [Fact]
    public void ReplenishExample()
    {
        // Arrange
        GameState gameState =
            ModelGenerator.CreateGameCustom(
                new List<int?> {6, 6, 1},
                new List<int?> {6, 6, 1},
                new List<int?> {5},
                player2Cards: new List<int?> {5}
            );

        Player player1 = gameState.Players[0];
        Player player2 = gameState.Players[1];

        // Act
        gameState = GameEngine.TryRequestTopUp(gameState, 1).Data;
        gameState = GameEngine.TryRequestTopUp(gameState, 0).Data;
        Assert.Single(gameState.CenterPiles[0]);

        // Manually add a 6 to ensure the 6 "was" shuffled in first
        var newCenterPile = gameState.CenterPiles[0].Add(ModelGenerator.CreateBasicCard(6));
        var newCenterPiles = gameState.CenterPiles.ReplaceElementAt(0, newCenterPile);
        gameState = gameState with {CenterPiles = newCenterPiles.ToImmutableList()};
        
        // Try to play a card
        gameState = GameEngine.TryPlayCard(gameState, 0, player1.HandCards[0], 0).Data;
        var winnerResult = GameEngine.TryGetWinner(gameState);

        // Assertion
        Assert.Equal(0, winnerResult.Data);
    }
}