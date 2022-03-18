namespace Engine;

[Serializable]
public class Settings
{
    public int MaxHandCards { get; set; } = 5;
    public int? RandomSeed { get; set; }
}

[Serializable]
public class GameState
{
    public Settings? Settings { get; set; }
    public List<Player> Players { get; set; }
    public List<List<Card>> CenterPiles { get; set; }
}

[Serializable]
public class Player
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Card> HandCards { get; set; }
    public List<Card> KittyCards { get; set; }
    public List<Card> TopUpCards { get; set; }
    public bool RequestingTopUp { get; set; }
}

[Serializable]
public class Card
{
    public int Id { get; set; }
    public Suit Suit { get; set; }
    public int Value { get; set; }
    public Coords? UpdatedCoords { get; set; }
}

[Serializable]
public enum CardPileName
{
    Hand,
    Kitty,
    TopUp,
    Center
}

[Serializable]
public struct Coords
{
    public int X;
    public int Y;
}

[Serializable]
public enum Suit
{
    Hearts,
    Diamonds,
    Clubs,
    Spades
}