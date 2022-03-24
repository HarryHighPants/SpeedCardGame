namespace Engine.Models;

public record Settings
{
    public int MaxHandCards { get; init; } = 5;
    public int? RandomSeed { get; init; } = null;

    public bool MinifiedCardStrings { get; init; } = false;
    public bool IncludeSuitInCardStrings { get; init; } = false;
}
