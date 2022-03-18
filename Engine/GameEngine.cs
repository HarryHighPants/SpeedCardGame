using Engine.Helpers;

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

    public static Result<Player> TryGetWinner(GameState gameState)
    {
        // Check if a player has no cards in their hand or kitty
        foreach (Player player in gameState.Players)
        {
            if (player.HandCards.Count <= 0 && player.KittyCards.Count <= 0)
            {
                // Return any player that does
                return new SuccessResult<Player>(player);
            }
        }

        return new ErrorResult<Player>("No winner yet!");
    }

    public static Result AllPlayersRequestingTopUp(GameState gameState)
    {
        // Check all players are requesting top up
        return !gameState.Players.All(p => p.RequestingTopUp)
            ? new ErrorResult("Not all players are requesting top up")
            : new SuccessResult();
    }

    public static Result<GameState> TryTopUp(GameState gameState)
    {
        // Check all players are requesting top up
        Result allPlayersRequestingTopUp = AllPlayersRequestingTopUp(gameState);
        if (allPlayersRequestingTopUp is ErrorResult allPlayersRequestingTopUpError)
        {
            return new ErrorResult<GameState>(allPlayersRequestingTopUpError.Message);
        }

        // Make sure we have cards in our top up pile
        if (gameState.Players[0].TopUpCards.Count <= 0)
        {
            Result<GameState> replenishResult = ReplenishTopUpCards(gameState);
            if (replenishResult is ErrorResult<GameState> replenishError) return replenishError;
            gameState = replenishResult.Data;
        }

        // Move each top up card to their respective center piles
        for (var i = 0; i < gameState.CenterPiles.Count; i++)
        {
            gameState.CenterPiles[i].Add(gameState.Players[i].TopUpCards.Pop());
        }

        return new SuccessResult<GameState>(gameState);
    }

    public static Result<GameState> ReplenishTopUpCards(GameState gameState)
    {
        // combine middle piles
        List<Card> combinedCenterPiles = gameState.CenterPiles.SelectMany(cp => cp, (list, card) => card).ToList();

        // Shuffle them
        combinedCenterPiles.Shuffle(gameState.Settings?.RandomSeed);

        // Split all but center piles count into top up piles
        int topUpPileSize = combinedCenterPiles.Count / gameState.Players.Count;
        foreach (Player player in gameState.Players)
        {
            player.TopUpCards = combinedCenterPiles.PopRange(topUpPileSize);
        }

        // Reset the center piles
        var newCenterPiles = new List<List<Card>>(gameState.CenterPiles.Count);
        for (var i = 0; i < newCenterPiles.Count; i++) newCenterPiles[i] = new List<Card>();
        gameState.CenterPiles = newCenterPiles;

        return new SuccessResult<GameState>(gameState);
    }

    public static Result<GameState> TryPlayCard(GameState gameState, Player player, Card card,
        int centerPileIndex)
    {
        if (centerPileIndex >= gameState.CenterPiles.Count)
        {
            return new ErrorResult<GameState>($"No center pile found at index {centerPileIndex}");
        }

        Result<CardLocation> cardLocationResult = FindCardLocation(gameState, card);
        if (cardLocationResult is IErrorResult cardLocationResultError)
        {
            return new ErrorResult<GameState>(cardLocationResultError.Message);
        }

        if (cardLocationResult.Data.PlayerIndex != gameState.Players.IndexOf(player))
        {
            return new ErrorResult<GameState>($"Player {player.Name} does not have card {card.Value} in their hand");
        }

        switch (cardLocationResult.Data.PileName)
        {
            case CardPileName.Hand:
                // Check that the card can be played onto the relevant center piles top card
                if (!ValidMove(card, gameState.CenterPiles[centerPileIndex].Last()))
                {
                    return new ErrorResult<GameState>(
                        $"Card with value {card.Value} can't be played onto {gameState.CenterPiles[centerPileIndex].Last().Value}");
                }

                // This is a valid play so play it
                return PlayCard(gameState, card, centerPileIndex);

            default:
                return new ErrorResult<GameState>(
                    $"Can't play a card from the {cardLocationResult.Data.PileName} pile");
        }
    }

    private static Result<GameState> PlayCard(GameState gameState, Card card, int centerPileIndex)
    {
        gameState.CenterPiles[centerPileIndex].Add(card);
        Player? player = gameState.Players.FirstOrDefault(p => p.HandCards.Contains(card));
        player?.HandCards.Remove(card);
        return new SuccessResult<GameState>(gameState);
    }

    public static Result<(GameState updatedGameState, Card pickedUpCard)> TryPickupFromKitty(
        GameState gameState, Player player)
    {
        Result canPickupResult = CanPickupFromKitty(gameState, player);
        if (canPickupResult is ErrorResult canPickupResultError)
        {
            return new ErrorResult<(GameState updatedGameState, Card pickedUpCard)>(canPickupResultError.Message);
        }

        player.HandCards.Add(player.KittyCards.Last());
        player.KittyCards.Remove(player.KittyCards.Last());
        return new SuccessResult<(GameState updatedGameState, Card pickedUpCard)>((gameState, player.HandCards.Last()));
    }

    public static Result CanPickupFromKitty(GameState gameState, Player player)
    {
        // Check the player has room in their hand
        if (player.HandCards.Count >= (gameState.Settings?.MaxHandCards ?? MaxHandCardsBase))
        {
            return new ErrorResult($"Player {player.Name} hand is full");
        }

        // Check there is enough cards from players kitty 
        if (player.KittyCards.Count < 1) return new ErrorResult($"No cards left in {player.Name} kitty to pickup");

        return new SuccessResult();
    }

    public static bool ValidMove(Card? topCard, Card? bottomCard)
    {
        if (topCard == null || bottomCard == null) return false;
        if (topCard.Value < 0 || bottomCard.Value < 0) return false;
        int valueDiff = Math.Abs(topCard.Value - bottomCard.Value);
        return valueDiff is 1 or CardsPerSuit - 1;
    }

    private static Result<CardLocation> FindCardLocation(
        GameState gameState, Card card)
    {
        for (var i = 0; i < gameState.Players.Count; i++)
        {
            Player player = gameState.Players[i];
            int handIndex = player.HandCards.IndexOf(card);
            if (handIndex != -1)
            {
                return new SuccessResult<CardLocation>(new CardLocation(CardPileName.Hand, handIndex, i, null));
            }

            int kittyIndex = player.KittyCards.IndexOf(card);
            if (kittyIndex != -1)
            {
                return new SuccessResult<CardLocation>(new CardLocation(CardPileName.Kitty, kittyIndex, i, null));
            }

            int topUpIndex = player.TopUpCards.IndexOf(card);
            if (topUpIndex != -1)
            {
                return new SuccessResult<CardLocation>(new CardLocation(CardPileName.TopUp, topUpIndex, i, null));
            }
        }

        for (var i = 0; i < gameState.CenterPiles.Count; i++)
        {
            List<Card> centerPile = gameState.CenterPiles[i];
            int centerPileIndex = centerPile.IndexOf(card);
            if (centerPileIndex != -1)
            {
                return new SuccessResult<CardLocation>(new CardLocation(CardPileName.Center, centerPileIndex, null, i));
            }
        }

        return new ErrorResult<CardLocation>("Card not found in gameState");
    }


    public static Result<(GameState updatedGameState, bool immediateTopUp, bool readyToTopUp)>
        TryRequestTopUp(
            GameState gameState,
            Player player, bool immediateTopUp = true)
    {
        // Check the player can top up
        Result requestTopUpResult = CanRequestTopUp(gameState, player);
        if (requestTopUpResult is ErrorResult requestTopUpResultError)
        {
            return new ErrorResult<(GameState updatedGameState, bool immediateTopUp, bool readyToTopUp)>(
                requestTopUpResultError.Message);
        }

        // Apply the top up request
        gameState.Players[gameState.Players.IndexOf(player)].RequestingTopUp = true;

        // Check if we can top up now
        Result<GameState> topUpResult = TryTopUp(gameState);

        // If we want to immediately top up and we can then do it
        if (immediateTopUp && topUpResult.Success)
        {
            return new SuccessResult<(GameState updatedGameState, bool immediateTopUp, bool readyToTopUp)>(
                (topUpResult.Data, true, false));
        }

        // Otherwise just return the gameState with the new top up request 
        return new SuccessResult<(GameState updatedGameState, bool immediateTopUp, bool readyToTopUp)>(
            (gameState, false, topUpResult.Success));
    }

    // Bot Helpers
    public static Result<(Card card, int centerPile)> PlayerHasPlay(GameState gameState, Player player)
    {
        foreach (Card card in player.HandCards)
        {
            Result<int> cardHasPlayResult = CardHasPlay(gameState, card);
            if (cardHasPlayResult.Success)
            {
                return new SuccessResult<(Card card, int centerPile)>((card, cardHasPlayResult.Data));
            }
        }

        return new ErrorResult<(Card card, int centerPile)>("No valid play for player");
    }

    /// <summary>
    /// </summary>
    /// <param name="gameState"></param>
    /// <param name="card"></param>
    /// <returns>Result with CenterPileindex as data</returns>
    public static Result<int> CardHasPlay(GameState gameState, Card card)
    {
        for (var i = 0; i < gameState.CenterPiles.Count; i++)
        {
            if (gameState.CenterPiles[i].Count < 1) continue;
            Card pileCard = gameState.CenterPiles[i].Last();
            if (ValidMove(card, pileCard))
            {
                return new SuccessResult<int>(i);
            }
        }

        return new ErrorResult<int>("Card can't be played onto any center pile");
    }

    private static Result CanRequestTopUp(GameState gameState, Player player)
    {
        // Check the player can't play
        Result<(Card card, int centerPile)> hasPlayResult = PlayerHasPlay(gameState, player);
        if (hasPlayResult.Success) return new ErrorResult("Player can play a card");

        // Check the player can't pickup
        Result<(GameState updatedGameState, Card pickedUpCard)> pickupFromKittyResult =
            TryPickupFromKitty(gameState, player);
        if (pickupFromKittyResult.Success) return new ErrorResult("Player can pickup from kitty");

        // We can't do anything else so top up is valid
        return new SuccessResult();
    }

    private struct CardLocation
    {
        public readonly CardPileName PileName;
        public int PileIndex;
        public readonly int? PlayerIndex;
        public int? CenterIndex;

        public CardLocation(CardPileName pileName, int pileIndex, int? playerIndex, int? centerIndex)
        {
            PileName = pileName;
            PileIndex = pileIndex;
            PlayerIndex = playerIndex;
            CenterIndex = centerIndex;
        }
    }
}