namespace Engine;

using Helpers;
using Models;

public record BotData
{
    public string Name { get; init; }
    public string? CustomIntroMessage { get; init; }
    public string? CustomWinMessage { get; init; }
    public string? CustomLoseMessage { get; init; }
    public double QuickestResponseTimeMs { get; init; }
    public double SlowestResponseTimeMs { get; init; }
    public double PickupIntervalMs { get; init; }
}

public static class BotRunner
{
    public static Result<Move> GetMove(
        EngineChecks engineChecks,
        GameState gameState,
        int playerId,
        List<bool> centerPilesAvailable
    ) =>
        engineChecks
            .TryGetWinner(gameState)
            .Map<Result<Move>>(
                winningPlayer =>
                    Result.Error<Move>($"can't move when {winningPlayer} has won already!"),
                _ =>
                    engineChecks
                        .PlayerHasPlay(gameState, playerId, centerPilesAvailable)
                        .Map(
                            play =>
                                Result.Successful(
                                    new Move(
                                        MoveType.PlayCard,
                                        playerId,
                                        play.card.Id,
                                        play.centerPile
                                    )
                                ),
                            _ =>
                                engineChecks
                                    .CanPickupFromKitty(gameState, playerId)
                                    .Map<Result<Move>>(
                                        () =>
                                            Result.Successful(
                                                new Move(MoveType.PickupCard, playerId)
                                            ),
                                        _ =>
                                            engineChecks
                                                .CanPlayerRequestTopUp(gameState, playerId)
                                                .Map<Result<Move>>(
                                                    () =>
                                                        Result.Successful(
                                                            new Move(
                                                                MoveType.RequestTopUp,
                                                                playerId
                                                            )
                                                        ),
                                                    _ =>
                                                        Result.Error<Move>(
                                                            "No moves for bot to make"
                                                        )
                                                )
                                    )
                        )
            );

    public static Result MakeMove(Game game, int playerId) =>
        game.TryGetWinner()
            .Map(
                winningPlayer =>
                    Result.Error($"can't move when {winningPlayer.Name} has won already!"),
                _ =>
                    game.gameEngine.Checks
                        .PlayerHasPlay(game.State, playerId)
                        .Map(
                            play => game.TryPlayCard(playerId, play.card.Id, play.centerPile),
                            _ =>
                                game.TryPickupFromKitty(playerId)
                                    .MapError(
                                        _ =>
                                            game.TryRequestTopUp(playerId)
                                                .MapError(
                                                    _ => Result.Error("No moves for bot to make")
                                                )
                                    )
                        )
            );
}
