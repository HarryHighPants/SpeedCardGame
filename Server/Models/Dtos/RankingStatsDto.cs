namespace Server;

using Services;

public class RankingStatsDto
{
	public Rank OldRank { get; set; }
	public Rank NewRank { get; set; }
	public int PreviousElo { get; set; }
	public int NewElo { get; set; }

	public RankingStatsDto(int currentElo, int eloChangeAmt)
	{
		var previousElo = currentElo - eloChangeAmt;
		OldRank = EloService.GetRank(previousElo);
		NewRank = EloService.GetRank(currentElo);
		PreviousElo = previousElo;
		NewElo = currentElo;
	}
}
