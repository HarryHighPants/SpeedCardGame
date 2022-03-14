// See https://aka.ms/new-console-template for more information

using System.Text.RegularExpressions;

namespace Engine;

class Program
{
    public static void Main(string[] args)
    {
        const int botSpeedMs = 3000;
        var gameState = Engine.NewGame(new List<string>{"Botty the quick", "You"}, null);
        
        Console.WriteLine("Game Created --------");
        Console.WriteLine($"Your opponent is: Botty the quick");
        
        Console.WriteLine($"Ready to start");
        Console.ReadKey();

        while (Engine.CalculateWinner(gameState) == null)
        {
            // Display the game state
            DrawGameState();
            
            // Ask what card the user would like to play before bot plays
            Console.WriteLine($"Play card or take from (k)itty");
            try {
                var input = Reader.ReadLine(botSpeedMs);
                HandleUserInput(input);
            } catch (TimeoutException) {
                // Bot tries to play
                Console.WriteLine("Bot moved");
                
            }
        }
        
        void HandleUserInput(string input)
        {
            var parsedInput = Regex.Replace(
                input, // Our input
                "[^0-9]", // Select everything that is not in the range of 0-9
                "");        // Replace that with an empty string.
            
            if (input == "k")
            {
                // Try and pickup from kitty
            }
            
            if (int.TryParse(parsedInput, out int result))
            {
                // Try to play card
            }
        }
        
        void DrawGameState()
        {
            Console.Clear();
            
            // Display the bots cards
            var player = gameState.Players[0];
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"{player.Name}:       {CardsToString(player.HandCards)}     Kitty count: {player.KittyCards.Count}");
            
            // Display the middle two cards
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine($"                             {CardsToString(gameState.CenterCards)}      ");
            Console.ResetColor();
            Console.WriteLine();
            
            // Display the players cards
            player = gameState.Players[1];
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine($"{player.Name}:                   {CardsToString(player.HandCards)}     Kitty count: {player.KittyCards.Count}");
            Console.ResetColor();
            Console.WriteLine();

        }

        string CardsToString(List<Card> cards)
        {
            string line = "";
            for (var index = 0; index < cards.Count; index++)
            {
                var card = cards[index];
                line += CardToString(card);
                if (index < cards.Count)
                {
                    line += "  ";
                }
            }

            return line;
        }

        string CardToString(Card card)
        {
            return $"{card.Value}{card.Suit.ToString().ToLower()[0]}";
        }
    }




    // static void getInput() {
    //
    //     while (!Console.KeyAvailable) {
    //         Move();
    //         updateScreen();
    //     }
    //     ConsoleKeyInfo input;
    //     input = Console.ReadKey();
    //     doInput(input.KeyChar);
    // }
    //
    // static void checkCell(Cell cell) {
    //     if (cell.val == "%") {
    //         eatFood();
    //     }
    //     if (cell.visited) {
    //         Lose();
    //     }
    // }
}
