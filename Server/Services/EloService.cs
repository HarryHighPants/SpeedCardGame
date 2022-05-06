namespace Server.Services;

using Models.Database;

public class EloService
{
	public static Rank GetRank(int elo) =>
		elo switch
		{
			<= 500 => Rank.EagerStarter,
			<= 1000 => Rank.DedicatedRival,
			<= 1500 => Rank.CertifiedRacer,
			<= 2000 => Rank.BossAthlete,
			<= 2500 => Rank.Acetronaut,
			> 2500 => Rank.SpeedDemon
		};

	public static readonly int StartingElo = 250;

	public static void CalculateElo(ref PlayerDao winner, ref PlayerDao loser)
	{
		int eloK = 20;

		int delta = (int)(eloK * (1 - ExpectationToWin(winner.Elo, loser.Elo)));
		winner.Elo += (int)(delta * PlayerK(winner.Wins+winner.Losses));
		loser.Elo -= (int)(delta * PlayerK(loser.Wins+loser.Losses));
	}

	/// <summary>
	/// If a player is newer we want their elo to change more
	/// </summary>
	/// <param name="gamesPlayed"></param>
	/// <returns></returns>
	static float PlayerK(int gamesPlayed)
	{
		return (20 / gamesPlayed) + 1;
	}

	static double ExpectationToWin(int playerOneRating, int playerTwoRating)
	{
		return 1 / (1 + Math.Pow(10, (playerTwoRating - playerOneRating) / 400.0));
	}
}
