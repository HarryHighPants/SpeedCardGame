namespace Server.Services;

using System.Linq;
using Engine.Models;
using Hubs;
using Microsoft.EntityFrameworkCore;
using Models.Database;

public class StatService
{
    private readonly IServiceScopeFactory scopeFactory;

    public StatService(
        IServiceScopeFactory scopeFactory,
        IGameService gameService,
        IBotService botService
    )
    {
        this.scopeFactory = scopeFactory;
    }

    public async Task UpdateGameOverStats(
        GameState gameState,
        List<GameParticipant> participants,
        bool dailyGame
    )
    {
        Console.WriteLine($"Game has ended");
        var winner = await GetPlayerDao(participants[0]);
        var loser = await GetPlayerDao(participants[1]);

        SetWinsAndLosses(winner, loser, dailyGame);
        EloService.SetPlayerElo(ref winner, ref loser);

        using var scope = scopeFactory.CreateScope();
        var gameResultContext = scope.ServiceProvider.GetRequiredService<GameResultContext>();

        var cardsRemaining = gameState.Players
            .Select(p => p.HandCards.Count + p.KittyCards.Count)
            .Sum();
        gameResultContext.GameResults.Add(
            new GameResultDao
            {
                Id = Guid.NewGuid(),
                Created = DateTime.UtcNow,
                Turns = gameState.MoveHistory.Count,
                LostBy = cardsRemaining,
                Winner = winner,
                Loser = loser,
                Daily = dailyGame,
                DailyIndex = InMemoryGameService.GetDayIndex()
            }
        );

        await gameResultContext.SaveChangesAsync();
    }

    public void SetWinsAndLosses(PlayerDao winner, PlayerDao loser, bool dailyGame)
    {
        loser.Losses += 1;
        winner.Wins += 1;

        if (dailyGame)
        {
            winner.DailyWins += 1;
            winner.DailyWinStreak += 1;
            if (winner.MaxDailyWinStreak < winner.DailyWinStreak)
            {
                winner.MaxDailyWinStreak = winner.DailyWinStreak;
            }

            loser.DailyLosses += 1;
            loser.DailyWinStreak = 0;
        }
    }

    // Todo: called from api
    public DailyResultDto? GetDailyResultDto(Guid persistentPlayerId)
    {
        var dailyMatch = GetDailyResult(persistentPlayerId);
        if (dailyMatch == null)
        {
            return null;
        }

        return new DailyResultDto(persistentPlayerId, dailyMatch);
    }

    // Todo: called from api
    public async Task<RankingStatsDto> GetRankingStats(
        GameState gameState,
        bool playerWon,
        List<GameParticipant> participants,
        bool dailyGame
    )
    {
        // Get the players
        var winner = await GetPlayerDao(participants[0]);
        var loser = await GetPlayerDao(participants[1]);
        var player = await GetPlayerDao(participants.Single(p => p.PersistentPlayerId == playerId));

        // Get the players Elo before the game
        var startingElo = playerWon ? winner.Elo : loser.Elo;
        EloService.SetPlayerElo(ref winner, ref loser);

        // Get the players Elo after the game
        var updatedElo = playerDaos.Single(p => p.Id == playerId).Elo;

        return new RankingStatsDto(startingElo, updatedElo);
    }

    private async Task<PlayerDao> GetPlayerDao(GameParticipant gameParticipant)
    {
        using var scope = scopeFactory.CreateScope();
        var gameResultContext = scope.ServiceProvider.GetRequiredService<GameResultContext>();
        var player = await gameResultContext.Players.FindAsync(gameParticipant.PersistentPlayerId);
        if (player == null)
        {
            player = new PlayerDao
            {
                Id = gameParticipant.PersistentPlayerId,
                Name = gameParticipant.Name,
                Elo = EloService.StartingElo
            };
            gameResultContext.Players.Add(player);
            await gameResultContext.SaveChangesAsync();
        }

        player.Name = gameParticipant.Name;
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
            .FirstOrDefault(
                gr =>
                    gr.Daily
                    && gr.DailyIndex == InMemoryGameService.GetDayIndex()
                    && (gr.Winner.Id == persistentPlayerId || gr.Loser.Id == persistentPlayerId)
            );
    }
}
