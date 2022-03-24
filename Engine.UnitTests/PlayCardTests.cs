namespace Engine.UnitTests;

using System.Linq;
using Helpers;
using Xunit;

public class PlayCardTests
{
    [Theory]
    [InlineData(0, 1, true)]
    [InlineData(1, 0, true)]
    [InlineData(11, 12, true)]
    [InlineData(0, 12, true)]
    [InlineData(12, 0, true)]
    [InlineData(1, 3, false)]
    [InlineData(3, 1, false)]
    [InlineData(12, 1, false)]
    [InlineData(1, 12, false)]
    [InlineData(11, 0, false)]
    [InlineData(0, 11, false)]
    [InlineData(-1, 0, false)]
    [InlineData(0, -1, false)]
    public void PlayCard_Theory(int centerCard, int player1Card, bool expectedCanPlay)
    {
        // Arrange
        var gameState = ModelGenerator.CreateGameBasic(centerCard, player1Card: player1Card);

        // Act
        // See if we have a play
        var hasPlayResult = GameEngine.PlayerHasPlay(gameState, 0);

        // Try to play it
        var tryPlayResult =
            GameEngine.TryPlayCard(gameState, 0, gameState.Players[0].HandCards[0], 0);

        // Assertion
        Assert.Equal(expectedCanPlay, hasPlayResult.Success);
        Assert.Equal(expectedCanPlay, tryPlayResult.Success);
        if (expectedCanPlay)
        {
            // Check the card has been added to the center pile
            var immutableList = tryPlayResult.Data.CenterPiles[0].Cards;
            if (immutableList != null)
            {
                Assert.Equal((CardValue)player1Card, immutableList.Last().CardValue);
            }

            // Check the card has been removed from players hand
            Assert.DoesNotContain(tryPlayResult.Data.Players[0].HandCards,
                card => card.CardValue == (CardValue)player1Card);
        }
    }

    [Fact]
    public void PlayCard_NotOwned_False()
    {
        // Arrange
        var gameState = ModelGenerator.CreateGameBasic(5, player1Card: 4, player2Card: 6);

        // Try to play it
        var tryPlayResult =
            GameEngine.TryPlayCard(gameState, 0, gameState.Players[1].HandCards[0], 0);

        // Assertion
        Assert.True(tryPlayResult.Failure);
        Assert.Equal("Player Player 1 does not have card Eight in their hand", (tryPlayResult as IErrorResult)?.Message);
    }

    [Fact]
    public void PlayCard_NotExist_False()
    {
        // Arrange
        var gameState = ModelGenerator.CreateGameBasic(5, player1Card: 4, player2Card: 6);
        var randomCard = new Card {Id = 0, Suit = Suit.Clubs, CardValue = (CardValue)4};

        // Try to play it
        var tryPlayResult =
            GameEngine.TryPlayCard(gameState, 0, randomCard, 0);

        // Assertion
        Assert.True(tryPlayResult.Failure);
        Assert.Equal("Card not found in gameState", (tryPlayResult as IErrorResult)?.Message);
    }
}
