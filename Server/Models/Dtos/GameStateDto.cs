namespace Server;

using System.Collections.Concurrent;
using System.Collections.Immutable;
using Engine;
using Engine.Models;

public class GameStateDto
{
	public List<PlayerDto> Players { get; }
	public List<CenterPile> CenterPiles { get; }
	public string LastMove { get; }
	public string? WinnerId { get; }
	public bool MustTopUp { get; }

	public GameStateDto(GameState gameState, List<Connection> connections, GameEngine gameEngine)
	{
		// todo: hash playerId
		Players = gameState.Players
			.Select((p, i) => new PlayerDto(p, gameEngine.Checks.CanRequestTopUp(gameState, i).Success))
			.ToList();

		MustTopUp = Players.All(p => p.CanRequestTopUp || p.RequestingTopUp);


		// Replace the playerId with the players connectionId
		foreach (var player in Players)
		{
			var connection = connections.FirstOrDefault(c => c.PlayerId.ToString() == player.Id);
			if (connection != null)
			{
				player.Id = connection.ConnectionId;
			}
		}

		// Only send the top 3
		CenterPiles = gameState.CenterPiles
			.Select((pile, i) => new CenterPile {Cards = pile.Cards.TakeLast(3).ToImmutableList()}).ToList();
		LastMove = gameState.LastMove;

		var winnerResult = gameEngine.Checks.TryGetWinner(gameState);
		WinnerId = gameEngine.Checks.TryGetWinner(gameState).Map(x =>
		{
			return connections.Single(c => c.PlayerId == x).ConnectionId;
		}, _ => null);
	}
}
