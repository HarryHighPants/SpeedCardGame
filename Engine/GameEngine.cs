namespace Engine;

using System.Collections.Immutable;
using Helpers;
using Models;

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
                new List<CenterPile>
                {
                    new() {Cards = deck.PopRange(1).ToImmutableList()},
                    new() {Cards = deck.PopRange(1).ToImmutableList()}
                }.ToImmutableList(),
            Settings = settings,
            MoveHistory = ImmutableList<Move>.Empty
        };
        return gameState;
    }

    /// <summary>
    /// </summary>
    /// <param name="gameState"></param>
    /// <returns>Player Index</returns>
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

    #region Helpers

    public static Result<int> CardWithValueIsInCenterPile(GameState gameState, int value)
    {
        for (var i = 0; i < gameState.CenterPiles.Count; i++)
        {
            if (gameState.CenterPiles[i].Cards.Count < 1)
            {
                continue;
            }

            var pileCard = gameState.CenterPiles[i].Cards.Last();
            if ((int)pileCard.CardValue == value)
            {
                return Result.Successful(i);
            }
        }

        return Result.Error<int>("Card with value not found in center pile");
    }

    #endregion

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
                .Select((pile, i) => new CenterPile
                {
                    Cards = pile.Cards.Append(state.Players[i].TopUpCards.Last()).ToImmutableList()
                })
                .ToImmutableList();
        newGameState = newGameState with {CenterPiles = newCenterPiles};

        // Remove the last top up card from each player
        var newPlayers = newGameState.Players.Select(player =>
                player with {RequestingTopUp = false, TopUpCards = player.TopUpCards.SkipLast(1).ToImmutableList()})
            .ToImmutableList();
        newGameState = newGameState with {Players = newPlayers};

        // Add the history
        newGameState = UpdateLastMove(newGameState, new Move {Type = MoveType.TopUp});

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
        // Combine center piles
        var combinedCenterPiles = gameState.CenterPiles.SelectMany(cp => cp.Cards.ToList()).ToList();

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
        var newCenterPiles = new List<CenterPile>();
        for (var i = 0; i < gameState.CenterPiles.Count; i++)
        {
            newCenterPiles.Add(new CenterPile());
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

        var cardLocationResult = card.Location(gameState);
        if (cardLocationResult.Equals(default(CardLocation)))
        {
            return Result.Error<GameState>("Card not found in gameState");
        }

        if (cardLocationResult.PlayerIndex != playerIndex)
        {
            return Result.Error<GameState>(
                $"Player {gameState.Players[playerIndex].Name} does not have card {card.CardValue} in their hand");
        }

        switch (cardLocationResult.PileName)
        {
            case CardPileName.Hand:
                // Check that the card can be played onto the relevant center piles top card
                if (!ValidPlay(card, gameState.CenterPiles[centerPileIndex].Cards.Last()))
                {
                    return Result.Error<GameState>(
                        $"Card with value {card.CardValue}({(int)card.CardValue}) can't be played onto {gameState.CenterPiles[centerPileIndex].Cards.Last().CardValue}({(int)gameState.CenterPiles[centerPileIndex].Cards.Last().CardValue})");
                }

                // This is a valid play so play it
                return PlayCard(gameState, card, centerPileIndex);

            default:
                return Result.Error<GameState>(
                    $"Can't play a card from the {cardLocationResult.PileName} pile");
        }
    }

    private static Result<GameState> PlayCard(GameState gameState, Card card, int centerPileIndex)
    {
        var newGameState = gameState;

        // Add the card being played to the center pile
        var newCenterPiles = newGameState.CenterPiles.ReplaceElementAt(
                centerPileIndex,
                new CenterPile {Cards = gameState.CenterPiles[centerPileIndex].Cards.Append(card).ToImmutableList()})
            .ToImmutableList();
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
            new Move
            {
                Type = MoveType.PlayCard,
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
            if (gameState.CenterPiles[i].Cards.Count < 1)
            {
                continue;
            }

            var pileCard = gameState.CenterPiles[i].Cards.Last();
            if (ValidPlay(card, pileCard))
            {
                return Result.Successful(i);
            }
        }

        return Result.Error<int>("Card can't be played onto any center pile");
    }

    public static bool ValidPlay(Card? topCard, Card? bottomCard)
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

        var newHandCards = player.HandCards;
        newHandCards = newHandCards.Add(player.KittyCards.Last());

        var newKittyCards = player.KittyCards;
        newKittyCards = newKittyCards.Remove(player.KittyCards.Last());
        var newPlayer = player with {HandCards = newHandCards, KittyCards = newKittyCards};
        var newPlayers = newGameState.Players.ReplaceElementAt(playerIndex, newPlayer).ToImmutableList();
        newGameState = newGameState with {Players = newPlayers};

        // Add the move to the history
        newGameState = UpdateLastMove(newGameState,
            new Move {Type = MoveType.PickupCard, PlayerId = player.Id, CardId = newPlayer.HandCards.Last().Id});

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

    #region Updating History

    private static GameState UpdateLastMove(GameState gameState, Move data)
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

        return gameState.MoveHistory.Last().GetDescription(gameState, minified, includeSuit);
    }

    #endregion
}
