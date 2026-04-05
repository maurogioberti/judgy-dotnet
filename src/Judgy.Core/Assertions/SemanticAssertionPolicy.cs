using Judgy.Evaluation;

namespace Judgy.Assertions;

public static class SemanticAssertionPolicy
{
    public static AssertionDecision Evaluate(EvaluationResult result, SemanticAssertionOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        options ??= new SemanticAssertionOptions();

        if (result.Evidence.Confidence >= options.MinimumScore)
            return AssertionDecision.Pass();

        var failure = AssertionFailureDetails.ForSemantic(
            result.Expectation,
            result.ActualOutput,
            result.Evidence.Confidence,
            options.MinimumScore,
            result.Evidence.Reasoning);

        return AssertionDecision.Fail(failure);
    }
}
