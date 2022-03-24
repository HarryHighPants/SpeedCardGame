namespace Engine;

using Helpers;
using Models;

public class Game
{
    public Game(List<string>? playerNames = null, Settings? settings = null) =>
        this.State = GameEngine.NewGame(playerNames, settings);

    public GameState State { get; private set; }

    public Result<Player> TryGetWinner()
    {
        var result = GameEngine.TryGetWinner(this.State);
        if (result.Success)
        {
            return new SuccessResult<Player>(this.State.GetPlayer(result.Data) ??
                                             throw new InvalidOperationException());
        }

        return Result.Error<Player>("No winner yet");
    }

    public Result TryPickupFromKitty(int playerId)
    {
        var result = GameEngine.TryPickupFromKitty(this.State, playerId);
        this.State = result.Success ? result.Data : this.State;
        return result as Result;
    }

    public Result TryPlayCard(int playerId, int cardId, int centerPileIndex)
    {
        var result = GameEngine.TryPlayCard(this.State, playerId, cardId, centerPileIndex);
        this.State = result.Success ? result.Data : this.State;
        return result as Result;
    }

    public Result TryRequestTopUp(int playerId)
    {
        var result = GameEngine.TryRequestTopUp(this.State, playerId);
        this.State = result.Success ? result.Data : this.State;
        return result as Result;
    }
}
