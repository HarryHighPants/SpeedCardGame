namespace Server.Services;

using System.Linq;
using Engine.Models;
using Hubs;
using Microsoft.EntityFrameworkCore;
using Models.Database;

public class StatService
{
    private readonly IServiceScopeFactory scopeFactory;

    public StatService(IServiceScopeFactory scopeFactory)
    {
        this.scopeFactory = scopeFactory;
    }

    public async Task UpdateGameOverStats(
        GameState gameState,
        List<GameParticipant> participants,
        bool dailyGame
    )
    {
        var winnerParticipant = participants.Single(p => p.PlayerIndex == gameState.WinnerIndex);
        var loserParticipant = participants.Single(p => p.PlayerIndex != gameState.WinnerIndex);
        
        using var scope = scopeFactory.CreateScope();
        var gameResultContext = scope.ServiceProvider.GetRequiredService<GameResultContext>();
        var winner = await GetPlayerDao(winnerParticipant, gameResultContext);
        var loser = await GetPlayerDao(loserParticipant, gameResultContext);

        SetWinsAndLosses(winner, loser, dailyGame);
        var (eloGained, eloLost) = EloService.SetPlayerElo(ref winner, ref loser);
        
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
                DailyIndex = InMemoryGameService.GetDayIndex(),
                EloGained = eloGained,
                EloLost = eloLost
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

    private async Task<PlayerDao> GetPlayerDao(GameParticipant gameParticipant, GameResultContext gameResultContext)
    {
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
        return player;
    }
}
