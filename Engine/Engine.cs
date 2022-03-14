namespace Engine;

public static class Engine
{
    // Constants
    public const int PlayersPerGame = 2;
    public const int CardsPerSuit = 13;
    public const int CardsInDeck = 52;
    
    // The default max number of cards in a players hand
    public const int MaxHandCardsBase = 5;
    public const int CardsInKitty = 15;
    public const int CardsInTopUp = 5;

    public static GameState NewGame(List<string>? playerNames, Settings? settings)
    {
        // Create deck
        List<Card> deck = new List<Card>();
        for (int i = 0; i < CardsInDeck; i++)
        {
            Suit suit = (Suit) (i / CardsPerSuit);
            int value = i % CardsPerSuit;
            Card newCard = new Card
            {
                Id = i,
                Suit = suit,
                Value = value
            };
            deck.Add(newCard);
        }
        
        // Shuffle the new deck
        deck.Shuffle();
        
        // Deal cards to players
        List<Player> players = new List<Player>();
        for (int i = 0; i < PlayersPerGame; i++)
        {
            players.Add(new Player
            {
                Id = i+1,
                Name = playerNames?[i] ?? $"Player {i+1}",
                HandCards = deck.PopRange(MaxHandCardsBase),
                KittyCards = deck.PopRange(CardsInKitty),
                TopupCards = deck.PopRange(CardsInTopUp)
            });
        }

        GameState gameState = new GameState
        {
            Players = players,
            CenterPiles = new List<List<Card>>{deck.PopRange(1), deck.PopRange(1)},
            Settings = settings
        };
        return gameState;
    }
    
    public static int? CalculateWinner(GameState gameState)
    {
        foreach (var player in gameState.Players)     
        {
            // Check if a player has no cards in their hand or kitty
            if (player.HandCards.Count <= 0 && player.KittyCards.Count <= 0)
            {
                return player.Id;
            }
        }
        return null;
    }

    public static (GameState? updatedGameState, string? errorMessage) AttemptPlay(GameState gameState, Card card, int centerPileIndex)
    {
        if(centerPileIndex >= gameState.CenterPiles.Count) {return (null, $"No center pile found at index {centerPileIndex}"); }
        var cardLocation = FindCardLocation(gameState, card);
        switch (cardLocation.pile)
        {
            case CardPile.Hand:
                // Check that the card can be played onto the relevant center piles top card
                if (!CanPlay(card, gameState.CenterPiles[centerPileIndex].Last()))
                    return (null, $"Card with value {card.Value} can't be played onto {gameState.CenterPiles[centerPileIndex].Last().Value}");
                
                // This is a valid play so update gamestate
                gameState.CenterPiles[centerPileIndex].Add(card);
                gameState.Players[(int)cardLocation.playerIndex!].HandCards.Remove(card);
                return (gameState, null);
                break;
            
            default:
                return (null, $"Can't play a card from the {cardLocation.pile} pile");
            
            case null:
                return (null, "Card not found in GameState");
        }
    }

    public static bool CanPlay(Card topCard, Card bottomCard)
    {
        var valueDiff = Math.Abs(topCard.Value - bottomCard.Value);
        return valueDiff is 1 or CardsPerSuit;
    }

    public static (CardPile? pile, int? pileIndex, int? playerIndex, int? centerIndex) FindCardLocation(GameState gameState, Card card)
    {
        for (var i = 0; i < gameState.Players.Count; i++)
        {
            Player player = gameState.Players[i];
            var handIndex = player.HandCards.IndexOf(card);
            if (handIndex != -1)
                return (CardPile.Hand, pileIndex: handIndex, playerIndex: i, centerIndex: null);

            var kittyIndex = player.KittyCards.IndexOf(card);
            if (kittyIndex != -1)
                return (CardPile.Kitty, pileIndex: kittyIndex, playerIndex: i, centerIndex: null);

            var topupIndex = player.TopupCards.IndexOf(card);
            if (topupIndex != -1)
                return (CardPile.Topup, pileIndex: topupIndex, playerIndex: i, centerIndex: null);
        }

        for (var i = 0; i < gameState.CenterPiles.Count; i++)
        {
            var centerPile = gameState.CenterPiles[i];
            var centerPileIndex = centerPile.IndexOf(card);
            if (centerPileIndex != -1)
                return (CardPile.Center, pileIndex: centerPileIndex, playerIndex: null, centerIndex: i);
        }

        return (null, null, null, null);
    }
}