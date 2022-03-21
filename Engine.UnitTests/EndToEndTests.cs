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
        while (GameEngine.TryGetWinner(gameState).Failure)
        {
            Result<(GameState updatedGameState, string moveMade)> move1 =
                BotRunner.MakeMove(gameState, gameState.Players[0]);
            Result<(GameState updatedGameState, string moveMade)> move2 =
                BotRunner.MakeMove(gameState, gameState.Players[1]);

            // Debugging
            if (!string.IsNullOrEmpty(move1.Data.moveMade))
            {
                _testOutputHelper.WriteLine($"{gameState.Players[0].Name} {move1.Data.moveMade}");
            }

            if (!string.IsNullOrEmpty(move2.Data.moveMade))
            {
                _testOutputHelper.WriteLine($"{gameState.Players[1].Name} {move2.Data.moveMade}");
            }

            movesMade++;
        }

        Result<Player> winnerResult = GameEngine.TryGetWinner(gameState);
        _testOutputHelper.WriteLine($"Bot game complete with {winnerResult.Data.Name} winning in {movesMade} moves");
        Assert.True(GameEngine.TryGetWinner(gameState).Success);
    }

    [Fact]
    public void TopUpExample()
    {
        // Arrange
        GameState gameState =
            ScenarioHelper.CreateGameCustom(
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
        Assert.True(GameEngine.TryRequestTopUp(gameState, player2).Success);
        Assert.True(GameEngine.TryPlayCard(gameState, player1, player1.HandCards[0], 0).Success);
        Assert.True(GameEngine.TryPickupFromKitty(gameState, player1).Success);
        Assert.True(GameEngine.TryRequestTopUp(gameState, player1).Success);
        Assert.True(GameEngine.TryPlayCard(gameState, player2, player2.HandCards[0], 0).Success);
        Assert.True(GameEngine.TryPlayCard(gameState, player1, player1.HandCards[0], 0).Success);
        Result<Player> winnerResult = GameEngine.TryGetWinner(gameState);

        // Assertion
        Assert.Equal(player1, winnerResult.Data);
    }

    [Fact]
    public void ReplenishExample()
    {
        // Arrange
        GameState gameState =
            ScenarioHelper.CreateGameCustom(
                new List<int?> {6, 6, 1},
                new List<int?> {6, 6, 1},
                new List<int?> {5},
                player2Cards: new List<int?> {5}
            );

        Player player1 = gameState.Players[0];
        Player player2 = gameState.Players[1];

        // Act
        Assert.True(GameEngine.TryRequestTopUp(gameState, player2).Success);
        Assert.True(GameEngine.TryRequestTopUp(gameState, player1).Success);
        Assert.Single(gameState.CenterPiles[0]);

        // Manually add a 6 to ensure the 6 "was" shuffled in first
        gameState.CenterPiles[0].Add(ScenarioHelper.CreateBasicCard(6));
        Assert.True(GameEngine.TryPlayCard(gameState, player1, player1.HandCards[0], 0).Success);
        Result<Player> winnerResult = GameEngine.TryGetWinner(gameState);

        // Assertion
        Assert.Equal(player1, winnerResult.Data);
    }
}