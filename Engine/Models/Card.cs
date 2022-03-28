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
                return new CardLocation(CardPileName.Hand, handIndex, player.Id,
                    null);
            }

            var kittyIndex = player.KittyCards.IndexOf(this);
            if (kittyIndex != -1)
            {
                return new CardLocation(CardPileName.Kitty, kittyIndex, player.Id,
                    null);
            }

            var topUpIndex = player.TopUpCards.IndexOf(this);
            if (topUpIndex != -1)
            {
                return new CardLocation(CardPileName.TopUp, topUpIndex, player.Id,
                    null);
            }
        }

        for (var i = 0; i < gameState.CenterPiles.Count; i++)
        {
            var centerPile = gameState.CenterPiles[i];
            var centerPileIndex = centerPile.Cards.IndexOf(this);
            if (centerPileIndex != -1)
            {
                return new CardLocation(CardPileName.Center, centerPileIndex, null,
                    i);
            }
        }

        return default;
    }

    public static string CardsToString(IReadOnlyList<Card> cards, bool minified = false, bool includeSuit = false) =>
        string.Join(", ", cards.Select(c => c.ToString(minified, includeSuit)));

    public string ToString(bool minified = false, bool includeSuit = false)
    {
        var value = minified ? ((int)CardValue).ToString() : CardValue.ToString();
        if (!includeSuit)
        {
            return value;
        }

        var joiner = minified ? "" : " of ";
        var suit = minified ? Suit.ToString().ToLower()[0].ToString() : Suit.ToString();
        return $"{value}{joiner}{suit}";
    }

    public string ToString(Settings settings) =>
        ToString(settings.MinifiedCardStrings,
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

    public CardLocation(CardPileName pileName, int? pileIndex, int? playerId, int? centerIndex)
    {
        PileName = pileName;
        PileIndex = pileIndex;
        PlayerId = playerId;
        CenterIndex = centerIndex;
    }
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
