namespace Server;

using System.Collections.Concurrent;
using System.Linq;
using Engine;
using Engine.Helpers;
using Engine.Models;

public class WebGame : Game
{
	public WebGame(List<KeyValuePair<string,Server.Connection>> connections, Settings? settings = null, GameEngine? gameEngine = null)
    {
	    Initialise(connections.Select(c=>c.Value.Name).ToList(), settings, gameEngine);
    }
}
