using System.Text.Json.Serialization;

namespace Judgy.Providers.Moonshot;

internal sealed record MoonshotChatResponse(
    [property: JsonPropertyName("choices")] MoonshotChoice[]? Choices,
    [property: JsonPropertyName("usage")] MoonshotUsage? Usage);

internal sealed record MoonshotChoice(
    [property: JsonPropertyName("message")] MoonshotMessage? Message);

internal sealed record MoonshotMessage(
    [property: JsonPropertyName("content")] string? Content);

internal sealed record MoonshotUsage(
    [property: JsonPropertyName("prompt_tokens")] int? PromptTokens,
    [property: JsonPropertyName("completion_tokens")] int? CompletionTokens);
