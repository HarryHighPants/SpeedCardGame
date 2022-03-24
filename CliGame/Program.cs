// See https://aka.ms/new-console-template for more information

namespace CliGame;

internal static class Program
{
    private static void Main(string[] args)
    {
        var cliGame = new CliGameRunner();
        cliGame.PlayGame();
    }
}
