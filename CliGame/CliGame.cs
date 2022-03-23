namespace CliGame;

using Engine;
using Engine.Helpers;
using Helpers;

public class CliGame
{
    private readonly Random random = new();
    private GameState GameState { get; set; } = new();

    public void PlayGame(bool skipIntro = false)
    {
        // Intro
        CliGameUtils.GameIntro(skipIntro);

        this.GameState = GameEngine.NewGame(new List<string> {BotRunnerCli.Bot.Name, "You"});
        CliGameUtils.UpdateMessage(this.GameState, "Game started!");

        // Main game loop
        while (GameEngine.TryGetWinner(this.GameState).Failure)
        {
            this.HandleUserInput(this.ReadlineWithBot());
        }

        // End game
        Reader.StopReading();
        CliGameUtils.GameOver(this.GameState);
        if (CliGameUtils.Replay())
        {
            this.PlayGame(true);
        }
    }

    private string? ReadlineWithBot()
    {
        try
        {
            return Reader.ReadLine(this.random.Next(BotRunnerCli.Bot.QuickestResponseTimeMs,
                BotRunnerCli.Bot.SlowestResponseTimeMs));
        }
        catch (TimeoutException)
        {
            // Let the bot move
            var botMoveResult = BotRunner.MakeMove(this.GameState, 0);
            this.GameState = botMoveResult.Success ? botMoveResult.Data : this.GameState;

            // Display the bots move
            if (botMoveResult.Success)
            {
                CliGameUtils.UpdateMessage(this.GameState, GameEngine.ReadableLastMove(this.GameState, true));
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
                var pickupKittyResult
                    = GameEngine.TryPickupFromKitty(this.GameState, 1);
                if (pickupKittyResult is IErrorResult
                    pickupKittyResultError)
                {
                    CliGameUtils.UpdateMessage(this.GameState, pickupKittyResultError.Message);
                    return;
                }

                this.GameState = pickupKittyResult.Data.updatedGameState;
                CliGameUtils.UpdateMessage(this.GameState, GameEngine.ReadableLastMove(this.GameState, true));
                break;
            case "t":
                var requestTopUpResult =
                    GameEngine.TryRequestTopUp(this.GameState, 1);
                if (requestTopUpResult is IErrorResult requestTopUpResultError)
                {
                    CliGameUtils.UpdateMessage(this.GameState, requestTopUpResultError.Message);
                    return;
                }

                this.GameState = requestTopUpResult.Data;
                CliGameUtils.UpdateMessage(this.GameState, GameEngine.ReadableLastMove(this.GameState, true));
                break;
            default:
                this.SelectCard(input);
                break;
        }
    }

    private void SelectCard(string input)
    {
        var splitInput = input.Split(new[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries);
        var inputCardValue = splitInput.Length > 0 ? splitInput[0].ExtractInt() : null;
        if (inputCardValue == null)
        {
            CliGameUtils.UpdateMessage(this.GameState,
                "Enter a card followed by the card to play it on e.g '6 5'. To pickup from the kitty enter 'k' ");
            return;
        }

        // Find card in hand
        var card = GetCardWithValue(this.GameState.Players[1].HandCards, inputCardValue);
        if (card == null)
        {
            CliGameUtils.UpdateMessage(this.GameState, $"No card with value: {input} found");
            return;
        }

        // Teh second param should be the center card
        var centerCard = splitInput.Length > 1 ? splitInput[1].ExtractInt() : null;
        if (centerCard == null)
        {
            CliGameUtils.UpdateMessage(this.GameState,
                "Invalid center card, specify it after the card like: '7, 6'");
            return;
        }

        // See if the center card is valid
        var centerPileResult = GameEngine.CardWithValueIsInCenterPile(this.GameState, (int)centerCard);
        if (centerPileResult is IErrorResult centerPileResultError)
        {
            CliGameUtils.UpdateMessage(this.GameState, centerPileResultError.Message);
            return;
        }

        // Try to play the card onto the pile
        var playCardResult = GameEngine.TryPlayCard(this.GameState, 1, card, centerPileResult.Data);
        if (playCardResult is IErrorResult playCardResultError)
        {
            CliGameUtils.UpdateMessage(this.GameState, playCardResultError.Message);
            return;
        }

        // Update the state with the move
        this.GameState = playCardResult.Data;
        CliGameUtils.UpdateMessage(this.GameState, GameEngine.ReadableLastMove(this.GameState, true));
    }

    public static Card? GetCardWithValue(IEnumerable<Card> cards, int? value) =>
        cards.FirstOrDefault(card => card.CardValue == (CardValue)value!);
}
