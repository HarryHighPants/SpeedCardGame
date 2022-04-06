namespace Server.Services;

using System.Collections.Concurrent;
using Engine;
using Engine.Helpers;
using Hubs;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

public class BotService
{
	public record Bot(string ConnectionId, BotData Data);

	private ConcurrentDictionary<string, Task> BotRunners;

	private readonly IHubContext<GameHub> hubContext;
	private readonly IGameService gameService;

	public BotService(IGameService gameService, IHubContext<GameHub> hubContext)
	{
		this.gameService = gameService;
		this.hubContext = hubContext;
	}

	// Start bot for room (RoomId)

	public void AddBotToRoom(string roomId, BotDifficulty difficulty)
	{
		var botId = Guid.NewGuid().ToString();
		gameService.JoinRoom(roomId, botId);
		var botData = BotConfigurations.GetBot(difficulty);
		var bot = new Bot(botId, botData);
		BotRunners.TryAdd(roomId, RunBots(roomId, new List<Bot> {bot}));
	}

	public async Task RunBots(string roomId, List<Bot> bots)
	{
		var random = new Random();
		while (gameService.GetGameStateDto(roomId).Data.WinnerId == null)
		{
			for (var i = 0; i < bots.Count; i++)
			{
				await Task.Delay(random.Next(bots[i].Data.QuickestResponseTimeMs, bots[i].Data.SlowestResponseTimeMs));
				var move = BotRunner.MakeMove(gameService, bots[i].playerId);
				if (move is IErrorResult)
				{
					continue;
				}

				SendGameState()
			}
		}
	}

	private async Task SendGameState(string roomId)
	{
		if (!gameService.GameStarted(roomId))
		{
			return;
		}

		// Get the gameState from the gameService
		var gameStateResult = gameService.GetGameStateDto(roomId);

		// Check it's valid
		if (gameStateResult is IErrorResult gameStateError)
		{
			throw new HubException(gameStateError.Message, new ApplicationException(gameStateError.Message));
		}

		// Send the gameState to the roomId
		var jsonData = JsonConvert.SerializeObject(gameStateResult.Data);
		await hubContext.Clients.Group(roomId).SendAsync("UpdateGameState", jsonData);
	}
}
