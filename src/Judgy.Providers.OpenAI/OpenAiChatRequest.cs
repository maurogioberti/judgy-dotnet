using System.Text.Json.Serialization;

namespace Judgy.Providers.OpenAI;

internal sealed record OpenAiChatRequest(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("messages")] OpenAiChatMessage[] Messages,
    [property: JsonPropertyName("temperature")] double Temperature,
    [property: JsonPropertyName("max_tokens")] int MaxTokens);

internal sealed record OpenAiChatMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content);
