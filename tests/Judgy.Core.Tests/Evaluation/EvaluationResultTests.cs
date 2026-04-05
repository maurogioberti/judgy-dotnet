using FluentAssertions;
using FluentAssertions.Execution;
using Judgy.Evaluation;
using Xunit;

namespace Judgy.Core.Tests.Evaluation;

public class EvaluationResultTests
{
    private const double AnyValidConfidence = 0.5;
    private const string AnyValidReasoning = "Good match";
    private const string AnyValidEvaluatorName = "SemanticEvaluator";
    private const string AnyValidActualOutput = "actual output";
    private const string AlternateValidActualOutput = "actual";
    private const string AnyValidExpectation = "expected output";
    private const string AlternateValidExpectation = "expected";

    private static readonly TimeSpan AnyValidEvaluationDuration = TimeSpan.FromSeconds(1);

    [Fact]
    public void Should_SetProperties_When_ConstructedWithValidInputs()
    {
        // Arrange
        var evidence = new EvaluationEvidence(
            AnyValidConfidence,
            AnyValidReasoning,
            AnyValidEvaluatorName);
        EvaluationResult? result = null;

        // Act
        result = new EvaluationResult(
            evidence,
            AnyValidActualOutput,
            AnyValidExpectation,
            AnyValidEvaluationDuration);

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.Evidence.Should().BeSameAs(evidence);
            result.ActualOutput.Should().Be(AnyValidActualOutput);
            result.Expectation.Should().Be(AnyValidExpectation);
            result.EvaluationDuration.Should().Be(AnyValidEvaluationDuration);
        }
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_EvidenceIsNull()
    {
        // Arrange
        EvaluationEvidence? evidence = null;

        // Act
        Action act = () => new EvaluationResult(
            evidence!,
            AlternateValidActualOutput,
            AlternateValidExpectation,
            AnyValidEvaluationDuration);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be(ToParameterName(nameof(EvaluationResult.Evidence)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(150)]
    [InlineData(1000)]
    public void Should_SetEvaluationDuration_When_EvaluationDurationIsNonNegative(int durationInMilliseconds)
    {
        // Arrange
        var evidence = new EvaluationEvidence(
            AnyValidConfidence,
            AnyValidReasoning,
            AnyValidEvaluatorName);
        var evaluationDuration = TimeSpan.FromMilliseconds(durationInMilliseconds);
        EvaluationResult? result = null;

        // Act
        result = new EvaluationResult(
            evidence,
            AnyValidActualOutput,
            AnyValidExpectation,
            evaluationDuration);

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.EvaluationDuration.Should().Be(evaluationDuration);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Should_ThrowArgumentException_When_ActualOutputIsNullOrWhiteSpace(string? actualOutput)
    {
        // Arrange
        var evidence = new EvaluationEvidence(
            AnyValidConfidence,
            AnyValidReasoning,
            AnyValidEvaluatorName);

        // Act
        Action act = () => new EvaluationResult(
            evidence,
            actualOutput!,
            AlternateValidExpectation,
            AnyValidEvaluationDuration);

        // Assert
        act.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be(ToParameterName(nameof(EvaluationResult.ActualOutput)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Should_ThrowArgumentException_When_ExpectationIsNullOrWhiteSpace(string? expectation)
    {
        // Arrange
        var evidence = new EvaluationEvidence(
            AnyValidConfidence,
            AnyValidReasoning,
            AnyValidEvaluatorName);

        // Act
        Action act = () => new EvaluationResult(
            evidence,
            AlternateValidActualOutput,
            expectation!,
            AnyValidEvaluationDuration);

        // Assert
        act.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be(ToParameterName(nameof(EvaluationResult.Expectation)));
    }

    [Theory]
    [InlineData(-1)]
    public void Should_ThrowArgumentOutOfRangeException_When_EvaluationDurationIsNegative(int durationInMilliseconds)
    {
        // Arrange
        var evidence = new EvaluationEvidence(
            AnyValidConfidence,
            AnyValidReasoning,
            AnyValidEvaluatorName);
        var evaluationDuration = TimeSpan.FromMilliseconds(durationInMilliseconds);

        // Act
        Action act = () => new EvaluationResult(
            evidence,
            AlternateValidActualOutput,
            AlternateValidExpectation,
            evaluationDuration);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .Which.ParamName.Should().Be(ToParameterName(nameof(EvaluationResult.EvaluationDuration)));
    }

    private static string ToParameterName(string memberName) =>
        char.ToLowerInvariant(memberName[0]) + memberName[1..];
}
