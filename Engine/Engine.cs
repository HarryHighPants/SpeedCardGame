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
            Card newCard = new Card(i, suit, value);
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
            CenterCards = deck.PopRange(2),
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
}