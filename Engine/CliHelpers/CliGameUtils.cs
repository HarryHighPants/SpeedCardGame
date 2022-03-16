namespace Engine.CliHelpers;

public static class CliGameUtils
{
    public static void DrawGameState(GameState gameState)
    {
        Console.Clear();

        // Display the bots cards
        Player player = gameState.Players[0];
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine(
            $"{player.Name}:       {CardsToString(player.HandCards)}     Kitty count: {player.KittyCards.Count}");

        // Display the middle two cards
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Black;
        Console.WriteLine(
            $"                             {CardToString(gameState.CenterPiles[0].Last())} {CardToString(gameState.CenterPiles[1].Last())}      ");
        Console.ResetColor();
        Console.WriteLine();

        // Display the players cards
        player = gameState.Players[1];
        Console.ForegroundColor = ConsoleColor.DarkBlue;
        Console.WriteLine(
            $"{player.Name}:                   {CardsToString(player.HandCards)}     Kitty count: {player.KittyCards.Count}");
        Console.ResetColor();
        Console.WriteLine();
    }

    public static Card? GetCardWithValue(List<Card> cards, int? value)
    {
        return cards.FirstOrDefault(card => card.Value == value);
    }

    public static string CardsToString(List<Card> cards)
    {
        var line = "";
        for (var index = 0; index < cards.Count; index++)
        {
            Card card = cards[index];
            line += CardToString(card);
            if (index < cards.Count) line += "  ";
        }

        return line;
    }

    public static string CardToString(Card card)
    {
        return $"{card.Value}{card.Suit.ToString().ToLower()[0]}";
    }

    public static void GameIntro()
    {
        var instructionsTitle = "------ Instructions -----";
        Console.Clear();
        Console.WriteLine(instructionsTitle);
        Console.WriteLine("Welcome to the speed card game cli version");
        Console.WriteLine();
        Console.WriteLine("Press any key to continue..");
        Console.ReadKey(true);
        Console.Clear();

        Console.WriteLine(instructionsTitle);
        Console.WriteLine("To play a card in your hand, enter its value followed by the center pile to play it on..");
        Console.WriteLine();
        Console.WriteLine("Press any key to continue..");
        Console.ReadKey(true);
        Console.Clear();

        Console.WriteLine(instructionsTitle);
        Console.WriteLine("For example to play a 7 onto the 2nd center pile use '7 2'");
        Console.WriteLine("You can also pickup from your kitty by typing 'k'");
        Console.WriteLine("If you can't make any moves request a top up of the center pile with 't'");
        Console.WriteLine();
        Console.WriteLine("Press any key to continue..");
        Console.ReadKey(true);
        Console.Clear();

        Console.WriteLine("------ Game initialised ------");
        Console.WriteLine("Your opponent is: Botty the quick");
        Console.WriteLine();
        Console.WriteLine("Press any key to start the match!");
        Console.ReadKey(true);
    }
}