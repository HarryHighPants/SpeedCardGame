using System.Linq;
using Engine.Helpers;
using Xunit;

namespace Engine.UnitTests;

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
        GameState gameState = ModelGenerator.CreateGameBasic(centerCard, player1Card: player1Card);

        // Act
        // See if we have a play
        Result<(Card card, int centerPile)> hasPlayResult = GameEngine.PlayerHasPlay(gameState, gameState.Players[0]);

        // Try to play it
        Result<GameState> tryPlayResult =
            GameEngine.TryPlayCard(gameState, gameState.Players[0], gameState.Players[0].HandCards[0], 0);

        // Assertion
        Assert.Equal(expectedCanPlay, hasPlayResult.Success);
        Assert.Equal(expectedCanPlay, tryPlayResult.Success);
        if (expectedCanPlay)
        {
            // Check the card has been added to the center pile
            Assert.Equal(player1Card, tryPlayResult.Data.CenterPiles[0].Last().Value);

            // Check the card has been removed from players hand
            Assert.DoesNotContain(tryPlayResult.Data.Players[0].HandCards, card => card.Value == player1Card);
        }
    }

    [Fact]
    public void PlayCard_NotOwned_False()
    {
        // Arrange
        GameState gameState = ModelGenerator.CreateGameBasic(5, player1Card: 4, player2Card: 6);

        // Try to play it
        Result<GameState> tryPlayResult =
            GameEngine.TryPlayCard(gameState, gameState.Players[0], gameState.Players[1].HandCards[0], 0);

        // Assertion
        Assert.True(tryPlayResult.Failure);
        Assert.Equal("Player Player 1 does not have card 6 in their hand", (tryPlayResult as IErrorResult)?.Message);
    }

    [Fact]
    public void PlayCard_NotExist_False()
    {
        // Arrange
        GameState gameState = ModelGenerator.CreateGameBasic(5, player1Card: 4, player2Card: 6);
        var randomCard = new Card {Id = 0, Suit = Suit.Clubs, Value = 4};

        // Try to play it
        Result<GameState> tryPlayResult =
            GameEngine.TryPlayCard(gameState, gameState.Players[0], randomCard, 0);

        // Assertion
        Assert.True(tryPlayResult.Failure);
        Assert.Equal("Card not found in gameState", (tryPlayResult as IErrorResult)?.Message);
    }
}