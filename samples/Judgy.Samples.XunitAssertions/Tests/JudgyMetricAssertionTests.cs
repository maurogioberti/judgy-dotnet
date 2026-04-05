using Xunit;

namespace Judgy.Samples.XunitAssertions;

public sealed class JudgyMetricAssertionTests : JudgeTestBase
{
    private const double MinimumScore = 0.70;

    [Fact]
    public async Task EvaluatingValidAnswer_ScoreExceedsMinimum()
    {
        // Arrange
        const string answer = "The capital of France is Paris.";
        const string expectation = "The answer identifies Paris as the capital of France";
        var evaluator = CreateJudgeEvaluator();

        // Act
        var result = await evaluator.EvaluateAsync(answer, expectation, CancellationToken);

        // Assert
        Assert.JudgyScore(result.Evidence.Confidence, MinimumScore);
    }

    [Fact]
    public async Task EvaluatingValidAnswer_DurationIsWithinLimit()
    {
        // Arrange
        const string answer = "The capital of France is Paris.";
        const string expectation = "The answer identifies Paris as the capital of France";
        var evaluator = CreateJudgeEvaluator();

        // Act
        var result = await evaluator.EvaluateAsync(answer, expectation, CancellationToken);

        // Assert
        Assert.JudgyDuration(result.EvaluationDuration, GetMaximumDuration());
    }
}
