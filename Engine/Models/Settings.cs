namespace Engine.Models;

public record Settings
{
    public int MaxHandCards { get; init; } = 5;
    public int? RandomSeed { get; init; } = null;
}
