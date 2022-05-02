namespace Server;

using Models.Database;

public class DailyResultDto
{
	public string BotName;
	public bool PlayerWon;
	public int LostBy;

	public int DailyWinStreak;
	public int MaxDailyWinStreak;
	public int DailyWins;
	public int DailyLosses;
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