using Judgy.Providers;

namespace Judgy.Providers.Google;

public class GoogleProviderOptions : LlmProviderOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-2.5-flash";
    public float Temperature { get; set; } = 0.0f;
    public int MaxOutputTokens { get; set; } = 1024;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);
    public HttpMessageHandler? HttpMessageHandler { get; set; }
}
