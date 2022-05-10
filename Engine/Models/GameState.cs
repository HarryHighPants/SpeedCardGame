namespace Engine.Models;

using System.Collections.Immutable;

public record GameState
{
    public Settings Settings { get; init; }
    public ImmutableList<Player> Players { get; init; }
    public ImmutableList<CenterPile> CenterPiles { get; init; }
    public ImmutableList<Move> MoveHistory { get; init; }
    public string LastMove { get; init; } = "";
    public int? WinnerIndex { get; init; }

    public bool MustTopUp { get; init; }

    public Player? GetPlayer(int? id)
    {
        var playerResult = Players.FirstOrDefault(p => p.Id == id);
        return playerResult;
    }

    public Card? GetCard(int? id)
    {
        foreach (var player in Players)
        {
            var handCard = player.HandCards.FirstOrDefault(c => c.Id == id);
            if (handCard != default)
            {
                return handCard;
            }

            var kittyCard = player.KittyCards.FirstOrDefault(c => c.Id == id);
            if (kittyCard != default)
            {
                return kittyCard;
            }

            var topUpCard = player.TopUpCards.FirstOrDefault(c => c.Id == id);
            if (topUpCard != default)
            {
                return topUpCard;
            }
        }

        for (var i = 0; i < CenterPiles.Count; i++)
        {
            var centerPile = CenterPiles[i];

            var centerCard = centerPile.Cards.FirstOrDefault(c => c.Id == id);
            if (centerCard != default)
            {
                return centerCard;
            }
        }

        return default;
    }
}

public record CenterPile
{
    public ImmutableList<Card> Cards { get; init; } = ImmutableList<Card>.Empty;
}
