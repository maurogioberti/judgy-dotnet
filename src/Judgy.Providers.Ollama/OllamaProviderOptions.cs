using Judgy.Providers;

namespace Judgy.Providers.Ollama;

public class OllamaProviderOptions : LlmProviderOptions
{
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string Model { get; set; } = "llama3:8b";
    public string? ApiKey { get; set; }
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(120);
    public HttpMessageHandler? HttpMessageHandler { get; set; }
}
