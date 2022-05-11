using Microsoft.AspNetCore.Mvc;
using Server.Models.Database;

namespace Server.Controllers;

[ApiController]
[Route("shield")]
public class ShieldController : ControllerBase
{
    
    private readonly GameResultContext gameResultContext;

    public ShieldController(GameResultContext gameResultContext)
    {
        this.gameResultContext = gameResultContext;
    }
    
    
    [HttpGet("daily-games")]
    public IActionResult DailyGamesShieldData()
    {
        var playersCount = GamesPlayed(1);
        var shieldData = new ShieldData("Games today", playersCount.ToString());
        return Ok(shieldData);
    }

    [HttpGet("weekly-games")]
    public IActionResult WeeklyGamesShieldData()
    {
        var playersCount = GamesPlayed(7);
        var shieldData = new ShieldData("Weekly Games", playersCount.ToString());
        return Ok(shieldData);
    }

    [HttpGet("monthly-games")]
    public IActionResult MonthlyGamesShieldData()
    {
        var playersCount = GamesPlayed(31);
        var shieldData = new ShieldData("Monthly Games", playersCount.ToString());
        return Ok(shieldData);
    }

    private int GamesPlayed(int? inDays = null)
    {
        var dateFrom = DateTime.UtcNow.Subtract(TimeSpan.FromDays(inDays ?? 0));
        return gameResultContext.GameResults.Count(g => inDays == null || g.Created > dateFrom);
    }
    
    
    // https://shields.io/endpoint
    record ShieldData(
        string label,
        string message,
        string? color = "lightgrey",
        int? schemaVersion = 1
    );
}