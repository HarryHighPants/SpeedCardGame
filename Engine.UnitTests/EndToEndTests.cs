using System.Collections.Generic;
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
            var move1 =
                BotRunner.MakeMove(gameState, 0);
            
            gameState = move1.Success ? move1.Data.updatedGameState : gameState;
            
            var move2 =
                BotRunner.MakeMove(gameState, 1);
            gameState = move2.Success ? move2.Data.updatedGameState : gameState;


            // Debugging
            if (move1 is not IErrorResult move1Error && !string.IsNullOrEmpty(move2.Data.moveMade))
            {
                _testOutputHelper.WriteLine($"{gameState.Players[0].Name} {move1.Data.moveMade}");
            }

            if (move1 is not IErrorResult move2Error && !string.IsNullOrEmpty(move2.Data.moveMade))
            {
                _testOutputHelper.WriteLine($"{gameState.Players[1].Name} {move2.Data.moveMade}");
            }

            movesMade++;
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

        Player player1 = gameState.Players[0];
        Player player2 = gameState.Players[1];

        // Act
        gameState = GameEngine.TryRequestTopUp(gameState, 1).Data.updatedGameState;
        gameState = GameEngine.TryPlayCard(gameState, 0, player1.HandCards[0], 0).Data;
        gameState = GameEngine.TryPickupFromKitty(gameState, 0).Data.updatedGameState;
        gameState = GameEngine.TryRequestTopUp(gameState, 0).Data.updatedGameState;
        gameState = GameEngine.TryPlayCard(gameState, 1, player2.HandCards[0], 0).Data;
        gameState = GameEngine.TryPlayCard(gameState, 0, player1.HandCards[0], 0).Data;
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
        gameState = GameEngine.TryRequestTopUp(gameState, 1).Data.updatedGameState;
        gameState = GameEngine.TryRequestTopUp(gameState, 0).Data.updatedGameState;
        Assert.Single(gameState.CenterPiles[0]);

        // Manually add a 6 to ensure the 6 "was" shuffled in first
        gameState.CenterPiles[0].Add(ModelGenerator.CreateBasicCard(6));
        gameState = GameEngine.TryPlayCard(gameState, 0, player1.HandCards[0], 0).Data;
        var winnerResult = GameEngine.TryGetWinner(gameState);

        // Assertion
        Assert.Equal(0, winnerResult.Data);
    }
}