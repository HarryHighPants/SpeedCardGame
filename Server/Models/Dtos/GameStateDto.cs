using Server.Helpers;

namespace Server;

using System.Linq;
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

    public GameStateDto(GameState gameState, List<GameParticipant> connections)
    {
        Players = gameState.Players
            .Select(p => new PlayerDto(p, GetPlayersId(p, connections)))
            .ToList();

        MustTopUp = gameState.MustTopUp;

        // Only send the top 3
        CenterPiles = gameState.CenterPiles
            .Select(
                (pile, i) => new CenterPile { Cards = pile.Cards.TakeLast(3).ToImmutableList() }
            )
            .ToList();
        LastMove = gameState.LastMove;
        WinnerId = connections
            .SingleOrDefault(c => c.PlayerIndex == gameState.WinnerIndex)
            ?.PersistentPlayerId.ToString().Hash();
    }

    private static string GetPlayersId(Player player, List<GameParticipant> connections) =>
        connections.SingleOrDefault(c => c.PlayerIndex == player.Id)?.PersistentPlayerId.ToString().Hash()
        ?? player.Id.ToString();
}
