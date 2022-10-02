using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Server.Helpers;

namespace Server.Controllers;

using System.Linq;
using Hubs;
using Microsoft.AspNetCore.Mvc;
using Models.Database;
using Services;

[ApiController]
[AllowAnonymous]
[Route("api")]
public class ApiController : ControllerBase
{
    private readonly GameResultContext gameResultContext;

    public ApiController(GameResultContext gameResultContext, EloService eloService)
    {
        this.gameResultContext = gameResultContext;
    }
    
    
    [HttpGet("id-hash/{persistentPlayerId}")]
    public IActionResult GetIdHash(Guid persistentPlayerId)
    {
        return Ok(persistentPlayerId.ToString().Hash());
    }

    [HttpGet("daily-stats/{persistentPlayerId}")]
    public IActionResult GetDailyResultDto(Guid persistentPlayerId)
    {
        var latestGame = GetPlayersDailyGame(persistentPlayerId);
        return Ok(new DailyResultDto(persistentPlayerId, latestGame));
    }

    /// <summary>
    /// Returns the top x players of today
    /// </summary>
    [HttpGet("daily-leaderboard")]
    public IActionResult GetDailyLeaderboardDto()
    {
        var todaysGames = gameResultContext.GameResults
            .Include(g => g.Winner)
            .Include(g => g.Loser)
            .Where(g =>
                g.Daily && g.DailyIndex == InMemoryGameService.GetDayIndex())
            .OrderByDescending(g => g.Winner.IsBot ? -g.LostBy : g.LostBy).ThenBy(g => g.Created);

        var bot = BotConfigurations.GetBot(BotType.Daily);
        
        var leaderboardData = new LeaderboardDto()
        {
            TotalPlayers = todaysGames.Count(),
            BotName = bot.Name,
            BotRank = EloService.GetRank((int)bot.Elo),
            Players = todaysGames.Take(50).AsEnumerable().Select((g, i) =>
            {
                var player = g.Winner.IsBot ? g.Loser : g.Winner;
                var playerScore = g.Winner.IsBot ? -g.LostBy : g.LostBy;
                return new LeaderboardPlayerDto()
                {
                    Name = player.Name,
                    Rank = EloService.GetRank(player.Elo),
                    Place = i+1,
                    Score = playerScore
                };
            }).ToArray()
        };
        return Ok(leaderboardData);
    }

    [HttpGet("latest-ranking-stats/{persistentPlayerId}")]
    public IActionResult GetRankingStats(Guid persistentPlayerId)
    {
        var latestGame = GetPlayersLatestGame(persistentPlayerId);

        if (latestGame == null)
        {
            return BadRequest("Could not find a game for player");
        }
        
        var won = latestGame.Winner.Id == persistentPlayerId;
        var playerDao = won ? latestGame.Winner : latestGame.Loser;
        var eloChangeAmt = won ? latestGame.EloGained : -latestGame.EloLost;
        var rankingStats = new RankingStatsDto(playerDao.Elo, eloChangeAmt);
        return Ok(rankingStats);
    }

    [HttpGet("top-players")]
    public IActionResult TopPlayers()
    {
        return Ok(
            gameResultContext.Players
                .OrderByDescending(p => p.Elo)
                .Take(10)
                .Select(p => new PlayerApi(p))
                .ToList()
        );
    }

    [HttpGet("player/{persistentPlayerId}")]
    public IActionResult TopPlayers(Guid persistentPlayerId)
    {
        var player = gameResultContext.Players.Find(persistentPlayerId);
        if (player == null)
        {
            return BadRequest("Could not find player with that id");
        }

        return Ok(new PlayerApi(player));
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(true);
    }

    private GameResultDao? GetPlayersLatestGame(Guid persistentPlayerId)
    {
        return gameResultContext.GameResults
            .OrderByDescending(g => g.Created)
            .Where(g => g.Created != DateTime.MinValue)
            .Include(g=>g.Winner)
            .Include(g=>g.Loser)
            .FirstOrDefault(g =>
                g.Winner.Id == persistentPlayerId
                 || g.Loser.Id == persistentPlayerId);
    }
    
    private GameResultDao? GetPlayersDailyGame(Guid persistentPlayerId)
    {
        return gameResultContext.GameResults
            .OrderByDescending(g => g.Created)
            .Include(g=>g.Winner)
            .Include(g=>g.Loser)
            .FirstOrDefault(g =>
                g.Daily && g.DailyIndex == InMemoryGameService.GetDayIndex() &&
                (g.Winner.Id == persistentPlayerId
                 || g.Loser.Id == persistentPlayerId));
    }

    class PlayerApi
    {
        public string Name { get; set; }
        public int Elo { get; set; }
        public string Rank { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int DailyLosses { get; set; }
        public int DailyWins { get; set; }
        public int DailyStreak { get; set; }

        public PlayerApi(PlayerDao playerDoa)
        {
            Name = playerDoa.Name;
            Elo = playerDoa.Elo;
            Rank = EloService.GetRank(playerDoa.Elo).ToString();
            DailyWins = playerDoa.DailyWins;
            DailyLosses = playerDoa.DailyLosses;
            Wins = playerDoa.Wins;
            Losses = playerDoa.Losses;
            DailyStreak = playerDoa.DailyWinStreak;
        }
    }
}
