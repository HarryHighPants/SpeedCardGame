namespace Engine.UnitTests;

using System.Collections.Generic;
using System.Linq;
using Xunit;

public class PickupCardTests
{
    [Theory]
    [InlineData(new[] {1, 2}, 3, true, 3)]
    [InlineData(new[] {1, 2}, null, false, null)]
    [InlineData(new[] {1, 2, 3, 4, 5}, 3, false, null)]
    public void PickupCard_Theory(int[] player1Cards, int? kittyCard, bool expectedCanPickup,
        int? expectedPickedUpValue)
    {
        // Convert inline data types to usable array
        var nullablePlayer1Cards = player1Cards.Select(i => new int?(i)).ToList();

        // Arrange
        var gameState = ModelGenerator.CreateGameCustom(player1Cards: nullablePlayer1Cards,
            player1Kittys: new List<int?> {kittyCard});

        // Act
        // See if we can pickup
        var canPickupResult = GameEngine.CanPickupFromKitty(gameState, 0);

        // Try to pickup
        var tryPickupResult =
            GameEngine.TryPickupFromKitty(gameState, 0);

        // Assertion
        Assert.Equal(expectedCanPickup, canPickupResult.Success);
        Assert.Equal(expectedCanPickup, tryPickupResult.Success);
        if (expectedCanPickup)
        {
            Assert.Equal((CardValue)expectedPickedUpValue, tryPickupResult.Data.pickedUpCard.CardValue);
            Assert.Single(tryPickupResult.Data.updatedGameState.Players[0].HandCards
                .Where(card => card.CardValue == (CardValue)expectedPickedUpValue));
        }
    }
}
