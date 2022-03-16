namespace Engine.Helpers;

public interface IGameState
{
    public Settings? Settings { get; set; }
    public List<Player> Players { get; set; }
    public List<List<Card>> CenterPiles { get; set; }
    
    
}