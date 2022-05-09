namespace Server;

public record UpdateMovingCardData
{
    public int CardId { get; set; }
    public Pos? Pos { get; set; }
    public int Location { get; set; }
    public int Index { get; set; }
}
