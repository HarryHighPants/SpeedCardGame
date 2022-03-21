using System.Collections.Immutable;
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
                HandCards = deck.PopRange(MaxHandCardsBase).ToImmutableList(),
                KittyCards = deck.PopRange(CardsInKitty).ToImmutableList(),
                TopUpCards = deck.PopRange(CardsInTopUp).ToImmutableList()
            });
        }

        var gameState = new GameState
        {
            Players = players.ToImmutableList(),
            CenterPiles = new List<ImmutableList<Card>> { deck.PopRange(1).ToImmutableList(), deck.PopRange(1).ToImmutableList()}.ToImmutableList(),
            Settings = settings
        };
        return gameState;
    }

    public static Result<int> TryGetWinner(GameState gameState)
    {
        // Check if a player has no cards in their hand or kitty
        foreach (var (player, i) in gameState.Players.IndexTuples())
        {
            if (player.HandCards.Count <= 0 && player.KittyCards.Count <= 0)
            {
                // Return any player that does
                return Result.Successful(i);
            }
        }

        return Result.Error<int>("No winner yet!");
    }

    public static Result AllPlayersRequestingTopUp(GameState gameState)
    {
        // Check all players are requesting top up
        return !gameState.Players.All(p => p.RequestingTopUp)
            ? Result.Error("Not all players are requesting top up")
            : new SuccessResult();
    }

    public static Result<GameState> TryTopUp(GameState gameState)
    {
        // Check all players are requesting top up
        Result canTopUpResult = CanTopUp(gameState);
        if (canTopUpResult is IErrorResult canTopUpError)
        {
            return Result.Error<GameState>(canTopUpError.Message);
        }

        // Make sure we have cards in our top up pile
        if (gameState.Players[0].TopUpCards.Count <= 0)
        {
            Result<GameState> replenishResult = ReplenishTopUpCards(gameState);
            if (replenishResult is ErrorResult<GameState> replenishError) return replenishError;
            gameState = replenishResult.Data;
        }

        // Move each top up card to their respective center piles
        var newCenterPiles =
            gameState.CenterPiles.Select((pile, i) => pile.Append(gameState.Players[i].TopUpCards.Last()).ToImmutableList()).ToImmutableList();

        var newPlayers = gameState.Players.Select(player =>
            player with {RequestingTopUp = false, TopUpCards = player.TopUpCards.Take(..^1).ToImmutableList()}).ToImmutableList();

        return Result.Successful(gameState with {Players = newPlayers, CenterPiles = newCenterPiles});
    }

    private static Result CanTopUp(GameState gameState)
    {
        // Check all players are requesting top up
        Result allPlayersRequestingTopUp = AllPlayersRequestingTopUp(gameState);
        if (allPlayersRequestingTopUp is ErrorResult allPlayersRequestingTopUpError)
        {
            return Result.Error(allPlayersRequestingTopUpError.Message);
        }

        return new SuccessResult();
    }

    public static Result<GameState> ReplenishTopUpCards(GameState gameState)
    {
        // combine middle piles
        List<Card> combinedCenterPiles = gameState.CenterPiles.SelectMany(cp => cp, (list, card) => card).ToList();

        if (combinedCenterPiles.Count < 1)
        {
            return Result.Error<GameState>("Can't replenish top up piles without any center cards");
        }

        // Shuffle them
        combinedCenterPiles.Shuffle(gameState.Settings?.RandomSeed);

        // Split all but center piles count into top up piles
        int topUpPileSize = combinedCenterPiles.Count / gameState.Players.Count;
        var newPlayers =
            gameState.Players.Select(player => player with {TopUpCards = combinedCenterPiles.PopRange(topUpPileSize).ToImmutableList()}).ToImmutableList();

        // Reset the center piles
        var newCenterPiles = new List<ImmutableList<Card>>();
        for (var i = 0; i < gameState.CenterPiles.Count; i++) newCenterPiles.Add( ImmutableList<Card>.Empty);

        return Result.Successful(gameState with{Players = newPlayers, CenterPiles = newCenterPiles.ToImmutableList()});
    }

    public static Result<GameState> TryPlayCard(GameState gameState, int playerIndex, Card card,
        int centerPileIndex)
    {
        if (centerPileIndex >= gameState.CenterPiles.Count)
        {
            return Result.Error<GameState>($"No center pile found at index {centerPileIndex}");
        }

        Result<CardLocation> cardLocationResult = FindCardLocation(gameState, card);
        if (cardLocationResult is IErrorResult cardLocationResultError)
        {
            return Result.Error<GameState>(cardLocationResultError.Message);
        }

        if (cardLocationResult.Data.PlayerIndex != playerIndex)
        {
            return Result.Error<GameState>($"Player {gameState.Players[playerIndex].Name} does not have card {card.Value} in their hand");
        }

        switch (cardLocationResult.Data.PileName)
        {
            case CardPileName.Hand:
                // Check that the card can be played onto the relevant center piles top card
                if (!ValidMove(card, gameState.CenterPiles[centerPileIndex].Last()))
                {
                    return Result.Error<GameState>(
                        $"Card with value {card.Value} can't be played onto {gameState.CenterPiles[centerPileIndex].Last().Value}");
                }

                // This is a valid play so play it
                return PlayCard(gameState, card, centerPileIndex);

            default:
                return Result.Error<GameState>(
                    $"Can't play a card from the {cardLocationResult.Data.PileName} pile");
        }
    }

    private static Result<GameState> PlayCard(GameState gameState, Card card, int centerPileIndex)
    {
        var newGameState = gameState;
        
        // Add the card being played to the center pile
        var newCenterPiles = newGameState.CenterPiles.ReplaceElementAt(
            centerPileIndex, gameState.CenterPiles[centerPileIndex].Append(card).ToImmutableList()).ToImmutableList();
        newGameState = newGameState with {CenterPiles = newCenterPiles};
        
        
        var (playerWithCard, playerIndexWithCard) = gameState.Players.IndexTuples().First(p => p.item.HandCards.Contains(card));
        var newHandCards = playerWithCard.HandCards.Where(c => c != card).ToImmutableList();
        var newPlayer = playerWithCard with {HandCards = newHandCards};
        var newPlayers = newGameState.Players.ReplaceElementAt(playerIndexWithCard, newPlayer).ToImmutableList();
        newGameState = newGameState with {Players = newPlayers};

        // Reset any ones request to top up if they can now move
        newPlayers = newPlayers.Select((player, i) =>
            player.RequestingTopUp && PlayerHasPlay(newGameState, i).Success
                ? player with {RequestingTopUp = false}
                : player).ToImmutableList();
        
        return Result.Successful(newGameState with {Players = newPlayers});
    }

    public static Result<(GameState updatedGameState, Card pickedUpCard)> TryPickupFromKitty(
        GameState gameState, int playerIndex)
    {
        var player = gameState.Players[playerIndex];
        
        Result canPickupResult = CanPickupFromKitty(gameState, playerIndex);
        if (canPickupResult is ErrorResult canPickupResultError)
        {
            return Result.Error<(GameState updatedGameState, Card pickedUpCard)>(canPickupResultError.Message);
        }

        var newHandCard = player.HandCards.ToImmutableList();
        newHandCard = newHandCard.Add(player.KittyCards.Last());

        var newKittyCards = player.KittyCards.ToImmutableList();
        newKittyCards = newKittyCards.Remove(player.KittyCards.Last());
        var newPlayer = player with {HandCards = newHandCard, KittyCards = newKittyCards};
        var newPlayers = gameState.Players.ReplaceElementAt(playerIndex, newPlayer).ToImmutableList();
        
        return Result.Successful((gameState with {Players = newPlayers}, newPlayer.HandCards.Last()));
    }

    public static Result CanPickupFromKitty(GameState gameState, int playerIndex)
    {
        var player = gameState.Players[playerIndex];

        // Check the player has room in their hand
        if (player.HandCards.Count >= (gameState.Settings?.MaxHandCards ?? MaxHandCardsBase))
        {
            return Result.Error($"Player {player.Name} hand is full");
        }

        // Check there is enough cards from players kitty 
        if (player.KittyCards.Count < 1) return Result.Error($"No cards left in {player.Name} kitty to pickup");

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
                return Result.Successful(new CardLocation(CardPileName.Hand, handIndex, i, null));
            }

            int kittyIndex = player.KittyCards.IndexOf(card);
            if (kittyIndex != -1)
            {
                return Result.Successful(new CardLocation(CardPileName.Kitty, kittyIndex, i, null));
            }

            int topUpIndex = player.TopUpCards.IndexOf(card);
            if (topUpIndex != -1)
            {
                return Result.Successful(new CardLocation(CardPileName.TopUp, topUpIndex, i, null));
            }
        }

        for (var i = 0; i < gameState.CenterPiles.Count; i++)
        {
            var centerPile = gameState.CenterPiles[i];
            int centerPileIndex = centerPile.IndexOf(card);
            if (centerPileIndex != -1)
            {
                return Result.Successful(new CardLocation(CardPileName.Center, centerPileIndex, null, i));
            }
        }

        return Result.Error<CardLocation>("Card not found in gameState");
    }


    public static Result<(GameState updatedGameState, bool couldTopUp)> TryRequestTopUp(
        GameState gameState,
        int playerIndex, bool immediateTopUp = true)
    {
        var player = gameState.Players[playerIndex];
        var newGameState = gameState;
        
        // Check player isn't already topped up
        if (player.RequestingTopUp)
        {
            return Result.Error<(GameState updatedGameState, bool couldTopUp)>("Already requesting to top up");
        }

        // Check the player can top up
        Result requestTopUpResult = CanRequestTopUp(gameState, playerIndex);
        if (requestTopUpResult is ErrorResult requestTopUpResultError)
        {
            return Result.Error<(GameState, bool)>(
                requestTopUpResultError.Message);
        }

        // Apply the top up request
        var newPlayer = player with {RequestingTopUp = true};
        var newPlayers = gameState.Players.ReplaceElementAt(playerIndex, newPlayer).ToImmutableList();
        newGameState = newGameState with {Players = newPlayers};
        
        // Check if we can top up now
        Result canTopUpResult = CanTopUp(newGameState);

        // If we want to immediately top up and we can then do it
        if (immediateTopUp && canTopUpResult.Success)
        {
            Result<GameState> topUpResult = TryTopUp(newGameState);
            return Result.Successful((topUpResult.Data ,true));
        }

        // Otherwise just return the gameState with the new top up request 
        return Result.Successful(
            (newGameState, canTopUpResult.Success));
    }

    // BotRunner Helpers
    public static Result<(Card card, int centerPile)> PlayerHasPlay(GameState gameState, int playerIndex)
    {
        var player = gameState.Players[playerIndex];

        foreach (Card card in player.HandCards)
        {
            Result<int> cardHasPlayResult = CardHasPlay(gameState, card);
            if (cardHasPlayResult.Success)
            {
                return Result.Successful((card, cardHasPlayResult.Data));
            }
        }

        return Result.Error<(Card card, int centerPile)>("No valid play for player");
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
                return Result.Successful(i);
            }
        }

        return Result.Error<int>("Card can't be played onto any center pile");
    }

    public static Result<int> CardWithValueIsInCenterPile(GameState gameState, int value)
    {
        for (var i = 0; i < gameState.CenterPiles.Count; i++)
        {
            if (gameState.CenterPiles[i].Count < 1) continue;
            Card pileCard = gameState.CenterPiles[i].Last();
            if (pileCard.Value == value)
            {
                return Result.Successful(i);
            }
        }

        return Result.Error<int>("Card with value not found in center pile");
    }

    private static Result CanRequestTopUp(GameState gameState, int playerIndex)
    {
        // Check the player can't play
        Result<(Card card, int centerPile)> hasPlayResult = PlayerHasPlay(gameState, playerIndex);
        if (hasPlayResult.Success) return Result.Error("Player can play a card");

        // Check the player can't pickup
        Result<(GameState updatedGameState, Card pickedUpCard)> pickupFromKittyResult =
            TryPickupFromKitty(gameState, playerIndex);
        if (pickupFromKittyResult.Success) return Result.Error("Player can pickup from kitty");

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