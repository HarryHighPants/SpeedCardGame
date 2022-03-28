namespace Server;

using Engine;
using Engine.Models;

public class WebGame : Game
{
    public WebGame(List<string>? playerNames = null, Settings? settings = null, GameEngine? gameEngine = null) : base(
        playerNames, settings, gameEngine)
    {
    }
}
