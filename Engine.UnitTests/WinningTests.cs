namespace Engine.UnitTests;

using Xunit;

public class WinningTests
{
    [Theory]
    [InlineData(1, null, 1, null, true, 0)]
    [InlineData(1, 5, null, null, true, 1)]
    [InlineData(1, 5, 1, null, false, 0)]
    [InlineData(1, null, 1, 9, true, 0)]
    [InlineData(1, 5, null, 9, true, 1)]
    public void Winning_Theory(int centerPile1, int? player1Card, int? player2Card, int? player1Kitty,
        bool expectedWinner, int expectedWinnerIndex)
    {
        // Arrange
        var gameState =
            ModelGenerator.CreateGameBasic(centerPile1, player1Card: player1Card, player2Card: player2Card);

        // Act
        var gameEngine = new GameEngine();
        var winnerResult = gameEngine.TryGetWinner(gameState);

        // Assertion
        Assert.Equal(expectedWinner, winnerResult.Success);
        if (expectedWinner)
        {
            // Check the winner is the expected one
            Assert.Equal(expectedWinnerIndex, winnerResult.Data);
        }
    }
}
