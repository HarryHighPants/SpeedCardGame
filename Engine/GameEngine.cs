namespace Engine;

public static class GameEngine
{
    // Constants
    public const int PlayersPerGame = 2;
    public const int CardsPerSuit = 13;
    public const int CardsInDeck = 52;

    // The default max number of cards in a players hand
    public const int MaxHandCardsBase = 5;
    public const int CardsInKitty = 15;
    public const int CardsInTopUp = 5;

    public static GameState NewGame(List<string>? playerNames = null,
        Settings? settings = null)
    {
        // Initialise player names if none were supplied
        if (playerNames == null)
        {
            playerNames = new List<string>();
            for (var i = 0; i < PlayersPerGame; i++) playerNames.Add($"Player {i + 1}");
        }

        // Create deck
        var deck = new List<Card>();
        for (var i = 0; i < CardsInDeck; i++)
        {
            var suit = (Suit) (i / CardsPerSuit);
            int value = i % CardsPerSuit;
            var newCard = new Card
            {
                Id = i,
                Suit = suit,
                Value = value
            };
            deck.Add(newCard);
        }

        // Shuffle the new deck
        deck.Shuffle(settings?.RandomSeed);

        // Deal cards to players
        var players = new List<Player>();
        for (var i = 0; i < PlayersPerGame; i++)
        {
            players.Add(new Player
            {
                Id = i + 1,
                Name = playerNames?[i] ?? $"Player {i + 1}",
                HandCards = deck.PopRange(MaxHandCardsBase),
                KittyCards = deck.PopRange(CardsInKitty),
                TopUpCards = deck.PopRange(CardsInTopUp)
            });
        }

        var gameState = new GameState
        {
            Players = players,
            CenterPiles = new List<List<Card>> {deck.PopRange(1), deck.PopRange(1)},
            Settings = settings
        };
        return gameState;
    }

    public static Player? CalculateWinner(GameState gameState)
    {
        foreach (Player player in gameState.Players)
            // Check if a player has no cards in their hand or kitty
        {
            if (player.HandCards.Count <= 0 && player.KittyCards.Count <= 0)
            {
                return player;
            }
        }

        return null;
    }

    public static (GameState? updatedGameState, string? errorMessage) TryTopUp(GameState gameState)
    {
        // Check all players are requesting top up
        (bool canTopUp, string? errorMessage) = CanTopUp(gameState);
        if (!canTopUp) return (null, errorMessage);

        GameState updatedGameState = TopUp(gameState);
        return (updatedGameState, null);
    }

    public static (bool canTopUp, string? errorMessage) CanTopUp(GameState gameState)
    {
        // Check all players are requesting top up
        return !gameState.Players.All(p => p.RequestingTopUp)
            ? (false, "Not all players are requesting top up")
            : (true, null);
    }

    public static GameState TopUp(GameState gameState)
    {
        // Make sure we have cards in our top up pile
        if (gameState.Players[0].TopUpCards.Count <= 0) gameState = ReplenishTopUpCards(gameState);

        // Move each top up card to their respective center piles
        for (var i = 0; i < gameState.CenterPiles.Count; i++)
        {
            gameState.CenterPiles[i].Add(gameState.Players[i].TopUpCards.Pop());
        }

        return gameState;
    }

    public static GameState ReplenishTopUpCards(GameState gameState)
    {
        // combine middle piles
        List<Card> combinedCenterPiles = gameState.CenterPiles.SelectMany(cp => cp, (list, card) => card).ToList();

        // Shuffle them
        combinedCenterPiles.Shuffle(gameState.Settings?.RandomSeed);

        // Split all but center piles count into top up piles
        int topUpPileSize = combinedCenterPiles.Count / gameState.Players.Count;
        for (var i = 0; i < gameState.Players.Count; i++)
        {
            gameState.Players[i].TopUpCards = combinedCenterPiles.PopRange(topUpPileSize);
        }

        // Reset the center piles
        var newCenterPiles = new List<List<Card>>(gameState.CenterPiles.Count);
        for (var i = 0; i < newCenterPiles.Count; i++) newCenterPiles[i] = new List<Card>();
        gameState.CenterPiles = newCenterPiles;

        return gameState;
    }

    public static (GameState? updatedGameState, string? errorMessage) TryPlayCard(GameState gameState, Card card,
        int centerPileIndex)
    {
        if (centerPileIndex >= gameState.CenterPiles.Count)
        {
            return (null, $"No center pile found at index {centerPileIndex}");
        }

        (CardPile? pile, int? pileIndex, int? playerIndex, int? centerIndex) cardLocation =
            FindCardLocation(gameState, card);
        switch (cardLocation.pile)
        {
            case CardPile.Hand:
                // Check that the card can be played onto the relevant center piles top card
                if (!ValidMove(card, gameState.CenterPiles[centerPileIndex].Last()))
                {
                    return (null,
                        $"Card with value {card.Value} can't be played onto {gameState.CenterPiles[centerPileIndex].Last().Value}");
                }

                // This is a valid play so update gamestate
                GameState updatedGameState = PlayCard(gameState, card, centerPileIndex);
                return (updatedGameState, null);

            default:
                return (null, $"Can't play a card from the {cardLocation.pile} pile");

            case null:
                return (null, "Card not found in GameState");
        }
    }

    private static GameState PlayCard(GameState gameState, Card card, int centerPileIndex)
    {
        gameState.CenterPiles[centerPileIndex].Add(card);
        Player? player = gameState.Players.FirstOrDefault(p => p.HandCards.Contains(card));
        player?.HandCards.Remove(card);
        return gameState;
    }

    public static (GameState? updatedGameState, Card? pickedUpCard, string? errorMessage) TryPickupFromKitty(
        GameState gameState, Player player)
    {
        (bool canPickupFromKitty, string? message) result = CanPickupFromKitty(gameState, player);
        if (!result.canPickupFromKitty) return (null, null, result.message);

        player.HandCards.Add(player.KittyCards.Last());
        player.KittyCards.Remove(player.KittyCards.Last());
        return (gameState, player.HandCards.Last(), null);
    }

    public static (bool canPickupFromKitty, string? message) CanPickupFromKitty(GameState gameState, Player player)
    {
        // Check the player has room in their hand
        if (player.HandCards.Count >= (gameState.Settings?.MaxHandCards ?? MaxHandCardsBase))
        {
            return (false, $"Player {player.Name} hand is full");
        }

        // Check there is enough cards from players kitty 
        if (player.KittyCards.Count < 1) return (false, $"No cards left it {player.Name} kitty to pickup");

        return (true, null);
    }

    public static bool ValidMove(Card? topCard, Card? bottomCard)
    {
        if (topCard == null || bottomCard == null) return false;
        if (topCard.Value < 0 || bottomCard.Value < 0) return false;
        int valueDiff = Math.Abs(topCard.Value - bottomCard.Value);
        return valueDiff is 1 or CardsPerSuit - 1;
    }

    public static (CardPile? pile, int? pileIndex, int? playerIndex, int? centerIndex) FindCardLocation(
        GameState gameState, Card card)
    {
        for (var i = 0; i < gameState.Players.Count; i++)
        {
            Player player = gameState.Players[i];
            int handIndex = player.HandCards.IndexOf(card);
            if (handIndex != -1)
            {
                return (CardPile.Hand, pileIndex: handIndex, playerIndex: i, centerIndex: null);
            }

            int kittyIndex = player.KittyCards.IndexOf(card);
            if (kittyIndex != -1)
            {
                return (CardPile.Kitty, pileIndex: kittyIndex, playerIndex: i, centerIndex: null);
            }

            int topUpIndex = player.TopUpCards.IndexOf(card);
            if (topUpIndex != -1)
            {
                return (CardPile.TopUp, pileIndex: topUpIndex, playerIndex: i, centerIndex: null);
            }
        }

        for (var i = 0; i < gameState.CenterPiles.Count; i++)
        {
            List<Card> centerPile = gameState.CenterPiles[i];
            int centerPileIndex = centerPile.IndexOf(card);
            if (centerPileIndex != -1)
            {
                return (CardPile.Center, pileIndex: centerPileIndex, playerIndex: null, centerIndex: i);
            }
        }

        return (null, null, null, null);
    }

    public static (GameState? updatedGameState, bool immediateTopUp, bool readyToTopUp, string? errorMessage)
        TryRequestTopUp(
            GameState gameState,
            Player player, bool immediateTopUp = true)
    {
        // Check the player can top up
        (bool valid, string? errorMessage) requestTopUp = CanRequestTopUp(gameState, player);
        if (!requestTopUp.valid) return (null, false, false, requestTopUp.errorMessage);

        // Apply the top up request
        gameState.Players[gameState.Players.IndexOf(player)].RequestingTopUp = true;

        // Check if we can top up now
        (GameState? updatedGameState, string? errorMessage) topUp = TryTopUp(gameState);

        // If we want to immediately top up and we can then do it
        if (immediateTopUp && topUp.updatedGameState != null) return (topUp.updatedGameState, true, false, null);

        // Otherwise just return the gameState with the new top up request 
        return (gameState, false, topUp.updatedGameState != null, null);
    }

    // Bot Helpers
    public static (bool canPlay, Card? card, int? centerPile) PlayerHasPlay(GameState gameState, Player player)
    {
        foreach (Card card in player.HandCards)
        {
            (bool canPlay, int? centerPile) result = CardHasPlay(gameState, card);
            if (result.canPlay) return (true, card, result.centerPile);
        }

        return (false, null, null);
    }

    public static (bool canPlay, int? centerPile) CardHasPlay(GameState gameState, Card card)
    {
        for (var i = 0; i < gameState.CenterPiles.Count; i++)
        {
            if (gameState.CenterPiles[i].Count < 1) continue;
            Card? pileCard = gameState.CenterPiles[i]?.Last();
            if (ValidMove(card, pileCard))
            {
                return (true, i);
            }
        }

        return (false, null);
    }

    private static (bool canTopUp, string? errorMessage) CanRequestTopUp(GameState gameState, Player player)
    {
        // Check the player can't play
        (bool canPlay, Card? card, int? centerPile) playCard = PlayerHasPlay(gameState, player);
        if (playCard.canPlay) return (false, "Player can play a card");

        // Check the player can't pickup
        (GameState? updatedGameState, Card? pickedUpCard, string? errorMessage) pickupFromKitty =
            TryPickupFromKitty(gameState, player);
        if (pickupFromKitty.pickedUpCard != null) return (false, "Player can pickup from kitty");

        // We can't do anything else so top up is valid
        return (true, null);
    }
}