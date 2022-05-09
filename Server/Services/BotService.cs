namespace Server.Services;

using System.Collections.Concurrent;
using System.Linq;
using Engine;
using Engine.Helpers;
using Engine.Models;
using Hubs;
using Microsoft.AspNetCore.SignalR;
using Models.Database;
using Newtonsoft.Json;

public class BotService : IBotService
{
    public record Bot(int PlayerIndex, WebBotData Data);

    private readonly IServiceScopeFactory scopeFactory;
    private readonly GameEngine gameEngine;

    public BotService(IServiceScopeFactory scopeFactory, GameEngine gameEngine)
    {
        this.scopeFactory = scopeFactory;
        this.gameEngine = gameEngine;
    }

    public async Task RunBot(
        int playerIndex,
        BotType botType,
        BotActionHandler actionHandler,
        CancellationToken cancellationToken
    )
    {
        var botData = BotConfigurations.GetBot(botType);
        await SeedBot(botData);
        var bot = new Bot(playerIndex, botData);
        _ = RunBot(bot, actionHandler, cancellationToken)
            .ContinueWith(
                (task, _) =>
                {
                    if (task.Exception != null)
                    {
                        Console.WriteLine(task.Exception);
                    }
                },
                null,
                cancellationToken
            );
    }

    private async Task RunBot(
        Bot bot,
        BotActionHandler actionHandler,
        CancellationToken cancellationToken
    )
    {
        var random = new Random();

        // Wait for the cards to animate in
        await Task.Delay(4000, cancellationToken);
        var centerPilesAvailable = new List<bool> { true, true };
        while (
            !cancellationToken.IsCancellationRequested
            && (actionHandler.GetGameState()).WinnerIndex == null
        )
        {
            var move = BotRunner.GetMove(
                gameEngine.Checks,
                actionHandler.GetGameState(),
                bot.PlayerIndex,
                centerPilesAvailable
            );
            if (move.Success)
            {
                await (
                    move.Data.Type switch
                    {
                        MoveType.PlayCard
                          => actionHandler.PlayCardAction(
                              new(move.Data.CardId.Value, move.Data.CenterPileIndex.Value)
                          ),
                        MoveType.PickupCard => actionHandler.PickupAction(),
                        MoveType.RequestTopUp => actionHandler.RequestTopUpAction(),
                        _ => throw new ArgumentOutOfRangeException()
                    }
                );
            }

            // Wait if we have just topped up
            var gameState = actionHandler.GetGameState();
            if (
                gameState.MoveHistory.Count > 0
                && gameState.MoveHistory.Last().Type == MoveType.TopUp
            )
            {
                await Task.Delay(1500, cancellationToken);
            }

            var turnDelay = random.Next(
                (int)bot.Data.QuickestResponseTimeMs,
                (int)bot.Data.SlowestResponseTimeMs
            );
            if (move.Success && move.Data.Type == MoveType.PickupCard)
            {
                turnDelay = (int)bot.Data.PickupIntervalMs;
            }

            var centerTotals = GetCenterPileTotals(actionHandler.GetGameState());
            await Task.Delay(turnDelay, cancellationToken);

            // If a pile was updated while the bot was thinking then it can't be played onto
            var updatedCenterTotals = GetCenterPileTotals(actionHandler.GetGameState());
            centerPilesAvailable = centerTotals
                .Select((ct, i) => ct == updatedCenterTotals[i])
                .ToList();
        }
    }

    private async Task SeedBot(WebBotData botData)
    {
        using var scope = scopeFactory.CreateScope();
        var gameResultContext = scope.ServiceProvider.GetRequiredService<GameResultContext>();

        var bot = await gameResultContext.Players.FindAsync(botData.PersistentId);
        if (bot == null)
        {
            bot = new PlayerDao
            {
                Id = botData.PersistentId,
                Name = botData.Name,
                IsBot = true
            };
            gameResultContext.Players.Add(bot);
        }

        bot.Elo = (int)botData.Elo;
        await gameResultContext.SaveChangesAsync();
    }

    private List<int> GetCenterPileTotals(GameState gameState) =>
        gameState.CenterPiles.Select(cp => cp.Cards.Count).ToList();
}
