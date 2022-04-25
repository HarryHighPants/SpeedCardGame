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

	public void AddBotToRoom(string roomId, BotType type)
	{
		var botData = BotConfigurations.GetBot(type);
		var botId = Guid.NewGuid().ToString();
		var bot = new Bot(botId, botData, roomId);
		gameService.JoinRoom(roomId, botId);
		gameService.UpdateName(bot.Data.Name, botId);
		Bots.TryAdd(botId, bot);
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
			if (BotRunners.ContainsKey(bot.ConnectionId))
			{
				gameService.LeaveRoom(roomId, bot.ConnectionId);
				BotRunners[bot.ConnectionId].Cancel();
				BotRunners.Remove(bot.ConnectionId, out _);
				Bots.Remove(bot.ConnectionId, out _);
			}
		}
	}

	private List<Bot> GetBotsInRoom(string roomId) => Bots.Where(b => b.Value.roomId == roomId).Select(b=>b.Value).ToList();

	private async Task RunBot(Bot bot, CancellationToken cancellationToken)
	{
		var random = new Random();

		var checks = gameService.GetGame(bot.roomId).gameEngine.Checks;
		var playerId = gameService.GetConnectionsPlayer(bot.ConnectionId).PlayerId;
		// Wait for the cards to animate in
		await Task.Delay(5000, cancellationToken);
		while (!cancellationToken.IsCancellationRequested || gameService.GetGameStateDto(bot.roomId).Data.WinnerId == null)
		{
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
				var turnDelay = random.Next(bot.Data.QuickestResponseTimeMs, bot.Data.SlowestResponseTimeMs);
				if (move.Data.Type == MoveType.PickupCard)
				{
					turnDelay = bot.Data.PickupIntervalMs;
				}
				await Task.Delay(turnDelay, cancellationToken);
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
