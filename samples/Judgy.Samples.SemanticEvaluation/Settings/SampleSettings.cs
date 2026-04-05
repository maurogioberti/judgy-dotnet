namespace Judgy.Samples.SemanticEvaluation;

public sealed class SampleSettings
{
    public SourceApiSettings SourceApi { get; init; } = new();
    public JudgeOllamaSettings JudgeOllama { get; init; } = new();
}

public sealed class SourceApiSettings
{
    public string Endpoint { get; init; } = string.Empty;
    public string RequestTemplate { get; init; } = string.Empty;
    public string ResponseJsonPath { get; init; } = string.Empty;
    public int TimeoutSeconds { get; init; } = 60;
}

public sealed class JudgeOllamaSettings
{
    public string BaseUrl { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public int TimeoutSeconds { get; init; } = 120;
    public string? ApiKey { get; init; }
}
