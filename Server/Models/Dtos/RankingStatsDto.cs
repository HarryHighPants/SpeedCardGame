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
		OldRank = InMemoryGameService.GetRank(previousElo);
		NewRank = InMemoryGameService.GetRank(newElo);
		PreviousElo = previousElo;
		NewElo = newElo;
	}
}
