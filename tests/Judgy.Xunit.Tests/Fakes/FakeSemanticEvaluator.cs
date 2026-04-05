using Judgy.Evaluation;

namespace Judgy.Xunit.Tests.Fakes;

internal sealed class FakeSemanticEvaluator(EvaluationResult result) : ISemanticEvaluator
{
    private readonly EvaluationResult _result = result;

    public string? CapturedActualOutput { get; private set; }
    public string? CapturedExpectation { get; private set; }
    public CancellationToken CapturedCancellationToken { get; private set; }
    public int CallCount { get; private set; }

    public Task<EvaluationResult> EvaluateAsync(
        string actualOutput,
        string expectation,
        CancellationToken cancellationToken = default)
    {
        CallCount++;
        CapturedActualOutput = actualOutput;
        CapturedExpectation = expectation;
        CapturedCancellationToken = cancellationToken;

        return Task.FromResult(_result);
    }
}
