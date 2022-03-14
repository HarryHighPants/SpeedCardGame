namespace Engine;

public class Settings
{
    public int MaxHandCards { get; set; } = 5;
}

public class GameState
{
    public Settings? Settings { get; set; }
    public List<Player> Players { get; set; }
    public List<List<Card>> CenterPiles { get; set; }
}

public class Player
{

    public int Id { get; set; }
    public string Name { get; set; }
    public List<Card> HandCards { get; set; }
    public List<Card> KittyCards { get; set; }
    
    public List<Card> TopupCards { get; set; }
}

public class Card
{
    public int Id { get; set; }
    public Suit Suit { get; set; }
    public int Value { get; set; }
    public Coords? UpdatedCoords { get; set; }
}

public enum CardPile
{
    Hand, Kitty, Topup, Center
}

public struct Coords
{
    public int X;
    public int Y;
}

public enum Suit
{
    Hearts, Diamonds, Clubs, Spades
}
