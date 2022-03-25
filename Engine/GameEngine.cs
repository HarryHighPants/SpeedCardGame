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
        return this.actions.NewGame(playerNames, settings);
    }


    /// <returns>Player Index</returns>
    public Result<int> TryGetWinner(GameState gameState) => this.Checks.TryGetWinner(gameState);

    public Result<GameState> TryPlayCard(GameState gameState, int playerId, int cardId, int centerPileIndex)
    {
        // Check - Move is valid
        var moveValid = this.Checks.PlayersMoveValid(gameState, playerId, cardId, centerPileIndex);
        if (moveValid is ErrorResult moveValidError)
        {
            return Result.Error<GameState>(moveValidError.Message);
        }

        // Action - Play card
        var card = gameState.GetCard(cardId);
        var newGameState = this.actions.PlayCard(gameState, card, centerPileIndex);

        // Action - Reset anyone's requesting top up if they can now play
        var state = newGameState;
        var newPlayers = state.Players.Select((player, i) =>
            player.RequestingTopUp && this.Checks.PlayerHasPlay(state, i).Success
                ? player with {RequestingTopUp = false}
                : player).ToImmutableList();
        newGameState = state with {Players = newPlayers};

        return Result.Successful(newGameState);
    }

    public Result<GameState> TryRequestTopUp(GameState gameState, int playerId)
    {
        // Check - Player can top up
        var requestTopUpResult = this.Checks.CanRequestTopUp(gameState, playerId);
        if (requestTopUpResult is ErrorResult requestTopUpResultError)
        {
            return Result.Error<GameState>(requestTopUpResultError.Message);
        }

        // Action - Request top up
        var newGameState = this.actions.RequestTopUp(gameState, playerId);

        // Action - Top up if possible
        var topUpResult = this.TryTopUp(newGameState);
        newGameState = topUpResult.Success ? topUpResult.Data : newGameState;

        return Result.Successful(newGameState);
    }

    public Result<GameState> TryPickupFromKitty(GameState gameState, int playerId)
    {
        var newGameState = gameState;

        // Check - Player can pickup from kitty
        var canPickupResult = this.Checks.CanPickupFromKitty(newGameState, playerId);
        if (canPickupResult is ErrorResult canPickupResultError)
        {
            return Result.Error<GameState>(canPickupResultError.Message);
        }

        // Action - Pickup card
        return Result.Successful(this.actions.PickupCard(gameState, playerId));
    }

    // Private
    private Result<GameState> TryTopUp(GameState gameState)
    {
        // Check - We can top up
        var canTopUpResult = this.Checks.CanTopUp(gameState);
        if (canTopUpResult is IErrorResult canTopUpError)
        {
            return Result.Error<GameState>(canTopUpError.Message);
        }

        // Action - Top Up
        return this.actions.TopUp(gameState);
    }
}
