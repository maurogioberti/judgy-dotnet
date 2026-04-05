using System.Text.Json.Serialization;

namespace Judgy.Providers.Anthropic;

internal sealed record AnthropicMessageResponse(
    [property: JsonPropertyName("content")] AnthropicContentBlock[]? Content,
    [property: JsonPropertyName("usage")] AnthropicUsage? Usage);

internal sealed record AnthropicContentBlock(
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("text")] string? Text);

internal sealed record AnthropicUsage(
    [property: JsonPropertyName("input_tokens")] int? InputTokens,
    [property: JsonPropertyName("output_tokens")] int? OutputTokens);
