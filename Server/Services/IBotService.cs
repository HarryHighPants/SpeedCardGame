namespace Server.Services;

using Engine.Models;

public interface IBotService
{
    public Task RunBot(
        int playerIndex,
        BotType botType,
        BotActionHandler actionHandler,
        CancellationToken cancellationToken
    );
    
    public Task SeedBot(
	    BotType botType
    );
}

public record BotActionHandler(
    Func<GameState> GetGameState,
    Func<PlayCardRequest, Task> PlayCardAction,
    Func<Task> PickupAction,
    Func<Task> RequestTopUpAction
);

public record PlayCardRequest(int CardId, int CenterPilIndex);
