namespace Server;

public class ConnectionDto
{
    public string ConnectionId;
    public string Name;
    public Rank Rank;

    public ConnectionDto(GameParticipant gameParticipant)
    {
        // TODO: Hash this
        ConnectionId = gameParticipant.PersistentPlayerId.ToString();
        Name = gameParticipant.Name;
        Rank = gameParticipant.Rank ?? 0;
    }
}
