namespace Judgy.Samples.XunitAssertions;

public sealed class SampleSettings
{
    public OllamaJudgeSettings JudgeOllama { get; init; } = new();
}

public sealed class OllamaJudgeSettings
{
    public string BaseUrl { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public int TimeoutSeconds { get; init; } = 120;
    public string? ApiKey { get; init; }
}
