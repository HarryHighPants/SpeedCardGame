namespace Engine;

using Helpers;
using Models;

public abstract class Game
{
    public Game(List<string>? playerNames, Settings settings, GameEngine gameEngine)
    {
        this.gameEngine = gameEngine;

        var result = gameEngine.NewGame(playerNames, settings);
        State = result.Success ? result.Data : State;
    }

    public GameEngine gameEngine { get; protected set; }

    private readonly object stateLock = new object();
    public GameState State { get; protected set; }

    public Result<Player> TryGetWinner()
    {
        lock (stateLock)
        {
            var result = gameEngine.TryGetWinner(State);
            if (result.Success)
            {
                return new SuccessResult<Player>(
                    State.GetPlayer(result.Data) ?? throw new InvalidOperationException()
                );
            }

            return Result.Error<Player>("No winner yet");
        }
    }

    public Result<GameState> TryPickupFromKitty(int playerId)
    {
        lock (stateLock)
        {
            var result = gameEngine.TryPickupFromKitty(State, playerId);
            State = result.Success ? result.Data : State;
            return result;
        }
    }

    public Result<GameState> TryPlayCard(int playerId, int cardId, int centerPileIndex)
    {
        lock (stateLock)
        {
            var result = gameEngine.TryPlayCard(State, playerId, cardId, centerPileIndex);
            State = result.Success ? result.Data : State;
            return result;
        }
    }

    public Result<GameState> TryRequestTopUp(int playerId)
    {
        lock (stateLock)
        {
            var result = gameEngine.TryRequestTopUp(State, playerId);
            State = result.Success ? result.Data : State;
            return result;
        }
    }
}