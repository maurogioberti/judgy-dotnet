using Judgy.Providers;

namespace Judgy.Providers.DeepSeek;

public class DeepSeekProviderOptions : LlmProviderOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "deepseek-chat";
    public float Temperature { get; set; } = 0.0f;
    public int MaxTokens { get; set; } = 1024;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);
    public HttpMessageHandler? HttpMessageHandler { get; set; }
}
