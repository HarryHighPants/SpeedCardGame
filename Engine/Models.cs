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
    public ImmutableList<ImmutableList<Card>> CenterPiles { get; init; }
    public ImmutableList<MoveData> MoveHistory { get; init; }
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

public record Card
{
    public int Id { get; init; }
    public Suit Suit { get; init; }
    public CardValue CardValue { get; init; }
}

public enum CardValue
{
    Two = 0,
    Three = 1,
    Four = 2,
    Five = 3,
    Six = 4,
    Seven = 5,
    Eight = 6,
    Nine = 7,
    Ten = 8,
    Jack = 9,
    Queen = 10,
    King = 11,
    Ace = 12
}

public enum CardPileName
{
    Hand,
    Kitty,
    TopUp,
    Center
}

public enum Suit
{
    Hearts,
    Diamonds,
    Clubs,
    Spades
}
