using FluentAssertions;
using FluentAssertions.Execution;
using Judgy.Assertions;
using Judgy.Evaluation;
using Xunit;

namespace Judgy.Core.Tests.Assertions;

public class SemanticAssertionPolicyTests
{
    private const string AnyValidReasoning = "Some reasoning";
    private const string AnyValidActualOutput = "actual output";
    private const string AnyValidExpectation = "expected output";
    private const double PassingConfidence = 0.90;
    private const double MinimumScoreThreshold = 0.70;
    private const double FailingConfidence = 0.50;
    private const double DefaultOptionsFailureConfidence = 0.60;

    private static readonly TimeSpan AnyValidEvaluationDuration = TimeSpan.FromMilliseconds(100);

    [Fact]
    public void Should_ReturnSuccess_When_ConfidenceExceedsMinimumScore()
    {
        // Arrange
        var evidence = new EvaluationEvidence(
            PassingConfidence,
            AnyValidReasoning,
            SemanticEvaluator.EvaluatorName);
        var result = new EvaluationResult(
            evidence,
            AnyValidActualOutput,
            AnyValidExpectation,
            AnyValidEvaluationDuration);
        var options = new SemanticAssertionOptions(MinimumScoreThreshold);

        // Act
        var decision = SemanticAssertionPolicy.Evaluate(result, options);

        // Assert
        using (new AssertionScope())
        {
            decision.Succeeded.Should().BeTrue();
            decision.Failure.Should().BeNull();
        }
    }

    [Fact]
    public void Should_ReturnSuccess_When_ConfidenceEqualsMinimumScore()
    {
        // Arrange
        var evidence = new EvaluationEvidence(
            MinimumScoreThreshold,
            AnyValidReasoning,
            SemanticEvaluator.EvaluatorName);
        var result = new EvaluationResult(
            evidence,
            AnyValidActualOutput,
            AnyValidExpectation,
            AnyValidEvaluationDuration);
        var options = new SemanticAssertionOptions(MinimumScoreThreshold);

        // Act
        var decision = SemanticAssertionPolicy.Evaluate(result, options);

        // Assert
        using (new AssertionScope())
        {
            decision.Succeeded.Should().BeTrue();
            decision.Failure.Should().BeNull();
        }
    }

    [Fact]
    public void Should_ReturnFailure_When_ConfidenceBelowMinimumScore()
    {
        // Arrange
        var evidence = new EvaluationEvidence(
            FailingConfidence,
            AnyValidReasoning,
            SemanticEvaluator.EvaluatorName);
        var result = new EvaluationResult(
            evidence,
            AnyValidActualOutput,
            AnyValidExpectation,
            AnyValidEvaluationDuration);
        var options = new SemanticAssertionOptions(MinimumScoreThreshold);

        // Act
        var decision = SemanticAssertionPolicy.Evaluate(result, options);

        // Assert
        using (new AssertionScope())
        {
            decision.Succeeded.Should().BeFalse();
            decision.Failure.Should().NotBeNull();
            decision.Failure!.Kind.Should().Be(AssertionFailureKind.SemanticExpectation);
            decision.Failure.Expectation.Should().Be(AnyValidExpectation);
            decision.Failure.ActualOutput.Should().Be(AnyValidActualOutput);
            decision.Failure.ActualScore.Should().Be(FailingConfidence);
            decision.Failure.MinimumScore.Should().Be(MinimumScoreThreshold);
            decision.Failure.Reasoning.Should().Be(AnyValidReasoning);
            decision.Failure.ActualDuration.Should().BeNull();
            decision.Failure.MaximumDuration.Should().BeNull();
        }
    }

    [Fact]
    public void Should_UseDefaultOptions_When_OptionsIsNull()
    {
        // Arrange
        var evidence = new EvaluationEvidence(
            DefaultOptionsFailureConfidence,
            AnyValidReasoning,
            SemanticEvaluator.EvaluatorName);
        var result = new EvaluationResult(
            evidence,
            AnyValidActualOutput,
            AnyValidExpectation,
            AnyValidEvaluationDuration);

        // Act
        var decision = SemanticAssertionPolicy.Evaluate(result, null);

        // Assert
        using (new AssertionScope())
        {
            decision.Succeeded.Should().BeFalse();
            decision.Failure!.MinimumScore.Should().Be(SemanticAssertionOptions.DefaultMinimumScore);
        }
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_ResultIsNull()
    {
        // Arrange
        EvaluationResult? result = null;

        // Act
        Action act = () => SemanticAssertionPolicy.Evaluate(result!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be(nameof(result));
    }
}
