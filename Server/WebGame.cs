namespace Server;

using System.Collections.Concurrent;
using System.Linq;
using Engine;
using Engine.Helpers;
using Engine.Models;

public class WebGame : Game
{
	private UpdateGameState updateGameState;
	// private List<BotPlayer> bots;

	private List<(int playerId, BotData data)> bots = new List<(int, BotData)>();

	public delegate Task UpdateGameState();



    public WebGame(List<KeyValuePair<string,Server.Connection>> connections, Settings? settings = null, GameEngine? gameEngine = null)
    {
	    Initialise(connections.Select(c=>c.Value.name).ToList(), settings, gameEngine);

	    // Add any bots
	    for (var i = 0; i < connections.Count; i++)
	    {
		    var connection = connections[i];
		    if (connection.Value.isBot)
		    {
			    bots.Add((i, connection.Value.botData));
		    }
	    }

	    this.updateGameState = updateGameState;
    }

    public async Task RunBots(UpdateGameState updateGameState)
    {
	    var random = new Random();
	    while (gameEngine.Checks.TryGetWinner(State).Failure)
	    {
		    for (var i = 0; i < bots.Count; i++)
		    {
			    await Task.Delay(random.Next(bots[i].data.QuickestResponseTimeMs, bots[i].data.SlowestResponseTimeMs));
			    var move = BotRunner.MakeMove(this, bots[i].playerId);
			    if (move is IErrorResult)
			    {
				    continue;
			    }
			    await updateGameState();
		    }
	    }
    }




}
