namespace Engine;

using System.Collections.Immutable;
using Helpers;
using Models;

public class GameEngine
{
    // Constants
    public const int PlayersPerGame = 2;
    public const int CardsPerSuit = 13;
    public const int CardsInDeck = 52;

    // The default max number of cards in a players hand
    public const int MaxHandCardsBase = 5;
    public const int CardsInKitty = 15;
    public const int CardsInTopUp = 5;

    private readonly EngineActions actions;
    public readonly EngineChecks Checks;

    public GameEngine(EngineChecks? engineChecks = null, EngineActions? engineActions = null)
    {
        this.Checks = engineChecks ?? new EngineChecks();
        this.actions = engineActions ?? new EngineActions();
    }

    public GameState NewGame(List<string>? playerNames = null, Settings? settings = null)
    {
        settings ??= new Settings();

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
        deck.Shuffle(settings.RandomSeed);

        // Deal cards to players
        var players = new List<Player>();
        for (var i = 0; i < PlayersPerGame; i++)
        {
            players.Add(new Player
            {
                Id = i,
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


    /// <returns>Player Index</returns>
    public Result<int> TryGetWinner(GameState gameState) => this.Checks.TryGetWinner(gameState);

    public Result<GameState> TryRequestTopUp(GameState gameState, int playerId)
    {
        var player = gameState.GetPlayer(playerId);
        var newGameState = gameState;

        // Check the player can top up
        var requestTopUpResult = this.Checks.CanRequestTopUp(gameState, playerId);
        if (requestTopUpResult is ErrorResult requestTopUpResultError)
        {
            return Result.Error<GameState>(requestTopUpResultError.Message);
        }

        // Apply the top up request
        var newPlayer = player with {RequestingTopUp = true};
        var newPlayers = gameState.Players.ReplaceElementAt(gameState.Players.IndexOf(player), newPlayer)
            .ToImmutableList();
        newGameState = newGameState with {Players = newPlayers};
        newGameState = this.actions.UpdateLastMove(newGameState,
            new Move {Type = MoveType.RequestTopUp, PlayerId = playerId});

        // Try and top up
        var topUpResult = this.TryTopUp(newGameState);
        newGameState = topUpResult.Success ? topUpResult.Data : newGameState;

        // Otherwise just return the gameState with the new top up request
        return Result.Successful(newGameState);
    }


    private Result<GameState> TryTopUp(GameState gameState)
    {
        var newGameState = gameState;

        // Check we can top up
        var canTopUpResult = this.Checks.CanTopUp(newGameState);
        if (canTopUpResult is IErrorResult canTopUpError)
        {
            return Result.Error<GameState>(canTopUpError.Message);
        }

        // Make sure we have cards in our top up pile
        if (newGameState.Players[0].TopUpCards.Count <= 0)
        {
            var replenishResult = this.actions.ReplenishTopUpCards(newGameState);
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
        newGameState = this.actions.UpdateLastMove(newGameState, new Move {Type = MoveType.TopUp});

        return Result.Successful(newGameState);
    }

    public Result<GameState> TryPlayCard(GameState gameState, int playerId, int cardId, int centerPileIndex)
    {
        if (centerPileIndex >= gameState.CenterPiles.Count)
        {
            return Result.Error<GameState>($"No center pile found at index {centerPileIndex}");
        }

        var card = gameState.GetCard(cardId);
        var cardLocationResult = card.Location(gameState);
        if (cardLocationResult.Equals(default(CardLocation)))
        {
            return Result.Error<GameState>("Card not found in gameState");
        }

        if (cardLocationResult.PlayerId != playerId)
        {
            return Result.Error<GameState>(
                $"Player {gameState.GetPlayer(playerId)?.Name} does not have card {card.ToString(gameState.Settings)} in their hand");
        }

        switch (cardLocationResult.PileName)
        {
            case CardPileName.Hand:
                // Check that the card can be played onto the relevant center piles top card
                if (!this.Checks.ValidPlay(card, gameState.CenterPiles[centerPileIndex].Cards.Last()))
                {
                    return Result.Error<GameState>(
                        $"Card with value {card.ToString(gameState.Settings)} can't be played onto {gameState.CenterPiles[centerPileIndex].Cards.Last().ToString(gameState.Settings)})");
                }

                // This is a valid play so play it
                var playResult = this.actions.PlayCard(gameState, card, centerPileIndex);
                var newGameState = playResult.Success ? playResult.Data : gameState;

                if (playResult.Success)
                {
                    // Reset any ones request to top up if they can now move
                    var state = playResult.Data;
                    var newPlayers = state.Players.Select((player, i) =>
                        player.RequestingTopUp && this.Checks.PlayerHasPlay(state, i).Success
                            ? player with {RequestingTopUp = false}
                            : player).ToImmutableList();
                    newGameState = state with {Players = newPlayers};
                }

                return playResult.Success ? Result.Successful(newGameState) : playResult;

            default:
                return Result.Error<GameState>(
                    $"Can't play a card from the {cardLocationResult.PileName} pile");
        }
    }

    public Result<GameState> TryPickupFromKitty(
        GameState gameState, int playerId)
    {
        var newGameState = gameState;
        var player = gameState.GetPlayer(playerId);

        var canPickupResult = this.Checks.CanPickupFromKitty(newGameState, playerId);
        if (canPickupResult is ErrorResult canPickupResultError)
        {
            return Result.Error<GameState>(canPickupResultError.Message);
        }

        var newHandCards = player.HandCards;
        newHandCards = newHandCards.Add(player.KittyCards.Last());

        var newKittyCards = player.KittyCards;
        newKittyCards = newKittyCards.Remove(player.KittyCards.Last());
        var newPlayer = player with {HandCards = newHandCards, KittyCards = newKittyCards};
        var newPlayers = newGameState.Players.ReplaceElementAt(newGameState.Players.IndexOf(player), newPlayer)
            .ToImmutableList();
        newGameState = newGameState with {Players = newPlayers};

        // Add the move to the history
        newGameState = this.actions.UpdateLastMove(newGameState,
            new Move {Type = MoveType.PickupCard, PlayerId = player.Id, CardId = newPlayer.HandCards.Last().Id});

        return Result.Successful(newGameState);
    }
}
