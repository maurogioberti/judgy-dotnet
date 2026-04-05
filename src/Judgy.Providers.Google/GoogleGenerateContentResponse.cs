using System.Text.Json.Serialization;

namespace Judgy.Providers.Google;

internal sealed record GoogleGenerateContentResponse(
    [property: JsonPropertyName("candidates")] GoogleCandidate[]? Candidates,
    [property: JsonPropertyName("usageMetadata")] GoogleUsageMetadata? UsageMetadata);

internal sealed record GoogleCandidate(
    [property: JsonPropertyName("content")] GoogleContent? Content);

internal sealed record GoogleUsageMetadata(
    [property: JsonPropertyName("promptTokenCount")] int? PromptTokenCount,
    [property: JsonPropertyName("candidatesTokenCount")] int? CandidatesTokenCount);
