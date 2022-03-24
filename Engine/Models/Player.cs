namespace Engine.Models;

using System.Collections.Immutable;

public record Player
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public ImmutableList<Card> HandCards { get; init; }
    public ImmutableList<Card> KittyCards { get; init; }
    public ImmutableList<Card> TopUpCards { get; init; }
    public bool RequestingTopUp { get; init; }


    public static Player Get(GameState gameState, int? playerId)
    {
        var playerResult = gameState.Players.FirstOrDefault(p => p.Id == playerId);
        if (playerResult != default)
        {
            return playerResult;
        }

        return default;
    }
}
