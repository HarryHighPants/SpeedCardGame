// See https://aka.ms/new-console-template for more information

using Engine.CliHelpers;

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

        gameState = GameEngine.NewGame(new List<string> {"Botty the quick", "You"}, null);

        while (GameEngine.CalculateWinner(gameState) == null)
            HandleUserInput(ReadlineWithBot());

        Player? winner = GameEngine.CalculateWinner(gameState);
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
                    (GameState? updatedGameState, Card? pickedUpCard, string? errorMessage) pickcupKitty
                        = GameEngine.TryPickupFromKitty(gameState, gameState.Players[1]);
                    if (pickcupKitty.errorMessage != null)
                    {
                        UpdateMessage(pickcupKitty.errorMessage);
                        return;
                    }

                    gameState = pickcupKitty.updatedGameState!;
                    UpdateMessage($"picked up a {CliGameUtils.CardToString(pickcupKitty.pickedUpCard!)}");
                    break;
                case "t":
                    (GameState? updatedGameState, bool immediateTopUp, bool readyToTopUp, string? errorMessage)
                        requestTopUp = GameEngine.TryRequestTopUp(gameState, gameState.Players[1]);
                    if (requestTopUp.errorMessage != null)
                    {
                        UpdateMessage(requestTopUp.errorMessage);
                        return;
                    }

                    gameState = requestTopUp.updatedGameState!;
                    UpdateMessage(requestTopUp.immediateTopUp
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
            (GameState? updatedGameState, string? errorMessage) result =
                GameEngine.TryPlayCard(gameState, card, (int) pile - 1);
            if (result.errorMessage != null)
            {
                UpdateMessage(result.errorMessage);
                return;
            }

            // Update the state with the move
            gameState = result.updatedGameState;
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
                (GameState? updatedGameState, string? moveMade, string? errorMessage) =
                    Bot.MakeMove(gameState, gameState.Players[0]);
                if (updatedGameState == null)
                {
                    UpdateMessage($"{errorMessage}");
                    return null;
                }

                gameState = updatedGameState;
                UpdateMessage($"{gameState.Players[0].Name} {moveMade}");
                return null;
            }
        }
    }
}