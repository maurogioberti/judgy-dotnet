using System.Diagnostics;
using Judgy.Providers;

namespace Judgy.Evaluation;

public class SemanticEvaluator : ISemanticEvaluator
{
    public const string EvaluatorName = "SemanticEvaluator";

    private readonly ILlmProvider _provider;

    public SemanticEvaluator(ILlmProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _provider = provider;
    }

    public async Task<EvaluationResult> EvaluateAsync(string actualOutput, string expectation, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actualOutput))
            throw new ArgumentException("ActualOutput cannot be null or whitespace.", nameof(actualOutput));

        if (string.IsNullOrWhiteSpace(expectation))
            throw new ArgumentException("Expectation cannot be null or whitespace.", nameof(expectation));

        var stopwatch = Stopwatch.StartNew();

        var systemPrompt = PromptBuilder.BuildSystemPrompt();
        var userPrompt = PromptBuilder.BuildUserPrompt(actualOutput, expectation);
        var request = new LlmRequest(userPrompt, systemPrompt, temperature: 0.0);

        var response = await _provider.CompleteAsync(request, cancellationToken);

        var evidence = ResponseParser.Parse(response.Text, EvaluatorName);

        stopwatch.Stop();

        return new EvaluationResult(evidence, actualOutput, expectation, stopwatch.Elapsed);
    }
}
