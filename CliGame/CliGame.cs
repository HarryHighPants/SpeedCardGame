namespace CliGame.Helpers;

using Engine;
using Engine.Helpers;
using Engine.Models;

public class CliGame : Game
{
    public CliGame(
        List<string>? playerNames = null,
        Settings? settings = null,
        GameEngine? gameEngine = null
    ) : base(playerNames, settings, gameEngine) { }

    public Card? GetCardWithValue(IEnumerable<Card> cards, int? value) =>
        cards.FirstOrDefault(card => card.CardValue == (CardValue)value!);

    public Result<int> CardWithValueIsInCenterPile(GameState gameState, int value)
    {
        for (var i = 0; i < gameState.CenterPiles.Count; i++)
        {
            if (gameState.CenterPiles[i].Cards.Count < 1)
            {
                continue;
            }

            var pileCard = gameState.CenterPiles[i].Cards.Last();
            if ((int)pileCard.CardValue == value)
            {
                return Result.Successful(i);
            }
        }

        return Result.Error<int>("Card with value not found in center pile");
    }
}
