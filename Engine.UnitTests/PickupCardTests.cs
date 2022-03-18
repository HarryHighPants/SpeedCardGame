using System.Collections.Generic;
using System.Linq;
using Engine.Helpers;
using Xunit;

namespace Engine.UnitTests;

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
        List<int?> nullablePlayer1Cards = player1Cards.Select(i => new int?(i)).ToList();

        // Arrange
        GameState gameState = ScenarioHelper.CreateGameCustom(player1Cards: nullablePlayer1Cards,
            player1Kittys: new List<int?> {kittyCard});

        // Act
        // See if we can pickup
        Result canPickupResult = GameEngine.CanPickupFromKitty(gameState, gameState.Players[0]);

        // Try to pickup
        Result<(GameState updatedGameState, Card pickedUpCard)> tryPickupResult =
            GameEngine.TryPickupFromKitty(gameState, gameState.Players[0]);

        // Assertion
        Assert.Equal(expectedCanPickup, canPickupResult.Success);
        Assert.Equal(expectedCanPickup, tryPickupResult.Success);
        if (expectedCanPickup)
        {
            Assert.Equal(expectedPickedUpValue, tryPickupResult.Data.pickedUpCard.Value);
            Assert.Single(tryPickupResult.Data.updatedGameState.Players[0].HandCards
                .Where(card => card.Value == expectedPickedUpValue));
        }
    }
}