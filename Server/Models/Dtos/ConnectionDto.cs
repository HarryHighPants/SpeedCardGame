namespace Server;

public class ConnectionDto
{
	public string ConnectionId;
	public string Name;
	public Rank Rank;

	public ConnectionDto(Connection connection)
	{
		ConnectionId = connection.ConnectionId;
		Name = connection.Name;
		Rank = connection.Rank ?? 0;
	}
}
