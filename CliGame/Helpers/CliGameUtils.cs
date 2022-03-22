using Engine;

namespace CliGame.Helpers;
public static class CliGameUtils
{
    public static void DrawGameState(GameState gameState)
    {
        Console.Clear();

        // Display the bots cards
        Player player = gameState.Players[0];
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine(
            $"{player.Name}:       {GameEngine.CardsToString(player.HandCards)}     Kitty count: {player.KittyCards.Count}");

        // Display the middle two cards
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Black;
        Console.WriteLine(
            $"                             {GameEngine.CardToString(gameState.CenterPiles[0].Last())} {GameEngine.CardToString(gameState.CenterPiles[1].Last())}      ");
        Console.ResetColor();
        Console.WriteLine();

        // Display the players cards
        player = gameState.Players[1];
        Console.ForegroundColor = ConsoleColor.DarkBlue;
        Console.WriteLine(
            $"{player.Name}:                   {GameEngine.CardsToString(player.HandCards)}     Kitty count: {player.KittyCards.Count}");
        Console.ResetColor();
        Console.WriteLine();
    }

    public static Card? GetCardWithValue(IEnumerable<Card> cards, int? value)
    {
        return cards.FirstOrDefault(card => card.Value == value);
    }

    public static BotDifficulty GameIntro(bool skipIntro = false)
    {
        var hearRulesInput = "";
        if (!skipIntro)
        {
            Console.Clear();
            Console.WriteLine("------ Speed Card Game ------");
            Console.WriteLine("Welcome to the speed card game cli version");
            Console.WriteLine();
            Console.WriteLine("Would you like to hear the rules? (y/n)");
            hearRulesInput = Console.ReadLine();
            Console.Clear();
        }

        if (hearRulesInput == "y")
        {
            var instructionsTitle = "------ Instructions -----";
            Console.WriteLine(instructionsTitle);
            Console.WriteLine("The goal of this game is to get rid of all your cards before your opponent.");
            Console.WriteLine(
                "To get rid of cards you can play them onto the center piles if it's 1 above or below the other card.");
            Console.WriteLine("Also, an ace (12) can be played onto a 2(0) or vice versa.");
            Console.WriteLine("You can pickup cards from your kitty if you have less than 5 cards in your hand.");
            Console.WriteLine();
            Console.WriteLine("Press any key to continue..");
            Console.ReadKey(true);
            Console.Clear();

            Console.WriteLine(instructionsTitle);
            Console.WriteLine("To play a card in your hand, enter its value followed by the card to play it on.");
            Console.WriteLine("For example to play a 7 onto a 6 in the center pile use '7 6'");
            Console.WriteLine();
            Console.WriteLine("Press any key to continue..");
            Console.ReadKey(true);
            Console.Clear();

            Console.WriteLine(instructionsTitle);
            Console.WriteLine("You can also pickup from your kitty by typing 'k'");
            Console.WriteLine("If you can't make any moves, request a top up of the center pile with 't'");
            Console.WriteLine();
            Console.WriteLine("Press any key to continue..");
            Console.ReadKey(true);
            Console.Clear();
        }

        Console.Clear();
        Console.WriteLine("------ Difficulty ------");
        Console.WriteLine("What difficulty opponent will you face?");
        Console.WriteLine("(e)asy, (m)edium, (h)ard, (i)mpossible");
        string difficultyInput = Console.ReadLine();
        Console.Clear();

        CliGame.BotDifficulty difficulty = difficultyInput switch
        {
            "e" => CliGame.BotDifficulty.Easy,
            "m" => CliGame.BotDifficulty.Medium,
            "h" => CliGame.BotDifficulty.Hard,
            "i" => CliGame.BotDifficulty.Impossible,
            _ => CliGame.BotDifficulty.Medium
        };
        // Bot Setup
        BotRunnerCli.SetDifficulty(difficulty);

        Console.WriteLine("------ Game initialised ------");
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine($"Your opponent is: {BotRunnerCli.Bot.Name}");
        Console.ForegroundColor = ConsoleColor.Black;
        Console.WriteLine(BotRunnerCli.Bot.CustomIntroMessage);
        Console.WriteLine();
        Console.WriteLine("Press any key to start the match!");
        Console.ReadKey(true);
        return difficulty;
    }
}