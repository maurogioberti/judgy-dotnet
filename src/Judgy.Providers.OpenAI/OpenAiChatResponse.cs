using System.Text.Json.Serialization;

namespace Judgy.Providers.OpenAI;

internal sealed record OpenAiChatResponse(
    [property: JsonPropertyName("choices")] OpenAiChoice[]? Choices,
    [property: JsonPropertyName("usage")] OpenAiUsage? Usage);

internal sealed record OpenAiChoice(
    [property: JsonPropertyName("message")] OpenAiChoiceMessage? Message);

internal sealed record OpenAiChoiceMessage(
    [property: JsonPropertyName("content")] string? Content);

internal sealed record OpenAiUsage(
    [property: JsonPropertyName("prompt_tokens")] int? PromptTokens,
    [property: JsonPropertyName("completion_tokens")] int? CompletionTokens);
