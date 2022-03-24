namespace Engine.Models;

using System.Collections.Immutable;

public record GameState
{
    public Settings? Settings { get; init; }
    public ImmutableList<Player> Players { get; init; }
    public ImmutableList<CenterPile> CenterPiles { get; init; }
    public ImmutableList<Move> MoveHistory { get; init; }
}

public record CenterPile
{
    public ImmutableList<Card> Cards = ImmutableList<Card>.Empty;
}
