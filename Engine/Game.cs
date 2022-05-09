namespace Engine;

using Helpers;
using Models;

public abstract class Game
{

    public Game(List<string> playerNames, Settings settings, GameEngine gameEngine)
    {
	    this.gameEngine = gameEngine;
	    State = this.gameEngine.NewGame(playerNames, settings);
    }

    public GameEngine gameEngine { get; protected set; }

    public GameState State { get; protected set; }

    public Result<Player> TryGetWinner()
    {
        var result = gameEngine.TryGetWinner(State);
        if (result.Success)
        {
            return new SuccessResult<Player>(State.GetPlayer(result.Data) ??
                                             throw new InvalidOperationException());
        }

        return Result.Error<Player>("No winner yet");
    }

    public Result<GameState> TryPickupFromKitty(int playerId)
    {
        var result = gameEngine.TryPickupFromKitty(State, playerId);
        State = result.Success ? result.Data : State;
        return result;
    }

    public Result<GameState> TryPlayCard(int playerId, int cardId, int centerPileIndex)
    {
        var result = gameEngine.TryPlayCard(State, playerId, cardId, centerPileIndex);
        State = result.Success ? result.Data : State;
        return result;
    }

    public Result<GameState> TryRequestTopUp(int playerId)
    {
        var result = gameEngine.TryRequestTopUp(State, playerId);
        State = result.Success ? result.Data : State;
        return result;
    }
}
