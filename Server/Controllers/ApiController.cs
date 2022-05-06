namespace Server.Controllers;

using System.Linq;
using Hubs;
using Microsoft.AspNetCore.Mvc;
using Models.Database;
using Services;

[ApiController]
[Route("api")]
public class ApiController : ControllerBase
{
	private readonly GameResultContext gameResultContext;

	public ApiController(GameResultContext gameResultContext)
	{
		this.gameResultContext = gameResultContext;
	}

	[HttpGet]
	public IActionResult GetGamesPlayed()
	{
		return Ok(gameResultContext.GameResults.Count());
	}

	[HttpGet("top-players")]
	public IActionResult TopPlayers()
	{
		return Ok(gameResultContext.Players.OrderByDescending(p=>p.Elo).Take(10).Select(p=>new PlayerApi(p)).ToList());
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

	[HttpGet("ping-shield-data")]
	public IActionResult PingShieldData()
	{
		var shieldData = new ShieldData("Server status", "Online", "green");
		return Ok(shieldData);
	}

	[HttpGet("daily-games-shield-data")]
	public IActionResult DailyGamesShieldData()
	{
		var playersCount = GamesPlayed(1);
		var shieldData = new ShieldData("Games today", playersCount.ToString());
		return Ok(shieldData);
	}

	[HttpGet("weekly-games-shield-data")]
	public IActionResult WeeklyGamesShieldData()
	{
		var playersCount = GamesPlayed(7);
		var shieldData = new ShieldData("Weekly Games", playersCount.ToString());
		return Ok(shieldData);
	}

	[HttpGet("monthly-games-shield-data")]
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
	record ShieldData(string label, string message, string? color = "lightgrey", int? schemaVersion = 1);

	class PlayerApi
	{
		public string Name { get; set; }
		private int Elo { get; set; }
		private string Rank { get; set; }
		private int Wins { get; set; }
		private int Losses { get; set; }
		private int DailyLosses { get; set; }
		private int DailyWins { get; set; }
		private int DailyStreak { get; set; }

		public PlayerApi(PlayerDao playerDoa)
		{
			Name = playerDoa.Name;
			Elo = playerDoa.Elo;
			Rank = InMemoryGameService.GetRank(playerDoa.Elo).ToString();
			DailyWins = playerDoa.DailyWins;
			DailyLosses = playerDoa.DailyLosses;
			Wins = playerDoa.Wins;
			Losses = playerDoa.Losses;
			DailyStreak = playerDoa.DailyWinStreak;
		}
	}
}
