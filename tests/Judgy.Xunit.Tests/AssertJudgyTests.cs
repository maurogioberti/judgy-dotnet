using FluentAssertions;
using Judgy.Assertions;
using Judgy.Evaluation;
using Xunit;
using Xunit.Sdk;

namespace Judgy.Xunit.Tests;

public class AssertJudgyTests
{
    private const double PassingConfidence = 0.90;
    private const double FailingConfidence = 0.30;
    private const double MinimumScoreThreshold = 0.70;
    private const double CustomMinimumScore = 0.50;
    private const string AnyValidReasoning = "Some reasoning";
    private const string AnyValidActualOutput = "actual output";
    private const string AnyValidExpectation = "expected output";
    private const string AnyValidEvaluatorName = "SemanticEvaluator";

    private static readonly TimeSpan AnyValidEvaluationDuration = TimeSpan.FromMilliseconds(100);

    private static EvaluationResult MakeResult(double confidence) =>
        new(
            new EvaluationEvidence(confidence, AnyValidReasoning, AnyValidEvaluatorName),
            AnyValidActualOutput,
            AnyValidExpectation,
            AnyValidEvaluationDuration);

    [Fact]
    public void Should_NotThrow_When_ConfidenceExceedsMinimumScore()
    {
        // Arrange
        var result = MakeResult(PassingConfidence);

        // Act
        var act = () => Assert.Judgy(result, MinimumScoreThreshold);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Should_ThrowFailException_When_ConfidenceBelowMinimumScore()
    {
        // Arrange
        var result = MakeResult(FailingConfidence);

        // Act
        var act = () => Assert.Judgy(result, MinimumScoreThreshold);

        // Assert
        act.Should().ThrowExactly<FailException>()
            .WithMessage("*Judgy semantic assertion failed.*");
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_ResultIsNull()
    {
        // Arrange

        // Act
        var act = () => Assert.Judgy(null!);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>()
            .WithParameterName("result");
    }

    [Fact]
    public void Should_RespectCustomMinimumScore_When_Provided()
    {
        // Arrange — confidence below default (0.70) but above custom (0.50)
        var result = MakeResult(0.60);

        // Act
        var act = () => Assert.Judgy(result, CustomMinimumScore);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Should_UseDefaultMinimumScore_When_NotProvided()
    {
        // Arrange — confidence below default (0.70)
        var result = MakeResult(0.60);

        // Act
        var act = () => Assert.Judgy(result);

        // Assert
        act.Should().ThrowExactly<FailException>();
    }
}
