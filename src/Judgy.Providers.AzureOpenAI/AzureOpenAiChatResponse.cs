using System.Text.Json.Serialization;

namespace Judgy.Providers.AzureOpenAI;

internal sealed record AzureOpenAiChatResponse(
    [property: JsonPropertyName("choices")] AzureOpenAiChoice[]? Choices,
    [property: JsonPropertyName("usage")] AzureOpenAiUsage? Usage);

internal sealed record AzureOpenAiChoice(
    [property: JsonPropertyName("message")] AzureOpenAiMessage? Message);

internal sealed record AzureOpenAiMessage(
    [property: JsonPropertyName("content")] string? Content);

internal sealed record AzureOpenAiUsage(
    [property: JsonPropertyName("prompt_tokens")] int? PromptTokens,
    [property: JsonPropertyName("completion_tokens")] int? CompletionTokens);
