namespace Server;

public class LeaderboardDto
{
    public LeaderboardPlayerDto[] Players { get; set; }
    public int TotalPlayers { get; set; }
    public string BotName { get; set; }
    
    public Rank BotRank { get; set; }
}

public class LeaderboardPlayerDto
{
    public int Place { get; set; }
    public string Name { get; set; }
    public Rank Rank { get; set; }
    public int Score { get; set; }
}