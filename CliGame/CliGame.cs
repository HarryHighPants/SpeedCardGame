using CliGame.Helpers;
using Engine;
using Engine.Helpers;

namespace CliGame;
public class CliGame
{
    public readonly Random Random = new();
    public GameState GameState { get; set; } = new();
    public readonly CliGameUtils CliGameUtils = new();
    
    public void PlayGame(bool skipIntro = false)
        {
            // Intro
            CliGameUtils.GameIntro(skipIntro);

            GameState = GameEngine.NewGame(new List<string> {BotRunnerCli.Bot.Name, "You"});
            CliGameUtils.UpdateMessage(GameState,"Game started!");

            // Main game loop
            while (GameEngine.TryGetWinner(GameState).Failure)
            {
                HandleUserInput(ReadlineWithBot());
            }
            
            // End game
            Reader.StopReading();
            CliGameUtils.GameOver(GameState);
            if (CliGameUtils.Replay())
            {
                PlayGame(true);
            }
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
            if (botMoveResult.Success) CliGameUtils.UpdateMessage(GameState, GameEngine.ReadableLastMove(GameState, true));

            return null;
        }
    }


        void HandleUserInput(string? input)
        {
            switch (input?.Trim())
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
                        CliGameUtils.UpdateMessage(GameState, pickupKittyResultError.Message);
                        return;
                    }

                    GameState = pickupKittyResult.Data.updatedGameState;
                    CliGameUtils.UpdateMessage(GameState,GameEngine.ReadableLastMove(GameState, true));
                    break;
                case "t":
                    var requestTopUpResult =
                        GameEngine.TryRequestTopUp(GameState, 1);
                    if (requestTopUpResult is IErrorResult requestTopUpResultError)
                    {
                        CliGameUtils.UpdateMessage(GameState,requestTopUpResultError.Message);
                        return;
                    }

                    GameState = requestTopUpResult.Data;
                    CliGameUtils.UpdateMessage(GameState,GameEngine.ReadableLastMove(GameState, true));
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
                CliGameUtils.UpdateMessage(GameState,
                    "Enter a card followed by the card to play it on e.g '6 5'. To pickup from the kitty enter 'k' ");
                return;
            }

            // Find card in hand
            Card? card = GetCardWithValue(GameState.Players[1].HandCards, inputCardValue);
            if (card == null)
            {
                CliGameUtils.UpdateMessage(GameState,$"No card with value: {input} found");
                return;
            }

            // Teh second param should be the center card
            int? centerCard = splitInput.Length > 1 ? splitInput[1].ExtractInt() : null;
            if (centerCard == null)
            {
                CliGameUtils.UpdateMessage(GameState,"Invalid center card, specify it after the card like: '7, 6'");
                return;
            }

            // See if the center card is valid
            Result<int> centerPileResult = GameEngine.CardWithValueIsInCenterPile(GameState, (int) centerCard);
            if (centerPileResult is IErrorResult centerPileResultError)
            {
                CliGameUtils.UpdateMessage(GameState,centerPileResultError.Message);
                return;
            }

            // Try to play the card onto the pile
            Result<GameState> playCardResult = GameEngine.TryPlayCard(GameState, 1, card, centerPileResult.Data);
            if (playCardResult is IErrorResult playCardResultError)
            {
                CliGameUtils.UpdateMessage(GameState,playCardResultError.Message);
                return;
            }

            // Update the state with the move
            GameState = playCardResult.Data;
            CliGameUtils.UpdateMessage(GameState,GameEngine.ReadableLastMove(GameState, true));
        }
        
        public static Card? GetCardWithValue(IEnumerable<Card> cards, int? value)
        {
            return cards.FirstOrDefault(card => card.CardValue == (CardValue)value!);
        }
}