#pragma warning disable CA1052 // Static holder types should be static

using Judgy.Assertions;
using Judgy.Evaluation;
using Xunit.Sdk;

namespace Xunit;

partial class Assert
{
    public static async Task JudgyAsync(
        string actualOutput,
        string expectation,
        ISemanticEvaluator evaluator,
        double minimumScore = SemanticAssertionOptions.DefaultMinimumScore,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actualOutput);
        ArgumentNullException.ThrowIfNull(expectation);
        ArgumentNullException.ThrowIfNull(evaluator);

        var result = await evaluator.EvaluateAsync(actualOutput, expectation, cancellationToken);
        var options = new SemanticAssertionOptions(minimumScore);
        var decision = SemanticAssertionPolicy.Evaluate(result, options);

        ThrowIfFailed(decision);
    }

    public static void Judgy(
        EvaluationResult result,
        double minimumScore = SemanticAssertionOptions.DefaultMinimumScore)
    {
        ArgumentNullException.ThrowIfNull(result);

        var options = new SemanticAssertionOptions(minimumScore);
        var decision = SemanticAssertionPolicy.Evaluate(result, options);

        ThrowIfFailed(decision);
    }

    public static void JudgyScore(double actualScore, double minimumScore)
    {
        var decision = MetricAssertionPolicy.EvaluateScore(actualScore, minimumScore);

        ThrowIfFailed(decision);
    }

    public static void JudgyDuration(TimeSpan actualDuration, TimeSpan maximumDuration)
    {
        var decision = MetricAssertionPolicy.EvaluateDuration(actualDuration, maximumDuration);

        ThrowIfFailed(decision);
    }

    private static void ThrowIfFailed(AssertionDecision decision)
    {
        if (!decision.Succeeded)
        {
            var message = AssertionFailureMessageFormatter.Format(decision.Failure!);
            throw FailException.ForFailure(message);
        }
    }
}
