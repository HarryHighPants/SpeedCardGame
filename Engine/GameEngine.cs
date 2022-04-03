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
        Checks = engineChecks ?? new EngineChecks();
        actions = engineActions ?? new EngineActions();
    }

    public GameState NewGame(List<string>? playerNames = null, Settings? settings = null) =>
        actions.NewGame(playerNames, settings);


    /// <returns>Player Index</returns>
    public Result<int> TryGetWinner(GameState gameState) => Checks.TryGetWinner(gameState);

    public Result<GameState> TryPlayCard(GameState gameState, int playerId, int cardId, int centerPileIndex)
    {
        // Check - Move is valid
        var moveValid = Checks.PlayersMoveValid(gameState, playerId, cardId, centerPileIndex);
        if (moveValid is IErrorResult moveValidError)
        {
            return Result.Error<GameState>(moveValidError.Message);
        }

        // Action - Play card
        var card = gameState.GetCard(cardId);
        var newGameState = actions.PlayCard(gameState, card, centerPileIndex);

        // Action - Reset anyone's requesting top up if they can now play
        var state = newGameState;
        var newPlayers = state.Players.Select((player, i) =>
            player.RequestingTopUp && Checks.PlayerHasPlay(state, i).Success
                ? player with {RequestingTopUp = false}
                : player).ToImmutableList();
        newGameState = state with {Players = newPlayers};

        return Result.Successful(newGameState);
    }

    public Result<GameState> TryRequestTopUp(GameState gameState, int playerId)
    {
        // Check - Player can top up
        var requestTopUpResult = Checks.CanRequestTopUp(gameState, playerId);
        if (requestTopUpResult is ErrorResult requestTopUpResultError)
        {
            return Result.Error<GameState>(requestTopUpResultError.Message);
        }

        // Action - Request top up
        var newGameState = actions.RequestTopUp(gameState, playerId);

        // Action - Top up if possible
        var topUpResult = TryTopUp(newGameState);
        newGameState = topUpResult.Success ? topUpResult.Data : newGameState;

        return Result.Successful(newGameState);
    }

    public Result<GameState> TryPickupFromKitty(GameState gameState, int playerId)
    {
        var newGameState = gameState;

        // Check - Player can pickup from kitty
        var canPickupResult = Checks.CanPickupFromKitty(newGameState, playerId);
        if (canPickupResult is ErrorResult canPickupResultError)
        {
            return Result.Error<GameState>(canPickupResultError.Message);
        }

        // Action - Pickup card
        return Result.Successful(actions.PickupCard(gameState, playerId));
    }

    // Private
    private Result<GameState> TryTopUp(GameState gameState)
    {
        // Check - We can top up
        var canTopUpResult = Checks.CanTopUp(gameState);
        if (canTopUpResult is IErrorResult canTopUpError)
        {
            return Result.Error<GameState>(canTopUpError.Message);
        }

        // Action - Top Up
        return actions.TopUp(gameState);
    }
}
