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

	private readonly EngineActions Actions;
	public readonly EngineChecks Checks;

	public GameEngine(EngineChecks engineChecks, EngineActions engineActions)
	{
		Checks = engineChecks;
		Actions = engineActions;
	}

	public GameState NewGame(List<string> playerNames, Settings settings) =>
		Actions.NewGame(playerNames, settings);


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
		var newGameState = Actions.PlayCard(gameState, card, centerPileIndex);


		// Action - Reset anyone's requesting top up if they can now play
		var state = newGameState;
		var newPlayers = state.Players.Select((player, i) =>
			player.RequestingTopUp && Checks.PlayerHasPlay(state, i).Success
				? player with {RequestingTopUp = false}
				: player).Select(player => player with
		{
			CanRequestTopUp = Checks.CanPlayerRequestTopUp(gameState, player.Id)
		}).ToImmutableList();
		newGameState = state with {Players = newPlayers};

		// Check - If the game has ended
		var winnerResult = Checks.TryGetWinner(gameState);
		if (winnerResult.Success)
		{
			newGameState = Actions.UpdateWinner(newGameState, winnerResult.Data);
		}

		Actions.SetCanRequestTopUp(newGameState, playerId, Checks.CanPlayerRequestTopUp(newGameState, playerId));

		return Result.Successful(newGameState);
	}

	public Result<GameState> TryRequestTopUp(GameState gameState, int playerId)
	{
		// Check - Player can top up
		if (!Checks.CanPlayerRequestTopUp(gameState, playerId))
		{
			return Result.Error<GameState>("Can't request top up");
		}

		// Action - Request top up
		var newGameState = Actions.RequestTopUp(gameState, playerId);

		// Action - Top up if possible
		var topUpResult = TryTopUp(newGameState);
		newGameState = topUpResult.Success ? topUpResult.Data : newGameState;

		Actions.SetCanRequestTopUp(newGameState, playerId, Checks.CanPlayerRequestTopUp(newGameState, playerId));

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
		return Result.Successful(Actions.PickupCard(gameState, playerId));
	}

	// Private
	private Result<GameState> TryTopUp(GameState gameState)
	{
		// Check - We can top up
		var canTopUpResult = Checks.AllPlayersRequestingTopUp(gameState);
		if (!canTopUpResult)
		{
			return Result.Error<GameState>("Can't Top up");
		}

		// Action - Top Up
		return Actions.TopUp(gameState);
	}
}
