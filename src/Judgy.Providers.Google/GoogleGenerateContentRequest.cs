using System.Text.Json.Serialization;

namespace Judgy.Providers.Google;

internal sealed record GoogleGenerateContentRequest(
    [property: JsonPropertyName("contents")] GoogleContent[] Contents,
    [property: JsonPropertyName("generationConfig")] GoogleGenerationConfig GenerationConfig,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [property: JsonPropertyName("systemInstruction")] GoogleContent? SystemInstruction = null);

internal sealed record GoogleContent(
    [property: JsonPropertyName("parts")] GooglePart[] Parts,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [property: JsonPropertyName("role")] string? Role = null);

internal sealed record GooglePart(
    [property: JsonPropertyName("text")] string Text);

internal sealed record GoogleGenerationConfig(
    [property: JsonPropertyName("temperature")] double Temperature,
    [property: JsonPropertyName("maxOutputTokens")] int MaxOutputTokens);
