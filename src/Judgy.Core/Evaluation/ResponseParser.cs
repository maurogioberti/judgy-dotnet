using System.Text.Json;

namespace Judgy.Evaluation;

internal static class ResponseParser
{
    private const string FallbackReasoning = "No reasoning provided by the evaluator.";
    private const string MissingConfidenceReasoning = "Response did not contain a valid confidence value.";
    private const string ParseFailureReasoning = "Failed to parse evaluator response as JSON.";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    internal static EvaluationEvidence Parse(string responseText, string evaluatorName)
    {
        try
        {
            var text = StripCodeFences(responseText);
            var parsed = JsonSerializer.Deserialize<ParsedResponse>(text, JsonOptions);

            if (parsed?.Confidence is null)
                return new EvaluationEvidence(0.0, MissingConfidenceReasoning, evaluatorName);

            var confidence = parsed.Confidence.Value;

            if (double.IsNaN(confidence) || double.IsInfinity(confidence))
                return new EvaluationEvidence(0.0, MissingConfidenceReasoning, evaluatorName);

            confidence = Math.Clamp(confidence, 0.0, 1.0);

            var reasoning = string.IsNullOrWhiteSpace(parsed.Reasoning)
                ? FallbackReasoning
                : parsed.Reasoning;

            return new EvaluationEvidence(confidence, reasoning, evaluatorName);
        }
        catch (JsonException)
        {
            return new EvaluationEvidence(0.0, ParseFailureReasoning, evaluatorName);
        }
        catch (Exception)
        {
            return new EvaluationEvidence(0.0, ParseFailureReasoning, evaluatorName);
        }
    }

    private static string StripCodeFences(string text)
    {
        var trimmed = text.Trim();

        if (!trimmed.StartsWith("```"))
            return trimmed;

        var firstNewline = trimmed.IndexOf('\n');
        if (firstNewline < 0)
            return trimmed;

        var content = trimmed[(firstNewline + 1)..];

        var lastFence = content.LastIndexOf("```");
        if (lastFence >= 0)
            content = content[..lastFence];

        return content.Trim();
    }

    private sealed class ParsedResponse
    {
        public double? Confidence { get; set; }
        public string? Reasoning { get; set; }
    }
}
