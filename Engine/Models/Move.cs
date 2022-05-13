namespace Engine.Models;

using System.Diagnostics.CodeAnalysis;
using System.Linq;

public record Move(
    MoveType Type,
    int? PlayerId = null,
    int? CardId = null,
    int? CenterPileIndex = null
)
{
    public string GetDescription(
        GameState gameState,
        bool minified = false,
        bool includeSuit = false
    ) =>
        Type switch
        {
            MoveType.PickupCard
              => $"{gameState.GetPlayer(PlayerId)?.Name} picked up card {Card.ToString(gameState, CardId, minified, includeSuit)}",
            MoveType.PlayCard
              => $"{gameState.GetPlayer(PlayerId)?.Name} played card {Card.ToString(gameState, CardId, minified, includeSuit)} onto {Card.ToString(gameState, gameState.CenterPiles[CenterPileIndex.Value].Cards.Reverse().Skip(1).Take(1).ToList()[0].Id, minified, includeSuit)}",
            MoveType.TopUp => "Center cards were topped up",
            MoveType.RequestTopUp => $"{gameState.GetPlayer(PlayerId)?.Name} requested top up",
            _ => throw new ArgumentOutOfRangeException()
        };
}

public enum MoveType
{
    PlayCard,
    PickupCard,
    RequestTopUp,
    TopUp
}
