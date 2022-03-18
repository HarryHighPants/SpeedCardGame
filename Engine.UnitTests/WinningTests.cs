using Engine.Helpers;
using Xunit;

namespace Engine.UnitTests;

public class WinningTests
{
    [Theory]
    [InlineData(1, null, 1, true, 0)]
    [InlineData(1, 5, null, true, 1)]
    [InlineData(1, 5, 1, false, 0)]
    public void Winning_Theory(int centerPile1, int? player1Card, int? player2Card,
        bool expectedWinner, int expectedWinnerIndex)
    {
        // Arrange
        GameState gameState =
            ScenarioHelper.CreateGameBasic(centerPile1, player1Card: player1Card, player2Card: player2Card);

        // Act
        Result<Player> winnerResult = GameEngine.TryGetWinner(gameState);

        // Assertion
        Assert.Equal(expectedWinner, winnerResult.Success);
        if (expectedWinner)
        {
            // Check the winner is the expected one
            Assert.Equal(expectedWinnerIndex, gameState.Players.IndexOf(winnerResult.Data));
        }
    }
}