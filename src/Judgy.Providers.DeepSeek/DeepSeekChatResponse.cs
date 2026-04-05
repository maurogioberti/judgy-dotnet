using System.Text.Json.Serialization;

namespace Judgy.Providers.DeepSeek;

internal sealed record DeepSeekChatResponse(
    [property: JsonPropertyName("choices")] DeepSeekChoice[]? Choices,
    [property: JsonPropertyName("usage")] DeepSeekUsage? Usage);

internal sealed record DeepSeekChoice(
    [property: JsonPropertyName("message")] DeepSeekMessage? Message);

internal sealed record DeepSeekMessage(
    [property: JsonPropertyName("content")] string? Content);

internal sealed record DeepSeekUsage(
    [property: JsonPropertyName("prompt_tokens")] int? PromptTokens,
    [property: JsonPropertyName("completion_tokens")] int? CompletionTokens);
