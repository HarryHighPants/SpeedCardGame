// See https://aka.ms/new-console-template for more information

using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Engine;

class Program
{
    public static void Main(string[] args)
    {
        const int botSpeedMs = 5000;
        var gameState = Engine.NewGame(new List<string>{"Botty the quick", "You"}, null);
        
        Console.WriteLine("Game Created --------");
        Console.WriteLine($"Your opponent is: Botty the quick");
        
        Console.WriteLine($"Ready to start");
        Console.ReadLine();

        var loops = 0;
        while (Engine.CalculateWinner(gameState) == null)
        {
            // Display the game state
            DrawGameState();
            
            // Ask what card the user would like to play before bot plays
            Console.WriteLine($"{loops} Play card or take from (k)itty?");
            HandleUserInput(ReadlineWithBot());
            loops++;
        }

        string? ReadlineWithBot()
        {
            try {
                return Reader.ReadLine(botSpeedMs);
            } catch (TimeoutException) {
                // Bot tries to play
                MessageAndWait("Bot moved", 2000);
                // Ask Bot what it would do with gamestate
                // Engine.AttemptPlay(card)
                return null;
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
                    // todo
                    break;
                default: 
                    SelectCard(input);
                    break;
            }
        }

        void SelectCard(string input)
        {
            var inputValue = input.ExtractInt();
            if (inputValue == null) return;
            
            // Find card in hand
            var card = GetCardWithValue(gameState.Players[1].HandCards, inputValue);
            if (card == null)
            {
                MessageAndWait($"No card with value: {input} found");
                return;
            }
                
            // Which center pile?
            Console.WriteLine($"On which pile, 1 or 2?");
            var pile = ReadlineWithBot()?.ExtractInt();
            if (pile == null || ((int)pile != 1 && (int)pile != 2))
            {
                MessageAndWait($"Invalid pile {pile}");
                return;
            }
            
            // Try to play the card onto the pile
            var result = Engine.AttemptPlay(gameState, card, (int)pile-1);
            if (result.errorMessage != null)
            {
                MessageAndWait(result.errorMessage);
                return;
            }

            // Update the state with the move
            gameState = result.updatedGameState;
            MessageAndWait($"Moved card {CardToString(card)} to pile {pile}");
        }

        void MessageAndWait(string message, int clearTimeMs = 2000)
        {
            Console.WriteLine(message);
            Console.WriteLine("---");
            Thread.Sleep(clearTimeMs);
        }

        Card? GetCardWithValue(List<Card> cards, int? value)
        {
            return cards.FirstOrDefault(card => card.Value == value);
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
            Console.WriteLine($"                             {CardToString(gameState.CenterPiles[0].Last())} {CardToString(gameState.CenterPiles[1].Last())}      ");
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
}
