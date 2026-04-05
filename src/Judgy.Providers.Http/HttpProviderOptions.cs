using Judgy.Providers;

namespace Judgy.Providers.Http;

public class HttpProviderOptions : LlmProviderOptions
{
    public string Endpoint { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
    public string RequestTemplate { get; set; } = """{"prompt": "{{prompt}}"}""";
    public string ResponseJsonPath { get; set; } = "$.response";
    public string? RegexPattern { get; set; }
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);
    public HttpMessageHandler? HttpMessageHandler { get; set; }
}
