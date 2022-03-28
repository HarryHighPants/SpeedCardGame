namespace Engine;

using Helpers;
using Models;

public class Game
{
    public Game(List<string>? playerNames = null, Settings? settings = null, GameEngine? gameEngine = null)
    {
        this.gameEngine = gameEngine ?? new GameEngine();
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

    public Result TryPickupFromKitty(int playerId)
    {
        var result = gameEngine.TryPickupFromKitty(State, playerId);
        State = result.Success ? result.Data : State;
        return result as Result;
    }

    public Result TryPlayCard(int playerId, int cardId, int centerPileIndex)
    {
        var result = gameEngine.TryPlayCard(State, playerId, cardId, centerPileIndex);
        State = result.Success ? result.Data : State;
        return result as Result;
    }

    public Result TryRequestTopUp(int playerId)
    {
        var result = gameEngine.TryRequestTopUp(State, playerId);
        State = result.Success ? result.Data : State;
        return result as Result;
    }
}
