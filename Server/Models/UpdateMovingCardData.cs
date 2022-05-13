using Engine.Models;

namespace Server;

public record UpdateMovingCardData
{
    public int CardId { get; set; }
    public Pos? Pos { get; set; }
}
