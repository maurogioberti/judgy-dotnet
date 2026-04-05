using Judgy.Providers;

namespace Judgy.Providers.Moonshot;

public class MoonshotProviderOptions : LlmProviderOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "kimi-k2.5";
    public float Temperature { get; set; } = 0.0f;
    public int MaxTokens { get; set; } = 1024;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);
    public HttpMessageHandler? HttpMessageHandler { get; set; }
}
