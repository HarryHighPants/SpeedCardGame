// See https://aka.ms/new-console-template for more information
using CliGame.Helpers;
using Engine.Helpers;
using Engine;

namespace CliGame;
static class Program
{
    public static readonly Random Random = new();
    public static GameState GameState { get; set; } = new();

    private static void Main(string[] args)
    {
        PlayGame();

        void PlayGame(bool skipIntro = false)
        {
            // Intro
            CliGameUtils.GameIntro(skipIntro);

            GameState = GameEngine.NewGame(new List<string> {BotRunnerCli.Bot.Name, "You"});
            UpdateMessage("Game started!");

            // Main game loop
            while (GameEngine.TryGetWinner(GameState).Failure)
            {
                var userInput = ReadlineWithBot();
                // if(GameEngine.TryGetWinner(GameState).Success) continue;
                HandleUserInput(userInput);
            }
            

            // End game
            var winner = GameState.Players[GameEngine.TryGetWinner(GameState).Data];
            bool winnerIsPlayer = GameState.Players.IndexOf(winner) == 1;

            // CliGameUtils.DrawGameState(GameState);
            Console.WriteLine("------ Game over ----");
            Console.WriteLine();
            Console.WriteLine(winnerIsPlayer ? BotRunnerCli.Bot.CustomWinMessage : BotRunnerCli.Bot.CustomLoseMessage);
            Console.WriteLine();
            Console.WriteLine();
            Console.ForegroundColor = winnerIsPlayer ? ConsoleColor.DarkBlue : ConsoleColor.DarkRed;
            UpdateMessage($"Winner is {winner.Name}");
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine();
            Console.WriteLine("Replay? (y/n)");
            if (Console.ReadLine()?.Trim() == "y")
            {
                PlayGame(true);
            }
        }


        void HandleUserInput(string? input)
        {
            switch (input)
            {
                case null:
                    return;
                case "k":
                    // Try and pickup from kitty
                    Result<(GameState updatedGameState, Card pickedUpCard)> pickupKittyResult
                        = GameEngine.TryPickupFromKitty(GameState, 1);
                    if (pickupKittyResult is IErrorResult
                        pickupKittyResultError)
                    {
                        UpdateMessage(pickupKittyResultError.Message);
                        return;
                    }

                    GameState = pickupKittyResult.Data.updatedGameState;
                    UpdateMessage(GameEngine.ReadableLastMove(GameState, true));
                    break;
                case "t":
                    var requestTopUpResult =
                        GameEngine.TryRequestTopUp(GameState, 1);
                    if (requestTopUpResult is IErrorResult requestTopUpResultError)
                    {
                        UpdateMessage(requestTopUpResultError.Message);
                        return;
                    }

                    GameState = requestTopUpResult.Data;
                    UpdateMessage(GameEngine.ReadableLastMove(GameState, true));
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
                    "Enter a card followed by the card to play it on e.g '6 5'. To pickup from the kitty enter 'k' ");
                return;
            }

            // Find card in hand
            Card? card = CliGameUtils.GetCardWithValue(GameState.Players[1].HandCards, inputCardValue);
            if (card == null)
            {
                UpdateMessage($"No card with value: {input} found");
                return;
            }

            // Teh second param should be the center card
            int? centerCard = splitInput.Length > 1 ? splitInput[1].ExtractInt() : null;
            if (centerCard == null)
            {
                UpdateMessage("Invalid center card, specify it after the card like: '7, 6'");
                return;
            }

            // See if the center card is valid
            Result<int> centerPileResult = GameEngine.CardWithValueIsInCenterPile(GameState, (int) centerCard);
            if (centerPileResult is IErrorResult centerPileResultError)
            {
                UpdateMessage(centerPileResultError.Message);
                return;
            }

            // Try to play the card onto the pile
            Result<GameState> playCardResult = GameEngine.TryPlayCard(GameState, 1, card, centerPileResult.Data);
            if (playCardResult is IErrorResult playCardResultError)
            {
                UpdateMessage(playCardResultError.Message);
                return;
            }

            // Update the state with the move
            GameState = playCardResult.Data;
            UpdateMessage(GameEngine.ReadableLastMove(GameState, true));
        }

        void UpdateMessage(string message)
        {
            CliGameUtils.DrawGameState(GameState);
            Console.WriteLine(message);
        }

        string? ReadlineWithBot()
        {
            try
            {
                return Reader.ReadLine(Random.Next(BotRunnerCli.Bot.QuickestResponseTimeMs, BotRunnerCli.Bot.SlowestResponseTimeMs));
            }
            catch (TimeoutException)
            {
                // Let the bot move
                var botMoveResult = BotRunnerCli.MakeMove(GameState, 0);
                GameState = botMoveResult.Success ? botMoveResult.Data : GameState;
                
                // Display the bots move
                if (botMoveResult.Success)UpdateMessage(GameEngine.ReadableLastMove(GameState, true));

                return null;
            }
        }
    }
}