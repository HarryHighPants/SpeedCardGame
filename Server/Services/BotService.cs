namespace Server.Services;

using System.Collections.Concurrent;
using System.Linq;
using Engine;
using Engine.Helpers;
using Engine.Models;
using Hubs;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

public class BotService : IBotService
{
	public record Bot(string ConnectionId, BotData Data, string roomId);

	private ConcurrentDictionary<string, Bot> Bots = new ConcurrentDictionary<string, Bot>();
	private ConcurrentDictionary<string, CancellationTokenSource> BotRunners = new ConcurrentDictionary<string, CancellationTokenSource>();

	private readonly IHubContext<GameHub> hubContext;
	private readonly IGameService gameService;

	public BotService(IGameService gameService, IHubContext<GameHub> hubContext)
	{
		this.gameService = gameService;
		this.hubContext = hubContext;
	}

	public void AddBotToRoom(string roomId, BotDifficulty difficulty)
	{
		var botId = Guid.NewGuid().ToString();
		gameService.JoinRoom(roomId, botId);
		var botData = BotConfigurations.GetBot(difficulty);
		Bots.TryAdd(botId, new Bot(botId, botData, roomId));
	}

	public int BotsInRoomCount(string roomId) => GetBotsInRoom(roomId).Count;

	public void RunBotsInRoom(string roomId)
	{
		var botsInRoom = GetBotsInRoom(roomId);
		foreach (var bot in botsInRoom)
		{
			var cancellationSource = new CancellationTokenSource();
			RunBot(bot, cancellationSource.Token);
			BotRunners.TryAdd(bot.ConnectionId, cancellationSource);
		}
	}

	public void RemoveBotsFromRoom(string roomId)
	{
		var botsInRoom = GetBotsInRoom(roomId);
		foreach (var bot in botsInRoom)
		{
			gameService.LeaveRoom(roomId, bot.ConnectionId);
			BotRunners[bot.ConnectionId].Cancel();
			BotRunners.Remove(bot.ConnectionId, out _);
			Bots.Remove(bot.ConnectionId, out _);
		}
	}

	private List<Bot> GetBotsInRoom(string roomId) => Bots.Where(b => b.Value.roomId == roomId).Select(b=>b.Value).ToList();

	private async Task RunBot(Bot bot, CancellationToken cancellationToken)
	{
		var random = new Random();

		var checks = gameService.GetGame(bot.roomId).gameEngine.Checks;
		var playerId = gameService.GetConnectionsPlayer(bot.ConnectionId).PlayerId;
		while (!cancellationToken.IsCancellationRequested || gameService.GetGameStateDto(bot.roomId).Data.WinnerId == null)
		{
				await Task.Delay(random.Next(bot.Data.QuickestResponseTimeMs, bot.Data.SlowestResponseTimeMs));

				if (playerId == null)
				{
					continue;
				}
				var move = BotRunner.GetMove(checks, gameService.GetGame(bot.roomId).State, playerId.Value);
				if (move is IErrorResult)
				{
					continue;
				}

				var result =  move.Data.Type switch
				{
					MoveType.PlayCard => gameService.TryPlayCard(bot.ConnectionId, move.Data.CardId.Value, move.Data.CenterPileIndex.Value),
					MoveType.PickupCard => gameService.TryPickupFromKitty(bot.ConnectionId),
					MoveType.RequestTopUp => gameService.TryRequestTopUp(bot.ConnectionId),
					_ => throw new ArgumentOutOfRangeException()
				};

				if (result is IErrorResult moveError)
				{
					throw new Exception(moveError.Message);
				}
				await SendGameState(bot.roomId);
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
