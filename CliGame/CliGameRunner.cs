namespace CliGame;

using Engine;
using Engine.Helpers;
using Engine.Models;
using Helpers;

public class CliGameRunner
{
    private readonly Random random = new();
    private CliGame Game { get; set; } = new();

    public void PlayGame(bool skipIntro = false)
    {
        // Intro
        CliGameIoHelper.GameIntro(skipIntro);

        Game = new CliGame(
            new List<string> { BotRunnerCli.Bot.Name, "You" },
            new Settings { MinifiedCardStrings = true, IncludeSuitInCardStrings = false }
        );
        CliGameIoHelper.UpdateMessage(Game.State, "Game started!");

        // Main game loop
        while (Game.TryGetWinner().Failure)
        {
            HandleUserInput(ReadlineWithBot());
        }

        // End game
        Reader.StopReading();
        CliGameIoHelper.GameOver(Game);
        if (CliGameIoHelper.Replay())
        {
            PlayGame(true);
        }
    }

    private string? ReadlineWithBot()
    {
        try
        {
            return Reader.ReadLine(
                random.Next(
                    (int)BotRunnerCli.Bot.QuickestResponseTimeMs,
                    (int)BotRunnerCli.Bot.SlowestResponseTimeMs
                )
            );
        }
        catch (TimeoutException)
        {
            // Let the bot move
            var botMoveResult = BotRunner.MakeMove(Game, 0);

            // Display the bots move
            if (botMoveResult.Success)
            {
                CliGameIoHelper.UpdateMessage(Game.State, Game.State.LastMove);
            }

            return null;
        }
    }

    private void HandleUserInput(string? input)
    {
        switch (input?.Trim())
        {
            case null:
                return;
            case "k":
                // Try and pickup from kitty
                var pickupKittyResult = Game.TryPickupFromKitty(1);
                if (pickupKittyResult is IErrorResult pickupKittyResultError)
                {
                    CliGameIoHelper.UpdateMessage(Game.State, pickupKittyResultError.Message);
                    return;
                }

                CliGameIoHelper.UpdateMessage(Game.State, Game.State.LastMove);
                break;
            case "t":
                var requestTopUpResult = Game.TryRequestTopUp(1);
                if (requestTopUpResult is IErrorResult requestTopUpResultError)
                {
                    CliGameIoHelper.UpdateMessage(Game.State, requestTopUpResultError.Message);
                    return;
                }

                CliGameIoHelper.UpdateMessage(Game.State, Game.State.LastMove);
                break;
            default:
                PlayCard(input);
                break;
        }
    }

    private void PlayCard(string input)
    {
        var splitInput = input.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var inputCardValue = splitInput.Length > 0 ? splitInput[0].ExtractInt() : null;
        if (inputCardValue == null)
        {
            CliGameIoHelper.UpdateMessage(
                Game.State,
                "Enter a card followed by the card to play it on e.g '6 5'. To pickup from the kitty enter 'k' "
            );
            return;
        }

        // Find card in hand
        var card = Game.GetCardWithValue(Game.State.Players[1].HandCards, inputCardValue);
        if (card == null)
        {
            CliGameIoHelper.UpdateMessage(Game.State, $"No card with value: {input} found");
            return;
        }

        // The second param should be the center card
        var centerCard = splitInput.Length > 1 ? splitInput[1].ExtractInt() : null;
        if (centerCard == null)
        {
            CliGameIoHelper.UpdateMessage(
                Game.State,
                "Invalid center card, specify it after the card like: '7, 6'"
            );
            return;
        }

        // See if the center card is valid
        var centerPileResult = Game.CardWithValueIsInCenterPile(Game.State, (int)centerCard);
        if (centerPileResult is IErrorResult centerPileResultError)
        {
            CliGameIoHelper.UpdateMessage(Game.State, centerPileResultError.Message);
            return;
        }

        // Try to play the card onto the pile
        var playCardResult = Game.TryPlayCard(1, card.Id, centerPileResult.Data);
        if (playCardResult is IErrorResult playCardResultError)
        {
            CliGameIoHelper.UpdateMessage(Game.State, playCardResultError.Message);
            return;
        }

        CliGameIoHelper.UpdateMessage(Game.State, Game.State.LastMove);
    }
}
