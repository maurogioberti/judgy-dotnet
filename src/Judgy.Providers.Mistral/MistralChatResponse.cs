using System.Text.Json.Serialization;

namespace Judgy.Providers.Mistral;

internal sealed record MistralChatResponse(
    [property: JsonPropertyName("choices")] MistralChoice[]? Choices,
    [property: JsonPropertyName("usage")] MistralUsage? Usage);

internal sealed record MistralChoice(
    [property: JsonPropertyName("message")] MistralMessage? Message);

internal sealed record MistralMessage(
    [property: JsonPropertyName("content")] string? Content);

internal sealed record MistralUsage(
    [property: JsonPropertyName("prompt_tokens")] int? PromptTokens,
    [property: JsonPropertyName("completion_tokens")] int? CompletionTokens);
