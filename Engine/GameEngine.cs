namespace Engine;

using System.Collections.Immutable;
using Helpers;

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
            for (var i = 0; i < PlayersPerGame; i++)
            {
                playerNames.Add($"Player {i + 1}");
            }
        }

        // Create deck
        var deck = new List<Card>();
        for (var i = 0; i < CardsInDeck; i++)
        {
            var suit = (Suit)(i / CardsPerSuit);
            var value = i % CardsPerSuit;
            var newCard = new Card {Id = i, Suit = suit, CardValue = (CardValue)value};
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
            CenterPiles =
                new List<ImmutableList<Card>>
                {
                    deck.PopRange(1).ToImmutableList(), deck.PopRange(1).ToImmutableList()
                }.ToImmutableList(),
            Settings = settings,
            MoveHistory = ImmutableList<MoveData>.Empty
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

    public static string CardsToString(IReadOnlyList<Card> cards, bool minified = false, bool includeSuit = false) =>
        string.Join(", ", cards.Select(c => CardToString(c, minified, includeSuit)));

    public static string CardToString(Card card, bool minified = false, bool includeSuit = false)
    {
        var value = minified ? ((int)card.CardValue).ToString() : card.CardValue.ToString();
        if (!includeSuit)
        {
            return value;
        }

        var joiner = minified ? "" : " of ";
        var suit = minified ? card.Suit.ToString().ToLower()[0].ToString() : card.Suit.ToString();
        return $"{value}{joiner}{suit}";
    }

    public static string CardToString(GameState gameState, int? cardId, bool minified = false, bool includeSuit = false)
    {
        var cardResult = GetCard(gameState, cardId);
        return CardToString(cardResult.Data, minified, includeSuit);
    }

    # region Top Up

    public static Result<GameState> TryRequestTopUp(GameState gameState, int playerIndex)
    {
        var player = gameState.Players[playerIndex];
        var newGameState = gameState;

        // Check the player can top up
        var requestTopUpResult = CanRequestTopUp(gameState, playerIndex);
        if (requestTopUpResult is ErrorResult requestTopUpResultError)
        {
            return Result.Error<GameState>(requestTopUpResultError.Message);
        }

        // Apply the top up request
        var newPlayer = player with {RequestingTopUp = true};
        var newPlayers = gameState.Players.ReplaceElementAt(playerIndex, newPlayer).ToImmutableList();
        newGameState = newGameState with {Players = newPlayers};

        // Try and top up
        var topUpResult = TryTopUp(newGameState);
        newGameState = topUpResult.Success ? topUpResult.Data : newGameState;

        // Otherwise just return the gameState with the new top up request
        return Result.Successful(newGameState);
    }

    private static Result CanRequestTopUp(GameState gameState, int playerIndex)
    {
        // Check player isn't already topped up
        if (gameState.Players[playerIndex].RequestingTopUp)
        {
            return Result.Error("Already requesting to top up");
        }

        // Check the player can't play
        var hasPlayResult = PlayerHasPlay(gameState, playerIndex);
        if (hasPlayResult.Success)
        {
            return Result.Error("Player can play a card");
        }

        // Check the player can't pickup
        var pickupFromKittyResult =
            TryPickupFromKitty(gameState, playerIndex);
        if (pickupFromKittyResult.Success)
        {
            return Result.Error("Player can pickup from kitty");
        }

        // We can't do anything else so top up is valid
        return new SuccessResult();
    }


    private static Result<GameState> TryTopUp(GameState gameState)
    {
        var newGameState = gameState;

        // Check we can top up
        var canTopUpResult = CanTopUp(newGameState);
        if (canTopUpResult is IErrorResult canTopUpError)
        {
            return Result.Error<GameState>(canTopUpError.Message);
        }

        // Make sure we have cards in our top up pile
        if (newGameState.Players[0].TopUpCards.Count <= 0)
        {
            var replenishResult = ReplenishTopUpCards(newGameState);
            if (replenishResult is ErrorResult<GameState> replenishError)
            {
                return replenishError;
            }

            newGameState = replenishResult.Data;
        }

        // Move each top up card to their respective center piles
        var state = newGameState;
        var newCenterPiles =
            newGameState.CenterPiles
                .Select((pile, i) => pile.Append(state.Players[i].TopUpCards.Last()).ToImmutableList())
                .ToImmutableList();
        newGameState = newGameState with {CenterPiles = newCenterPiles};

        // Remove the last top up card from each player
        var newPlayers = newGameState.Players.Select(player =>
                player with {RequestingTopUp = false, TopUpCards = player.TopUpCards.SkipLast(1).ToImmutableList()})
            .ToImmutableList();
        newGameState = newGameState with {Players = newPlayers};

        // Add the history
        newGameState = UpdateLastMove(newGameState, new MoveData {Move = MoveType.TopUp});

        return Result.Successful(newGameState);
    }

    private static Result CanTopUp(GameState gameState)
    {
        // Check all players are requesting top up
        var allPlayersRequestingTopUp = AllPlayersRequestingTopUp(gameState);
        if (allPlayersRequestingTopUp is ErrorResult allPlayersRequestingTopUpError)
        {
            return Result.Error(allPlayersRequestingTopUpError.Message);
        }

        return new SuccessResult();
    }

    private static Result<GameState> ReplenishTopUpCards(GameState gameState)
    {
        // combine middle piles
        var combinedCenterPiles = gameState.CenterPiles.SelectMany(cp => cp, (list, card) => card).ToList();

        if (combinedCenterPiles.Count < 1)
        {
            return Result.Error<GameState>("Can't replenish top up piles without any center cards");
        }

        // Shuffle them
        combinedCenterPiles.Shuffle(gameState.Settings?.RandomSeed);

        // Split all but center piles count into top up piles
        var topUpPileSize = combinedCenterPiles.Count / gameState.Players.Count;
        var newPlayers =
            gameState.Players.Select(player =>
                    player with {TopUpCards = combinedCenterPiles.PopRange(topUpPileSize).ToImmutableList()})
                .ToImmutableList();

        // Reset the center piles
        var newCenterPiles = new List<ImmutableList<Card>>();
        for (var i = 0; i < gameState.CenterPiles.Count; i++)
        {
            newCenterPiles.Add(ImmutableList<Card>.Empty);
        }

        return Result.Successful(gameState with {Players = newPlayers, CenterPiles = newCenterPiles.ToImmutableList()});
    }

    private static Result AllPlayersRequestingTopUp(GameState gameState) =>
        // Check all players are requesting top up
        !gameState.Players.All(p => p.RequestingTopUp)
            ? Result.Error("Not all players are requesting top up")
            : new SuccessResult();

    #endregion

    #region Play Card

    public static Result<GameState> TryPlayCard(GameState gameState, int playerIndex, Card card,
        int centerPileIndex)
    {
        if (centerPileIndex >= gameState.CenterPiles.Count)
        {
            return Result.Error<GameState>($"No center pile found at index {centerPileIndex}");
        }

        var cardLocationResult = FindCardLocation(gameState, card);
        if (cardLocationResult is IErrorResult cardLocationResultError)
        {
            return Result.Error<GameState>(cardLocationResultError.Message);
        }

        if (cardLocationResult.Data.PlayerIndex != playerIndex)
        {
            return Result.Error<GameState>(
                $"Player {gameState.Players[playerIndex].Name} does not have card {card.CardValue} in their hand");
        }

        switch (cardLocationResult.Data.PileName)
        {
            case CardPileName.Hand:
                // Check that the card can be played onto the relevant center piles top card
                if (!ValidMove(card, gameState.CenterPiles[centerPileIndex].Last()))
                {
                    return Result.Error<GameState>(
                        $"Card with value {card.CardValue}({(int)card.CardValue}) can't be played onto {gameState.CenterPiles[centerPileIndex].Last().CardValue}({(int)gameState.CenterPiles[centerPileIndex].Last().CardValue})");
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

        // Remove the played card from the players hand
        var (playerWithCard, playerIndexWithCard) =
            newGameState.Players.IndexTuples().First(p => p.item.HandCards.Contains(card));
        var newHandCards = playerWithCard.HandCards.Where(c => c != card).ToImmutableList();
        var newPlayer = playerWithCard with {HandCards = newHandCards};
        var newPlayers = newGameState.Players.ReplaceElementAt(playerIndexWithCard, newPlayer).ToImmutableList();
        newGameState = newGameState with {Players = newPlayers};

        // Reset any ones request to top up if they can now move
        var state = newGameState;
        newPlayers = newPlayers.Select((player, i) =>
            player.RequestingTopUp && PlayerHasPlay(state, i).Success
                ? player with {RequestingTopUp = false}
                : player).ToImmutableList();
        newGameState = newGameState with {Players = newPlayers};

        // Add the move history
        newGameState = UpdateLastMove(newGameState,
            new MoveData
            {
                Move = MoveType.PlayCard,
                CardId = card.Id,
                PlayerId = playerWithCard.Id,
                CenterPileIndex = centerPileIndex
            });

        return Result.Successful(newGameState);
    }

    public static Result<(Card card, int centerPile)> PlayerHasPlay(GameState gameState, int playerIndex)
    {
        var player = gameState.Players[playerIndex];

        foreach (var card in player.HandCards)
        {
            var cardHasPlayResult = CardHasPlay(gameState, card);
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
            if (gameState.CenterPiles[i].Count < 1)
            {
                continue;
            }

            var pileCard = gameState.CenterPiles[i].Last();
            if (ValidMove(card, pileCard))
            {
                return Result.Successful(i);
            }
        }

        return Result.Error<int>("Card can't be played onto any center pile");
    }

    public static bool ValidMove(Card? topCard, Card? bottomCard)
    {
        if (topCard == null || bottomCard == null)
        {
            return false;
        }

        if (topCard.CardValue < 0 || bottomCard.CardValue < 0)
        {
            return false;
        }

        var valueDiff = Math.Abs(topCard.CardValue - bottomCard.CardValue);
        return valueDiff is 1 or CardsPerSuit - 1;
    }

    #endregion

    #region Pickup

    public static Result<(GameState updatedGameState, Card pickedUpCard)> TryPickupFromKitty(
        GameState gameState, int playerIndex)
    {
        var newGameState = gameState;
        var player = newGameState.Players[playerIndex];

        var canPickupResult = CanPickupFromKitty(newGameState, playerIndex);
        if (canPickupResult is ErrorResult canPickupResultError)
        {
            return Result.Error<(GameState updatedGameState, Card pickedUpCard)>(canPickupResultError.Message);
        }

        var newHandCards = player.HandCards.ToImmutableList();
        newHandCards = newHandCards.Add(player.KittyCards.Last());

        var newKittyCards = player.KittyCards.ToImmutableList();
        newKittyCards = newKittyCards.Remove(player.KittyCards.Last());
        var newPlayer = player with {HandCards = newHandCards, KittyCards = newKittyCards};
        var newPlayers = newGameState.Players.ReplaceElementAt(playerIndex, newPlayer).ToImmutableList();
        newGameState = newGameState with {Players = newPlayers};

        // Add the move to the history
        newGameState = UpdateLastMove(newGameState,
            new MoveData {Move = MoveType.PickupCard, PlayerId = player.Id, CardId = newPlayer.HandCards.Last().Id});

        return Result.Successful((newGameState, newPlayer.HandCards.Last()));
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
        if (player.KittyCards.Count < 1)
        {
            return Result.Error($"No cards left in {player.Name} kitty to pickup");
        }

        return new SuccessResult();
    }

    #endregion

    #region Helpers

    public static Result<int> CardWithValueIsInCenterPile(GameState gameState, int value)
    {
        for (var i = 0; i < gameState.CenterPiles.Count; i++)
        {
            if (gameState.CenterPiles[i].Count < 1)
            {
                continue;
            }

            var pileCard = gameState.CenterPiles[i].Last();
            if ((int)pileCard.CardValue == value)
            {
                return Result.Successful(i);
            }
        }

        return Result.Error<int>("Card with value not found in center pile");
    }

    private static Result<Card> GetCard(
        GameState gameState, int? cardId)
    {
        foreach (var player in gameState.Players)
        {
            var handCard = player.HandCards.FirstOrDefault(c => c.Id == cardId);
            if (handCard != default)
            {
                return Result.Successful(handCard);
            }

            var kittyCard = player.KittyCards.FirstOrDefault(c => c.Id == cardId);
            if (kittyCard != default)
            {
                return Result.Successful(kittyCard);
            }

            var topUpCard = player.TopUpCards.FirstOrDefault(c => c.Id == cardId);
            if (topUpCard != default)
            {
                return Result.Successful(topUpCard);
            }
        }

        for (var i = 0; i < gameState.CenterPiles.Count; i++)
        {
            var centerPile = gameState.CenterPiles[i];

            var centerCard = centerPile.FirstOrDefault(c => c.Id == cardId);
            if (centerCard != default)
            {
                return Result.Successful(centerCard);
            }
        }

        return Result.Error<Card>("Card not found in gameState");
    }

    private static Result<Player> GetPlayer(GameState gameState, int? playerId)
    {
        var playerResult = gameState.Players.FirstOrDefault(p => p.Id == playerId);
        if (playerResult != default)
        {
            return Result.Successful(playerResult);
        }

        return Result.Error<Player>("Player not found in gameState");
    }

    private struct CardLocation
    {
        public readonly CardPileName PileName;
        public int PileIndex;
        public readonly int? PlayerIndex;
        public int? CenterIndex;

        public CardLocation(CardPileName pileName, int pileIndex, int? playerIndex, int? centerIndex)
        {
            this.PileName = pileName;
            this.PileIndex = pileIndex;
            this.PlayerIndex = playerIndex;
            this.CenterIndex = centerIndex;
        }
    }

    private static Result<CardLocation> FindCardLocation(
        GameState gameState, Card card)
    {
        for (var i = 0; i < gameState.Players.Count; i++)
        {
            var player = gameState.Players[i];
            var handIndex = player.HandCards.IndexOf(card);
            if (handIndex != -1)
            {
                return Result.Successful(new CardLocation(CardPileName.Hand, handIndex, i, null));
            }

            var kittyIndex = player.KittyCards.IndexOf(card);
            if (kittyIndex != -1)
            {
                return Result.Successful(new CardLocation(CardPileName.Kitty, kittyIndex, i, null));
            }

            var topUpIndex = player.TopUpCards.IndexOf(card);
            if (topUpIndex != -1)
            {
                return Result.Successful(new CardLocation(CardPileName.TopUp, topUpIndex, i, null));
            }
        }

        for (var i = 0; i < gameState.CenterPiles.Count; i++)
        {
            var centerPile = gameState.CenterPiles[i];
            var centerPileIndex = centerPile.IndexOf(card);
            if (centerPileIndex != -1)
            {
                return Result.Successful(new CardLocation(CardPileName.Center, centerPileIndex, null, i));
            }
        }

        return Result.Error<CardLocation>("Card not found in gameState");
    }

    #endregion

    #region Updating History

    private static GameState UpdateLastMove(GameState gameState, MoveData data)
    {
        var newMoveHistory = gameState.MoveHistory.Add(data);
        var newGameState = gameState with {MoveHistory = newMoveHistory};
        return newGameState;
    }

    public static string ReadableLastMove(GameState gameState, bool minified = false, bool includeSuit = false)
    {
        if (gameState.MoveHistory.Count < 1)
        {
            return "";
        }

        var lastMove = gameState.MoveHistory.Last();
        return lastMove.Move switch
        {
            MoveType.PickupCard =>
                $"{GetPlayer(gameState, lastMove.PlayerId).Data.Name} picked up card {CardToString(gameState, lastMove.CardId, minified, includeSuit)}",
            MoveType.PlayCard =>
                $"{GetPlayer(gameState, lastMove.PlayerId).Data.Name} played card {CardToString(gameState, lastMove.CardId, minified, includeSuit)} onto pile {lastMove.CenterPileIndex + 1}",
            MoveType.TopUp => "Center cards were topped up",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    #endregion
}
