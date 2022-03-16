namespace Engine.Helpers;

public abstract class GameResult : Result, IGameState
{
    private GameState _data;

    protected GameResult(GameState initData)
    {
        data = initData;
    }

    private GameState data
    {
        get => Success
            ? _data
            : throw new Exception($"You can't access .{nameof(data)} when .{nameof(Success)} is false");
        set => _data = value;
    }

    public Settings? Settings
    {
        get => data.Settings;
        set => data.Settings = value;
    }

    public List<Player> Players
    {
        get => data.Players;
        set => data.Players = value;
    }

    public List<List<Card>> CenterPiles
    {
        get => data.CenterPiles;
        set => data.CenterPiles = value;
    }
}

public class SuccessGameResult : GameResult
{
    public SuccessGameResult(GameState data) : base(data)
    {
        Success = true;
    }
}

public class ErrorGameResult : GameResult, IErrorResult
{
    public ErrorGameResult(string message) : this(message, Array.Empty<Error>())
    {
    }

    public ErrorGameResult(string message, IReadOnlyCollection<Error> errors) : base(default)
    {
        Message = message;
        Success = false;
        Errors = errors ?? Array.Empty<Error>();
    }

    public string Message { get; set; }
    public IReadOnlyCollection<Error> Errors { get; }
}