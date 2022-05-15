namespace Server;

using Models.Database;

public class DailyResultDto
{
    public bool CompletedToday { get; set; }
    public string? BotName { get; set; }
    public bool PlayerWon { get; set; }
    public int LostBy { get; set; } = -1;

    public int DailyWinStreak { get; set; } = -1;
    public int MaxDailyWinStreak { get; set; } = -1;
    public int DailyWins { get; set; } = -1;
    public int DailyLosses { get; set; } = -1;

    public DailyResultDto(Guid playerId, GameResultDao? game)
    {
        CompletedToday = game != null;
        if (!CompletedToday)
        {
            return;
        }

        PlayerWon = game!.Winner.Id == playerId;

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