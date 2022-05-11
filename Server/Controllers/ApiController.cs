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

    public ApiController(GameResultContext gameResultContext)
    {
        this.gameResultContext = gameResultContext;
    }
    
    
    [HttpGet("id-hash/{persistentPlayerId}")]
    public IActionResult GetIdHash(Guid persistentPlayerId)
    {
        return Ok(persistentPlayerId.ToString().Hash());
    }

    [HttpGet("latest-daily-stats")]
    public IActionResult GetDailyResultDto(Guid persistentPlayerId)
    {
        var latestGame = GetPlayersLatestGame(persistentPlayerId, true);
        if (latestGame == null)
        {
            return BadRequest("Could not find a daily game for player");
        }

        return Ok(new DailyResultDto(persistentPlayerId, latestGame));
    }

    [HttpGet("latest-ranking-stats")]
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

    [HttpGet("player/{playerId}")]
    public IActionResult TopPlayers(Guid playerId)
    {
        var player = gameResultContext.Players.Find(playerId);
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

    private GameResultDao? GetPlayersLatestGame(Guid persistentPlayerId, bool dailyGameOnly = false)
    {
        return gameResultContext.GameResults
            .OrderBy(g => g.Created)
            .Include(g=>g.Winner)
            .Include(g=>g.Loser)
            .FirstOrDefault(g =>
                (!dailyGameOnly || g.Daily) && 
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