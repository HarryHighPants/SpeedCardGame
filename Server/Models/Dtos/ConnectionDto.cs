using Server.Helpers;

namespace Server;

public class ConnectionDto
{
	public string PlayerId { get; set; }
	public string Name { get; set; }
	public Rank Rank { get; set; }

	public ConnectionDto(GameParticipant gameParticipant)
	{
		PlayerId = gameParticipant.PersistentPlayerId.ToString().Hash();
		Name = gameParticipant.Name;
		Rank = gameParticipant.Rank ?? 0;
	}
}
