namespace Engine;

using System.Collections.Immutable;
using Helpers;
using Models;

public class EngineActions
{
	public GameState NewGame(List<string>? playerNames = null, Settings? settings = null)
	{
		settings ??= new Settings();

		// Initialise player names if none were supplied
		if (playerNames == null)
		{
			playerNames = new List<string>();
			for (var i = 0; i < GameEngine.PlayersPerGame; i++)
			{
				playerNames.Add($"Player {i + 1}");
			}
		}

		// Create deck
		var deck = new List<Card>();
		for (var i = 0; i < GameEngine.CardsInDeck; i++)
		{
			var suit = (Suit)(i / GameEngine.CardsPerSuit);
			var value = i % GameEngine.CardsPerSuit;
			var newCard = new Card {Id = i, Suit = suit, CardValue = (CardValue)value};
			deck.Add(newCard);
		}

		// Shuffle the new deck
		deck.Shuffle(settings.RandomSeed);

		// Deal cards to players
		var players = new List<Player>();
		for (var i = 0; i < GameEngine.PlayersPerGame; i++)
		{
			players.Add(new Player
			{
				Id = i,
				Name = playerNames?[i] ?? $"Player {i + 1}",
				HandCards = deck.PopRange(GameEngine.MaxHandCardsBase).ToImmutableList(),
				KittyCards = deck.PopRange(GameEngine.CardsInKitty).ToImmutableList(),
				TopUpCards = deck.PopRange(GameEngine.CardsInTopUp).ToImmutableList()
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

	public GameState UpdateLastMove(GameState gameState, Move data)
	{
		var newMoveHistory = gameState.MoveHistory.Add(data);
		var newGameState = gameState with {MoveHistory = newMoveHistory};
		var moveDescription = data.GetDescription(newGameState,
			gameState.Settings.MinifiedCardStrings,
			gameState.Settings.IncludeSuitInCardStrings);

		// Update the players last move
		var newPlayers = gameState.Players;
		if (data.PlayerId != null)
		{
			var playerIndex =
				gameState.Players.IndexOf(gameState.Players.FirstOrDefault(p => p.Id == data.PlayerId.Value));
			if (playerIndex != -1)
			{
				var newPlayer = gameState.Players[playerIndex] with {LastMove = moveDescription};
				newPlayers = newPlayers.Replace(gameState.Players[playerIndex], newPlayer);
			}
		}

		newGameState = newGameState with {LastMove = moveDescription, Players = newPlayers};
		return newGameState;
	}

	public GameState RequestTopUp(GameState gameState, int playerId)
	{
		// Update the player to requesting top up
		var player = gameState.GetPlayer(playerId);
		var newPlayer = player with {RequestingTopUp = true};
		var newPlayers = gameState.Players.ReplaceElementAt(gameState.Players.IndexOf(player), newPlayer)
			.ToImmutableList();
		var newGameState = gameState with {Players = newPlayers};

		// Add the move to the history
		newGameState = UpdateLastMove(newGameState, new Move(MoveType.RequestTopUp, playerId));

		return newGameState;
	}

	public Result<GameState> TopUp(GameState gameState)
	{
		var newGameState = gameState;
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

		// Add the move to the history
		newGameState = UpdateLastMove(newGameState, new Move(MoveType.TopUp));

		return Result.Successful(newGameState);
	}

	public Result<GameState> ReplenishTopUpCards(GameState gameState)
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

	public GameState PickupCard(GameState gameState, int playerId)
	{
		var newGameState = gameState;
		var player = gameState.GetPlayer(playerId);

		// Get the new hand cards for the Player
		var newHandCards = player.HandCards;
		newHandCards = newHandCards.Add(player.KittyCards.Last());

		// Get the new Kitty cards for the Player
		var newKittyCards = player.KittyCards;
		newKittyCards = newKittyCards.Remove(player.KittyCards.Last());

		// Create the new player with the updated cards
		var newPlayer = player with {HandCards = newHandCards, KittyCards = newKittyCards};
		var newPlayers = newGameState.Players.ReplaceElementAt(newGameState.Players.IndexOf(player), newPlayer)
			.ToImmutableList();
		newGameState = newGameState with {Players = newPlayers};

		// Add the move to the history
		newGameState = UpdateLastMove(newGameState,
			new Move(MoveType.PickupCard, player.Id, newPlayer.HandCards.Last().Id));

		return newGameState;
	}

	public GameState PlayCard(GameState gameState, Card card, int centerPileIndex)
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

		// Add the move to the history
		newGameState = UpdateLastMove(newGameState,
			new Move(
				MoveType.PlayCard,
				playerWithCard.Id,
				card.Id,
				centerPileIndex
			));

		return newGameState;
	}
}
