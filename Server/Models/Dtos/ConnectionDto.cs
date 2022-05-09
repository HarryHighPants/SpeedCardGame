namespace Server;

public class ConnectionDto
{
	public string ConnectionId { get; set; }
	public string Name { get; set; }
	public Rank Rank { get; set; }

	public ConnectionDto(GameParticipant gameParticipant)
	{
		// TODO: Hash this
		ConnectionId = gameParticipant.PersistentPlayerId.ToString();
		Name = gameParticipant.Name;
		Rank = gameParticipant.Rank ?? 0;
	}
}
