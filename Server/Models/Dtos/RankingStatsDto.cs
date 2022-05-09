namespace Server;

using Services;

public class RankingStatsDto
{
	public Rank OldRank { get; set; }
	public Rank NewRank { get; set; }
	public int PreviousElo { get; set; }
	public int NewElo { get; set; }

	public RankingStatsDto(int previousElo, int newElo)
	{
		OldRank = EloService.GetRank(previousElo);
		NewRank = EloService.GetRank(newElo);
		PreviousElo = previousElo;
		NewElo = newElo;
	}
}
