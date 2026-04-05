using FluentAssertions;
using Judgy.Assertions;
using Judgy.Evaluation;
using Xunit;

namespace Judgy.Core.Tests.Assertions;

public class AssertionFailureMessageFormatterTests
{
    private const double SemanticFailingConfidence = 0.12;
    private const string SemanticReasoning = "The actual output is an HTTP error message, not a greeting.";
    private const string SemanticActualOutput = "ERROR: 503 Service Unavailable";
    private const string SemanticExpectation = "A polite greeting";
    private const double SemanticMinimumScore = 0.70;

    private static readonly TimeSpan AnyValidEvaluationDuration = TimeSpan.FromMilliseconds(100);

    private const double ScoreFailingActual = 0.45;
    private const double ScoreMinimumThreshold = 0.80;

    private static readonly TimeSpan DurationFailingActual = TimeSpan.FromSeconds(5.23);
    private static readonly TimeSpan DurationMaximum = TimeSpan.FromSeconds(2);

    [Fact]
    public void Should_FormatSemanticFailure_When_KindIsSemanticExpectation()
    {
        // Arrange
        var evidence = new EvaluationEvidence(SemanticFailingConfidence, SemanticReasoning, "SemanticEvaluator");
        var result = new EvaluationResult(evidence, SemanticActualOutput, SemanticExpectation, AnyValidEvaluationDuration);
        var decision = SemanticAssertionPolicy.Evaluate(result, new SemanticAssertionOptions(SemanticMinimumScore));

        // Act
        var message = AssertionFailureMessageFormatter.Format(decision.Failure!);

        // Assert
        message.Should().Contain("Judgy semantic assertion failed.");
        message.Should().Contain($"Expectation  : {SemanticExpectation}");
        message.Should().Contain($"Actual       : {SemanticActualOutput}");
        message.Should().Contain($"Score        : {SemanticFailingConfidence:F2}");
        message.Should().Contain($"MinimumScore : {SemanticMinimumScore:F2}");
        message.Should().Contain($"Reasoning    : {SemanticReasoning}");
    }

    [Fact]
    public void Should_FormatScoreFailure_When_KindIsScore()
    {
        // Arrange
        var decision = MetricAssertionPolicy.EvaluateScore(ScoreFailingActual, ScoreMinimumThreshold);

        // Act
        var message = AssertionFailureMessageFormatter.Format(decision.Failure!);

        // Assert
        message.Should().Contain("Judgy score assertion failed.");
        message.Should().Contain($"Score        : {ScoreFailingActual:F2}");
        message.Should().Contain($"MinimumScore : {ScoreMinimumThreshold:F2}");
        message.Should().NotContain("Expectation");
        message.Should().NotContain("Actual");
        message.Should().NotContain("Reasoning");
        message.Should().NotContain("Duration");
    }

    [Fact]
    public void Should_FormatDurationFailure_When_KindIsDuration()
    {
        // Arrange
        var decision = MetricAssertionPolicy.EvaluateDuration(DurationFailingActual, DurationMaximum);

        // Act
        var message = AssertionFailureMessageFormatter.Format(decision.Failure!);

        // Assert
        message.Should().Contain("Judgy duration assertion failed.");
        message.Should().Contain("Duration");
        message.Should().Contain("MaximumDuration");
        message.Should().NotContain("Score");
        message.Should().NotContain("Expectation");
        message.Should().NotContain("Reasoning");
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_FailureIsNull()
    {
        // Arrange

        // Act
        Action act = () => AssertionFailureMessageFormatter.Format(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
