namespace Engine.Models;

public record Move
{
    public int? CardId;
    public int? CenterPileIndex;
    public int? PlayerId;
    public MoveType Type;

    public string GetDescription(GameState gameState, bool minified = false, bool includeSuit = false) =>
        this.Type switch
        {
            MoveType.PickupCard =>
                $"{gameState.GetPlayer(this.PlayerId)?.Name} picked up card {Card.ToString(gameState, this.CardId, minified, includeSuit)}",
            MoveType.PlayCard =>
                $"{gameState.GetPlayer(this.PlayerId)?.Name} played card {Card.ToString(gameState, this.CardId, minified, includeSuit)} onto pile {this.CenterPileIndex + 1}",
            MoveType.TopUp => "Center cards were topped up",
            MoveType.RequestTopUp => $"{gameState.GetPlayer(this.PlayerId)?.Name} requested top up",
            _ => throw new ArgumentOutOfRangeException()
        };
}

public enum MoveType
{
    PlayCard, PickupCard, RequestTopUp, TopUp
}
