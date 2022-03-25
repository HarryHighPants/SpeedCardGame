namespace Engine.Models;

public record Card
{
    public int Id { get; init; }
    public Suit Suit { get; init; }
    public CardValue CardValue { get; init; }

    public CardLocation Location(GameState gameState)
    {
        for (var i = 0; i < gameState.Players.Count; i++)
        {
            var player = gameState.Players[i];
            var handIndex = player.HandCards.IndexOf(this);
            if (handIndex != -1)
            {
                return new CardLocation
                {
                    PileName = CardPileName.Hand, PileIndex = handIndex, PlayerId = player.Id, CenterIndex = null
                };
            }

            var kittyIndex = player.KittyCards.IndexOf(this);
            if (kittyIndex != -1)
            {
                return new CardLocation
                {
                    PileName = CardPileName.Kitty, PileIndex = kittyIndex, PlayerId = player.Id, CenterIndex = null
                };
            }

            var topUpIndex = player.TopUpCards.IndexOf(this);
            if (topUpIndex != -1)
            {
                return new CardLocation
                {
                    PileName = CardPileName.TopUp, PileIndex = topUpIndex, PlayerId = player.Id, CenterIndex = null
                };
            }
        }

        for (var i = 0; i < gameState.CenterPiles.Count; i++)
        {
            var centerPile = gameState.CenterPiles[i];
            var centerPileIndex = centerPile.Cards.IndexOf(this);
            if (centerPileIndex != -1)
            {
                return new CardLocation
                {
                    PileName = CardPileName.Center, PileIndex = centerPileIndex, PlayerId = null, CenterIndex = i
                };
            }
        }

        return default;
    }

    public static string CardsToString(IReadOnlyList<Card> cards, bool minified = false, bool includeSuit = false) =>
        string.Join(", ", cards.Select(c => c.ToString(minified, includeSuit)));

    public string ToString(bool minified = false, bool includeSuit = false)
    {
        var value = minified ? ((int)this.CardValue).ToString() : this.CardValue.ToString();
        if (!includeSuit)
        {
            return value;
        }

        var joiner = minified ? "" : " of ";
        var suit = minified ? this.Suit.ToString().ToLower()[0].ToString() : this.Suit.ToString();
        return $"{value}{joiner}{suit}";
    }

    public string ToString(Settings settings) =>
        this.ToString(settings.MinifiedCardStrings,
            settings.IncludeSuitInCardStrings);

    public static string ToString(GameState gameState, int? cardId, bool? minified = null, bool? includeSuit = null)
    {
        var cardResult = gameState.GetCard(cardId);
        return cardResult?.ToString(minified ?? gameState.Settings.MinifiedCardStrings,
            includeSuit ?? gameState.Settings.IncludeSuitInCardStrings) ?? "";
    }
}

public enum CardPileName
{
    Undefined,
    Hand,
    Kitty,
    TopUp,
    Center
}

public struct CardLocation
{
    public CardPileName PileName = CardPileName.Undefined;
    public int? PileIndex;
    public int? PlayerId;
    public int? CenterIndex;
}

public enum CardValue
{
    Two = 0,
    Three = 1,
    Four = 2,
    Five = 3,
    Six = 4,
    Seven = 5,
    Eight = 6,
    Nine = 7,
    Ten = 8,
    Jack = 9,
    Queen = 10,
    King = 11,
    Ace = 12
}

public enum Suit
{
    Hearts,
    Diamonds,
    Clubs,
    Spades
}
