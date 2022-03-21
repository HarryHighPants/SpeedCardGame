namespace Engine;

public record Settings
{
    public int MaxHandCards { get; init; } = 5;
    public int? RandomSeed { get; init; } = null;
}

public record GameState
{
    public Settings? Settings { get; init; }
    public List<Player> Players { get; init; } = new(0);
    public List<List<Card>> CenterPiles { get; init; } = new(0);
}

public record Player
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public List<Card> HandCards { get; init; } = new(0);
    public List<Card> KittyCards { get; init; } = new(0);
    public List<Card> TopUpCards { get; init; } = new(0);
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