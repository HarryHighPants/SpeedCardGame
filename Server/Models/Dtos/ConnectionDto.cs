namespace Server;

public class ConnectionDto
{
    public string ConnectionId;
    public string Name;
    public Rank Rank;

    public ConnectionDto(GameParticipant gameParticipant)
    {
        ConnectionId = gameParticipant.ConnectionId;
        Name = gameParticipant.Name;
        Rank = gameParticipant.Rank ?? 0;
    }
}
