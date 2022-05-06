namespace Server.Services;

using System.Collections.Concurrent;
using System.Linq;
using Engine;
using Engine.Helpers;
using Engine.Models;
using Hubs;
using Microsoft.AspNetCore.SignalR;
using Models.Database;
using Newtonsoft.Json;

public class BotService : IBotService
{
	public record Bot(string ConnectionId, WebBotData Data, string roomId);

	private ConcurrentDictionary<string, Bot> Bots = new ConcurrentDictionary<string, Bot>();

	private ConcurrentDictionary<string, CancellationTokenSource> BotRunners =
		new ConcurrentDictionary<string, CancellationTokenSource>();

	private readonly IHubContext<GameHub> hubContext;
	private readonly IGameService gameService;
	private readonly IServiceScopeFactory scopeFactory;

	public BotService(IServiceScopeFactory scopeFactory, IGameService gameService, IHubContext<GameHub> hubContext)
	{
		this.gameService = gameService;
		this.hubContext = hubContext;
		this.scopeFactory = scopeFactory;
	}

	public async Task AddBotToRoom(string roomId, BotType type)
	{
		var botData = BotConfigurations.GetBot(type);
		await SeedBot(botData);
		var botId = Guid.NewGuid().ToString();
		var bot = new Bot(botId, botData, roomId);
		gameService.JoinRoom(roomId, botId, botData.PersistentId);
		gameService.UpdateName(bot.Data.Name, botId);
		Bots.TryAdd(botId, bot);
	}

	public List<Bot> GetBotsInRoom(string roomId) =>
		Bots.Where(b => b.Value.roomId == roomId).Select(b => b.Value).ToList();

	public int BotsInRoomCount(string roomId) => GetBotsInRoom(roomId).Count;

	public void RunBotsInRoom(string roomId)
	{
		var botsInRoom = GetBotsInRoom(roomId);
		foreach (var bot in botsInRoom)
		{
			var cancellationSource = new CancellationTokenSource();
			BotRunners.TryAdd(bot.ConnectionId, cancellationSource);
			RunBot(bot, cancellationSource.Token).ContinueWith((task, _)=>{
				if (task.Exception != null)
				{
					Console.WriteLine(task.Exception);
				}}, cancellationSource);
		}
	}

	public void RemoveBotsFromRoom(string roomId)
	{
		var botsInRoom = GetBotsInRoom(roomId);
		foreach (var bot in botsInRoom)
		{
			gameService.LeaveRoom(roomId, bot.ConnectionId);
			Bots.Remove(bot.ConnectionId, out _);

			if (BotRunners.ContainsKey(bot.ConnectionId))
			{
				BotRunners[bot.ConnectionId].Cancel();
				BotRunners.Remove(bot.ConnectionId, out _);
			}
		}
	}

	private async Task SeedBot(WebBotData botData)
	{
		using var scope = scopeFactory.CreateScope();
		var gameResultContext = scope.ServiceProvider.GetRequiredService<GameResultContext>();

		var bot = await gameResultContext.Players.FindAsync(botData.PersistentId);
		if (bot == null)
		{
			bot = new PlayerDao {Id = botData.PersistentId, Name = botData.Name, IsBot = true};
			gameResultContext.Players.Add(bot);
		}
		bot.Elo = (int)botData.Elo;
		await gameResultContext.SaveChangesAsync();
	}


	private async Task RunBot(Bot bot, CancellationToken cancellationToken)
	{
		var random = new Random();

		var checks = gameService.GetGame(bot.roomId).gameEngine.Checks;
		var playerId = gameService.GetConnectionInfo(bot.ConnectionId).PlayerId;
		if (playerId == null)
		{
			Console.WriteLine("Bot has no playerId so can't participate in game");
			return;
		}

		// Wait for the cards to animate in
		await Task.Delay(4000, cancellationToken);
		var centerPilesAvailable = new List<bool> {true, true};
		while (!cancellationToken.IsCancellationRequested &&
		       gameService.GetGameStateDto(bot.roomId).Data.WinnerId == null)
		{
			var move = BotRunner.GetMove(checks, GetGameState(bot.roomId), playerId.Value, centerPilesAvailable);
			if (move.Success)
			{
				var result = move.Data.Type switch
				{
					MoveType.PlayCard => gameService.TryPlayCard(bot.ConnectionId, move.Data.CardId.Value,
						move.Data.CenterPileIndex.Value),
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

			// Wait if we have just topped up
			var gameState = GetGameState(bot.roomId);
			if (gameState.MoveHistory.Count > 0 && gameState.MoveHistory.Last().Type == MoveType.TopUp)
			{
				await Task.Delay(1500, cancellationToken);
			}

			var turnDelay = random.Next((int)bot.Data.QuickestResponseTimeMs, (int)bot.Data.SlowestResponseTimeMs);
			if (move.Success && move.Data.Type == MoveType.PickupCard)
			{
				turnDelay = (int)bot.Data.PickupIntervalMs;
			}
			var centerTotals = GetCenterPileTotals(bot.roomId);
			await Task.Delay(turnDelay, cancellationToken);

			// If a pile was updated while the bot was thinking then it can't be played onto
			var updatedCenterTotals = GetCenterPileTotals(bot.roomId);
			centerPilesAvailable = centerTotals.Select((ct, i) => ct == updatedCenterTotals[i]).ToList();
		}
	}

	private GameState GetGameState(string roomId) => gameService.GetGame(roomId).State;

	private List<int> GetCenterPileTotals(string roomId) => GetGameState(roomId).CenterPiles.Select(cp => cp.Cards.Count).ToList();

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
