namespace Server.Services;

using System.Linq;
using Hubs;
using Microsoft.EntityFrameworkCore;
using Models.Database;

public class StatService
{
	private readonly IGameService gameService;
	private readonly IBotService botService;
	private readonly IServiceScopeFactory scopeFactory;


	public StatService(IServiceScopeFactory scopeFactory, IGameService gameService, IBotService botService)
	{
		this.scopeFactory = scopeFactory;
		this.gameService = gameService;
		this.botService = botService;
	}

	public async Task HandleGameOver(string roomId)
	{
		Console.WriteLine($"{roomId} has ended");
		var bot = botService.GetBotsInRoom(roomId).FirstOrDefault();
		var gameState = gameService.GetGame(roomId).State;
		var (dbWinner, dbLoser) = await GetWinnerLoser(roomId);
		var cardsRemaining = gameState.Players.Select(p => p.HandCards.Count + p.KittyCards.Count).Sum();

		var dailyGame = bot is {Data.Type: BotType.Daily};
		if (dailyGame)
		{
			dbWinner.DailyWins += 1;
			dbWinner.DailyWinStreak += 1;
			if (dbWinner.MaxDailyWinStreak < dbWinner.DailyWinStreak)
			{
				dbWinner.MaxDailyWinStreak = dbWinner.DailyWinStreak;
			}

			dbLoser.DailyLosses += 1;
			dbLoser.DailyWinStreak = 0;
		}

		dbLoser.Losses += 1;
		dbWinner.Wins += 1;

		// Update the players Elo
		EloService.CalculateElo(ref dbWinner, ref dbLoser);

		using var scope = scopeFactory.CreateScope();
		var gameResultContext = scope.ServiceProvider.GetRequiredService<GameResultContext>();

		gameResultContext.GameResults.Add(new GameResultDao
		{
			Id = Guid.NewGuid(),
			Created = DateTime.UtcNow,
			Turns = gameState.MoveHistory.Count,
			LostBy = cardsRemaining,
			Winner = dbWinner,
			Loser = dbLoser,
			Daily = dailyGame,
			DailyIndex = InMemoryGameService.GetDayIndex()
		});

		await gameResultContext.SaveChangesAsync();
	}

	public DailyResultDto? GetDailyResultDto(Guid persistentPlayerId)
	{
		var dailyMatch = GetDailyResult(persistentPlayerId);
		if (dailyMatch == null)
		{
			return null;
		}

		return new DailyResultDto(persistentPlayerId, dailyMatch);
	}

	public async Task<RankingStatsDto> GetRankingStats(Guid playerId, string roomId)
	{
		var (dbWinner, dbLoser) = await GetWinnerLoser(roomId);
		var player = dbWinner;
		if (dbWinner.Id != playerId)
		{
			player = dbLoser;
		}
		var startingElo = player.Elo;
		EloService.CalculateElo(ref dbWinner, ref dbLoser);
		return new RankingStatsDto(startingElo, player.Elo);

	}

	private async Task<(PlayerDao winner, PlayerDao loser)> GetWinnerLoser(string roomId)
	{
		// Get the gameState from the gameService
		var gameStateResult = gameService.GetGameStateDto(roomId);
		var players = gameService.ConnectionsInRoom(roomId);
		var winner = players.Single(p => p.PlayerId != null && p.ConnectionId == gameStateResult.Data.WinnerId);
		var loser = players.Single(p => p.PlayerId != null && p.ConnectionId != gameStateResult.Data.WinnerId);

		var dbWinner = await GetConnectionsPlayerDao(winner);
		var dbLoser = await GetConnectionsPlayerDao(loser);

		using var scope = scopeFactory.CreateScope();
		var gameResultContext = scope.ServiceProvider.GetRequiredService<GameResultContext>();
		await gameResultContext.SaveChangesAsync();
		return (dbWinner, dbLoser);
	}

	private async Task<PlayerDao> GetConnectionsPlayerDao(Connection connection)
	{
		using var scope = scopeFactory.CreateScope();
		var gameResultContext = scope.ServiceProvider.GetRequiredService<GameResultContext>();
		var player = await gameResultContext.Players.FindAsync(connection.PersistentPlayerId);
		if (player == null)
		{
			player = new PlayerDao {Id = connection.PersistentPlayerId, Name = connection.Name, Elo = EloService.StartingElo};
			gameResultContext.Players.Add(player);
			await gameResultContext.SaveChangesAsync();
		}
		player.Name = connection.Name;
		await gameResultContext.SaveChangesAsync();
		return player;
	}

	private GameResultDao? GetDailyResult(Guid persistentPlayerId)
	{
		using var scope = scopeFactory.CreateScope();
		var gameResultContext = scope.ServiceProvider.GetRequiredService<GameResultContext>();
		return gameResultContext.GameResults
			.Include(gr => gr.Loser)
			.Include(gr => gr.Winner)
			.FirstOrDefault(gr =>
				gr.Daily && gr.DailyIndex == InMemoryGameService.GetDayIndex() &&
				(gr.Winner.Id == persistentPlayerId || gr.Loser.Id == persistentPlayerId));
	}
}
