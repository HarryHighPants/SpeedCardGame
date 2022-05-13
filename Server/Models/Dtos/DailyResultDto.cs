namespace Server;

using Models.Database;

public class DailyResultDto
{
	public string BotName { get; set; }
	public bool PlayerWon { get; set; }
	public int LostBy { get; set; }

	public int DailyWinStreak { get; set; }
	public int MaxDailyWinStreak { get; set; }
	public int DailyWins { get; set; }
	public int DailyLosses { get; set; }

	public DailyResultDto(Guid playerId, GameResultDao game)
	{
		PlayerWon = game.Winner.Id == playerId;

		var player = PlayerWon ? game.Winner : game.Loser;
		LostBy = game.LostBy;
		DailyWinStreak = player.DailyWinStreak;
		MaxDailyWinStreak = player.MaxDailyWinStreak;
		DailyWins = player.DailyWins;
		DailyLosses = player.DailyLosses;

		var bot = PlayerWon ? game.Loser : game.Winner;
		BotName = bot.Name;
	}
}
