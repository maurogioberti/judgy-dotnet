using Judgy.Providers;

namespace Judgy.Providers.Anthropic;

public class AnthropicProviderOptions : LlmProviderOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "claude-sonnet-4-20250514";
    public string ApiVersion { get; set; } = "2023-06-01";
    public float Temperature { get; set; } = 0.0f;
    public int MaxTokens { get; set; } = 1024;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);
    public HttpMessageHandler? HttpMessageHandler { get; set; }
}
