namespace Engine;

using Helpers;
using Models;

public class Game
{
    public readonly GameEngine gameEngine;

    public Game(List<string>? playerNames = null, Settings? settings = null, GameEngine? gameEngine = null)
    {
        this.gameEngine = gameEngine ?? new GameEngine();
        this.State = this.gameEngine.NewGame(playerNames, settings);
    }

    public GameState State { get; private set; }

    public Result<Player> TryGetWinner()
    {
        var result = this.gameEngine.TryGetWinner(this.State);
        if (result.Success)
        {
            return new SuccessResult<Player>(this.State.GetPlayer(result.Data) ??
                                             throw new InvalidOperationException());
        }

        return Result.Error<Player>("No winner yet");
    }

    public Result TryPickupFromKitty(int playerId)
    {
        var result = this.gameEngine.TryPickupFromKitty(this.State, playerId);
        this.State = result.Success ? result.Data : this.State;
        return result as Result;
    }

    public Result TryPlayCard(int playerId, int cardId, int centerPileIndex)
    {
        var result = this.gameEngine.TryPlayCard(this.State, playerId, cardId, centerPileIndex);
        this.State = result.Success ? result.Data : this.State;
        return result as Result;
    }

    public Result TryRequestTopUp(int playerId)
    {
        var result = this.gameEngine.TryRequestTopUp(this.State, playerId);
        this.State = result.Success ? result.Data : this.State;
        return result as Result;
    }
}
