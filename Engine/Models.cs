using System.Collections.Immutable;

namespace Engine;

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

    public int Value { get; init; }
    // public Coords? UpdatedCoords { get; init; }
}

public enum CardPileName
{
    Hand,
    Kitty,
    TopUp,
    Center
}

// public struct Coords
// {
//     public int X;
//     public int Y;
// }

public enum Suit
{
    Hearts,
    Diamonds,
    Clubs,
    Spades
}