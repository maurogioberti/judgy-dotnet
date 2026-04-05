using System.Text.Json.Serialization;

namespace Judgy.Providers.Anthropic;

internal sealed record AnthropicMessageRequest(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("max_tokens")] int MaxTokens,
    [property: JsonPropertyName("messages")] AnthropicMessage[] Messages,
    [property: JsonPropertyName("temperature")] double Temperature,
    [property: JsonPropertyName("system")] string? System);

internal sealed record AnthropicMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content);
