namespace Engine;

using Models;

public class Game
{
    private GameState gameState;

    private Game(List<string>? playerNames = null, Settings? settings = null) => this.gameState = GameEngine.NewGame(playerNames, settings);
}
