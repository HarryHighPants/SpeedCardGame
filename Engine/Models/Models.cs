namespace Engine;

using System.Collections.Immutable;

public record Settings
{
    public int MaxHandCards { get; init; } = 5;
    public int? RandomSeed { get; init; } = null;
}

public record GameState
{
    public Settings? Settings { get; init; }
    public ImmutableList<Player> Players { get; init; }
    public ImmutableList<CenterPile> CenterPiles { get; init; }
    public ImmutableList<MoveData> MoveHistory { get; init; }
}

public record CenterPile
{
    public ImmutableList<Card> Cards = ImmutableList<Card>.Empty;
}

public enum MoveType
{
    PlayCard, PickupCard, TopUp
}

public record MoveData
{
    public int? CardId;
    public int? CenterPileIndex;
    public MoveType Move;
    public int? PlayerId;
}

public record Player
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public ImmutableList<Card> HandCards { get; init; }
    public ImmutableList<Card> KittyCards { get; init; }
    public ImmutableList<Card> TopUpCards { get; init; }
    public bool RequestingTopUp { get; init; }
}

public enum CardPileName
{
    Hand,
    Kitty,
    TopUp,
    Center
}
