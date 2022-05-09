namespace Server;

using Services;

public class RankingStatsDto
{
    public Rank OldRank;
    public Rank NewRank;
    public int PreviousElo;
    public int NewElo;

    public RankingStatsDto(int previousElo, int newElo)
    {
        OldRank = EloService.GetRank(previousElo);
        NewRank = EloService.GetRank(newElo);
        PreviousElo = previousElo;
        NewElo = newElo;
    }
}
