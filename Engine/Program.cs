// See https://aka.ms/new-console-template for more information

using Engine.CliHelpers;
using Engine.Helpers;

namespace Engine;

public static class Program
{
    private static GameState _gameState;

    public static Random random = new();

    public static GameState gameState
    {
        get => _gameState;
        set
        {
            CliGameUtils.DrawGameState(value);
            _gameState = value;
        }
    }

    public static void Main(string[] args)
    {
        (int min, int max) botSpeedMs = (500, 5000);

        CliGameUtils.GameIntro();

        gameState = GameEngine.NewGame(new List<string> {"Botty the quick", "You"});

        while (GameEngine.TryGetWinner(gameState).Failure)
        {
            HandleUserInput(ReadlineWithBot());
        }

        Player winner = GameEngine.TryGetWinner(gameState).Data;
        Console.WriteLine("------ Game over ----");
        UpdateMessage($"Winner is {winner!.Name}");

        void HandleUserInput(string? input)
        {
            switch (input)
            {
                case null:
                    // UpdateMessage("");
                    return;
                case "k":
                    // Try and pickup from kitty
                    Result<(GameState updatedGameState, Card pickedUpCard)> pickupKittyResult
                        = GameEngine.TryPickupFromKitty(gameState, gameState.Players[1]);
                    if (pickupKittyResult is IErrorResult
                        pickupKittyResultError)
                    {
                        UpdateMessage(pickupKittyResultError.Message);
                        return;
                    }

                    gameState = pickupKittyResult.Data.updatedGameState;
                    UpdateMessage($"picked up a {CliGameUtils.CardToString(pickupKittyResult.Data.pickedUpCard)}");
                    break;
                case "t":
                    Result<(GameState updatedGameState, bool immediateTopUp, bool readyToTopUp)> requestTopUpResult =
                        GameEngine.TryRequestTopUp(gameState, gameState.Players[1]);
                    if (requestTopUpResult is IErrorResult requestTopUpResultError)
                    {
                        UpdateMessage(requestTopUpResultError.Message);
                        return;
                    }

                    gameState = requestTopUpResult.Data.updatedGameState;
                    UpdateMessage(requestTopUpResult.Data.immediateTopUp
                        ? "topped up the center piles"
                        : "requested to top up center piles");
                    break;
                default:
                    SelectCard(input);
                    break;
            }
        }

        void SelectCard(string input)
        {
            string[] splitInput = input.Split(new[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries);
            int? inputCardValue = splitInput.Length > 0 ? splitInput[0].ExtractInt() : null;
            if (inputCardValue == null)
            {
                UpdateMessage(
                    "Enter a card followed by the pile to play it on e.g '6 1'. To pickup from the kitty enter 'k' ");
                return;
            }

            // Find card in hand
            Card? card = CliGameUtils.GetCardWithValue(gameState.Players[1].HandCards, inputCardValue);
            if (card == null)
            {
                UpdateMessage($"No card with value: {input} found");
                return;
            }

            // Which center pile?
            // Try and get it as a second param
            int? pile = splitInput.Length > 1 ? splitInput[1].ExtractInt() : null;
            if (pile == null || (int) pile != 1 && (int) pile != 2)
            {
                UpdateMessage("Invalid pile, specify it after the card like: '7, 2'");
                return;
            }

            // Try to play the card onto the pile
            Result<GameState> playCardResult =
                GameEngine.TryPlayCard(gameState, gameState.Players[1], card, (int) pile - 1);
            if (playCardResult is IErrorResult playCardResultError)
            {
                UpdateMessage(playCardResultError.Message);
                return;
            }

            // Update the state with the move
            gameState = playCardResult.Data;
            UpdateMessage($"Moved card {CliGameUtils.CardToString(card)} to pile {pile}");
        }

        void UpdateMessage(string message)
        {
            CliGameUtils.DrawGameState(gameState);
            Console.WriteLine(message);
        }

        string? ReadlineWithBot()
        {
            try
            {
                return Reader.ReadLine(random.Next(botSpeedMs.min, botSpeedMs.max));
            }
            catch (TimeoutException)
            {
                // Bot tries to play
                Result<(GameState updatedGameState, string moveMade)> botMoveResult =
                    Bot.MakeMove(gameState, gameState.Players[0]);
                if (botMoveResult is IErrorResult botMoveResultError)
                {
                    UpdateMessage($"{botMoveResultError.Message}");
                    return null;
                }

                gameState = botMoveResult.Data.updatedGameState;
                UpdateMessage($"{gameState.Players[0].Name} {botMoveResult.Data.moveMade}");
                return null;
            }
        }
    }
}