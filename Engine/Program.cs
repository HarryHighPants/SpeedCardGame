// See https://aka.ms/new-console-template for more information

using Engine.CliHelpers;
using Engine.Helpers;

namespace Engine;

public static class Program
{
    public enum BotDifficulty
    {
        Easy,
        Medium,
        Hard,
        Impossible
    }

    public static Random random = new();

    public static Dictionary<BotDifficulty, BotData> Bots = new()
    {
        {
            BotDifficulty.Easy,
            new BotData
            {
                Name = "Limping Liam", CustomIntroMessage = "He can't jump far", CustomLoseMessage = "Oh no",
                CustomWinMessage = "Easy", QuickestResponseTimeMs = 3000,
                SlowestResponseTimeMs = 5000
            }
        },
        {
            BotDifficulty.Medium,
            new BotData
            {
                Name = "Harrowing Hayden", CustomIntroMessage = "He's a bit of a trickster so watch out",
                CustomLoseMessage = "Damn, he's tricky", CustomWinMessage = "Down goes the trickster",
                QuickestResponseTimeMs = 2000, SlowestResponseTimeMs = 4000
            }
        },
        {
            BotDifficulty.Hard,
            new BotData
            {
                Name = "Masterful Mikaela", CustomIntroMessage = "She can't be trusted",
                CustomLoseMessage = "Oof, rough one", CustomWinMessage = "Down falls Mikaela and her wicked ways",
                QuickestResponseTimeMs = 1000,
                SlowestResponseTimeMs = 2000
            }
        },
        {
            BotDifficulty.Impossible,
            new BotData
            {
                Name = "Chaotic Kate", CustomIntroMessage = "rip lol", CustomLoseMessage = "No chance",
                CustomWinMessage = "No one will ever see this message so it doesn't matter",
                QuickestResponseTimeMs = 500,
                SlowestResponseTimeMs = 1500
            }
        }
    };

    public static BotData Bot;

    public static GameState gameState { get; set; }

    public static void Main(string[] args)
    {
        PlayGame();

        void PlayGame(bool skipIntro = false)
        {
            // Difficulty
            BotDifficulty botDifficulty = CliGameUtils.GameIntro(skipIntro);
            Bot = Bots[botDifficulty];
            (int min, int max) botSpeedMs = (Bot.QuickestResponseTimeMs, Bot.SlowestResponseTimeMs);

            gameState = GameEngine.NewGame(new List<string> {Bot.Name, "You"});
            UpdateMessage("Game started!");

            // Main game loop
            while (GameEngine.TryGetWinner(gameState).Failure)
            {
                HandleUserInput(ReadlineWithBot());
            }

            // End game
            Player winner = GameEngine.TryGetWinner(gameState).Data;
            bool winnerIsPlayer = gameState.Players.IndexOf(winner) == 1;

            Console.WriteLine("------ Game over ----");
            Console.WriteLine();
            Console.WriteLine(winnerIsPlayer ? Bot.CustomWinMessage : Bot.CustomLoseMessage);
            Console.WriteLine();
            Console.WriteLine();
            Console.ForegroundColor = winnerIsPlayer ? ConsoleColor.DarkBlue : ConsoleColor.DarkRed;
            UpdateMessage($"Winner is {winner.Name}");
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine();
            Console.WriteLine("Replay? (y/n)");
            if (Console.ReadLine() == "y")
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
                    Result<(GameState updatedGameState, bool couldTopUp)> requestTopUpResult =
                        GameEngine.TryRequestTopUp(gameState, gameState.Players[1]);
                    if (requestTopUpResult is IErrorResult requestTopUpResultError)
                    {
                        UpdateMessage(requestTopUpResultError.Message);
                        return;
                    }

                    gameState = requestTopUpResult.Data.updatedGameState;
                    UpdateMessage(requestTopUpResult.Data.couldTopUp
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
                    "Enter a card followed by the card to play it on e.g '6 5'. To pickup from the kitty enter 'k' ");
                return;
            }

            // Find card in hand
            Card? card = CliGameUtils.GetCardWithValue(gameState.Players[1].HandCards, inputCardValue);
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
            Result<int> centerPileResult = GameEngine.CardWithValueIsInCenterPile(gameState, (int) centerCard);
            if (centerPileResult is IErrorResult centerPileResultError)
            {
                UpdateMessage(centerPileResultError.Message);
                return;
            }

            // Try to play the card onto the pile
            Result<GameState> playCardResult =
                GameEngine.TryPlayCard(gameState, gameState.Players[1], card, centerPileResult.Data);
            if (playCardResult is IErrorResult playCardResultError)
            {
                UpdateMessage(playCardResultError.Message);
                return;
            }

            // Update the state with the move
            gameState = playCardResult.Data;
            UpdateMessage($"Played card {CliGameUtils.CardToString(card)} on center pile {centerPileResult.Data + 1}");
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
                return Reader.ReadLine(random.Next(Bot.QuickestResponseTimeMs, Bot.SlowestResponseTimeMs));
            }
            catch (TimeoutException)
            {
                // BotRunner tries to play
                Result<(GameState updatedGameState, string moveMade)> botMoveResult =
                    BotRunner.MakeMove(gameState, gameState.Players[0]);
                if (botMoveResult is IErrorResult botMoveResultError)
                {
                    UpdateMessage($"{botMoveResultError.Message}");
                    return null;
                }

                gameState = botMoveResult.Data.updatedGameState;
                if (!string.IsNullOrEmpty(botMoveResult.Data.moveMade))
                {
                    UpdateMessage($"{gameState.Players[0].Name} {botMoveResult.Data.moveMade}");
                }

                return null;
            }
        }
    }

    public struct BotData
    {
        public string Name;
        public string? CustomIntroMessage;
        public string? CustomWinMessage;
        public string? CustomLoseMessage;
        public int QuickestResponseTimeMs;
        public int SlowestResponseTimeMs;
    }
}