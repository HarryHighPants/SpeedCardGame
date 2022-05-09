namespace Server;

using System.Collections.Concurrent;
using System.Linq;
using Engine;
using Engine.Helpers;
using Engine.Models;

public class WebGame : Game
{
    public WebGame(List<GameParticipant> connections, Settings settings, GameEngine gameEngine)
        : base(connections.Select(c => c.Name).ToList(), settings, gameEngine) { }
}
