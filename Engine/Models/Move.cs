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
                $"{Player.Get(gameState, this.PlayerId).Name} picked up card {Card.ToString(gameState, this.CardId, minified, includeSuit)}",
            MoveType.PlayCard =>
                $"{Player.Get(gameState, this.PlayerId).Name} played card {Card.ToString(gameState, this.CardId, minified, includeSuit)} onto pile {this.CenterPileIndex + 1}",
            MoveType.TopUp => "Center cards were topped up",
            _ => throw new ArgumentOutOfRangeException()
        };
}

public enum MoveType
{
    PlayCard, PickupCard, TopUp
}
