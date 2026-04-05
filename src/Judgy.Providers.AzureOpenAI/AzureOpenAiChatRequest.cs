using System.Text.Json.Serialization;

namespace Judgy.Providers.AzureOpenAI;

internal sealed record AzureOpenAiChatRequest(
    [property: JsonPropertyName("messages")] AzureOpenAiChatMessage[] Messages,
    [property: JsonPropertyName("temperature")] double Temperature,
    [property: JsonPropertyName("max_tokens")] int MaxTokens);

internal sealed record AzureOpenAiChatMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content);
