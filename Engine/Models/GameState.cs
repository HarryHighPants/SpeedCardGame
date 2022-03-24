namespace Engine.Models;

using System.Collections.Immutable;

public record GameState
{
    public Settings Settings { get; init; }
    public ImmutableList<Player> Players { get; init; }
    public ImmutableList<CenterPile> CenterPiles { get; init; }
    public ImmutableList<Move> MoveHistory { get; init; }
    public string LastMove { get; init; } = "";

    public Player? GetPlayer(int? id)
    {
        var playerResult = this.Players.FirstOrDefault(p => p.Id == id);
        return playerResult;
    }

    public Card? GetCard(int? id)
    {
        foreach (var player in this.Players)
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

        for (var i = 0; i < this.CenterPiles.Count; i++)
        {
            var centerPile = this.CenterPiles[i];

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
    public ImmutableList<Card> Cards = ImmutableList<Card>.Empty;
}
