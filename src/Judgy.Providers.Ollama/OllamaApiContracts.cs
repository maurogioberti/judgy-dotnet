using System.Text.Json.Serialization;

namespace Judgy.Providers.Ollama;

internal sealed record OllamaChatRequest(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("stream")] bool Stream,
    [property: JsonPropertyName("messages")] OllamaChatMessage[] Messages);

internal sealed record OllamaChatMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content);

internal sealed record OllamaChatResponse(
    [property: JsonPropertyName("message")] OllamaChatMessage? Message,
    [property: JsonPropertyName("prompt_eval_count")] int? PromptEvalCount,
    [property: JsonPropertyName("eval_count")] int? EvalCount);
