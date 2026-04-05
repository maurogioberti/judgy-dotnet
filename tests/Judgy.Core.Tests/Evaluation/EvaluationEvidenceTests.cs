using FluentAssertions;
using FluentAssertions.Execution;
using Judgy.Evaluation;
using Xunit;

namespace Judgy.Core.Tests.Evaluation;

public class EvaluationEvidenceTests
{
    private const double AnyValidConfidence = 0.5;
    private const double AlternateValidConfidence = 0.1;
    private const double DifferentValidConfidence = 0.8;

    private const string AnyValidReasoning = "Good match";
    private const string AlternateValidReasoning = "No match";
    private const string AnyValidEvaluatorName = "SemanticEvaluator";

    [Fact]
    public void Should_SetProperties_When_ConstructedWithValidInputs()
    {
        // Arrange
        EvaluationEvidence? evidence = null;

        // Act
        evidence = new EvaluationEvidence(
            AlternateValidConfidence,
            AlternateValidReasoning,
            AnyValidEvaluatorName);

        // Assert
        using (new AssertionScope())
        {
            evidence.Should().NotBeNull();
            evidence!.Confidence.Should().Be(AlternateValidConfidence);
            evidence.Reasoning.Should().Be(AlternateValidReasoning);
            evidence.EvaluatorName.Should().Be(AnyValidEvaluatorName);
        }
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(1.0)]
    public void Should_SetConfidence_When_ConfidenceIsAtTheBoundary(double confidence)
    {
        // Arrange
        EvaluationEvidence? evidence = null;

        // Act
        evidence = new EvaluationEvidence(
            confidence,
            AnyValidReasoning,
            AnyValidEvaluatorName);

        // Assert
        using (new AssertionScope())
        {
            evidence.Should().NotBeNull();
            evidence!.Confidence.Should().Be(confidence);
        }
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Should_ThrowArgumentOutOfRangeException_When_ConfidenceIsInvalid(double confidence)
    {
        // Arrange

        // Act
        Action act = () => new EvaluationEvidence(
            confidence,
            AnyValidReasoning,
            AnyValidEvaluatorName);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .Which.ParamName.Should().Be(ToParameterName(nameof(EvaluationEvidence.Confidence)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Should_ThrowArgumentException_When_ReasoningIsNullOrWhiteSpace(string? reasoning)
    {
        // Arrange

        // Act
        Action act = () => new EvaluationEvidence(
            AnyValidConfidence,
            reasoning!,
            AnyValidEvaluatorName);

        // Assert
        act.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be(ToParameterName(nameof(EvaluationEvidence.Reasoning)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Should_ThrowArgumentException_When_EvaluatorNameIsNullOrWhiteSpace(string? evaluatorName)
    {
        // Arrange

        // Act
        Action act = () => new EvaluationEvidence(
            AnyValidConfidence,
            AnyValidReasoning,
            evaluatorName!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be(ToParameterName(nameof(EvaluationEvidence.EvaluatorName)));
    }

    [Fact]
    public void Should_BeEqual_When_InstancesHaveIdenticalValues()
    {
        // Arrange
        var first = new EvaluationEvidence(
            DifferentValidConfidence,
            AnyValidReasoning,
            AnyValidEvaluatorName);
        var second = new EvaluationEvidence(
            DifferentValidConfidence,
            AnyValidReasoning,
            AnyValidEvaluatorName);

        // Act
        var actual = first;

        // Assert
        using (new AssertionScope())
        {
            actual.Should().Be(second);
        }
    }

    [Fact]
    public void Should_NotBeEqual_When_ConfidenceDiffers()
    {
        // Arrange
        var first = new EvaluationEvidence(
            AnyValidConfidence,
            AnyValidReasoning,
            AnyValidEvaluatorName);
        var second = new EvaluationEvidence(
            DifferentValidConfidence,
            AnyValidReasoning,
            AnyValidEvaluatorName);

        // Act
        var actual = first;

        // Assert
        using (new AssertionScope())
        {
            actual.Should().NotBe(second);
        }
    }

    private static string ToParameterName(string memberName) =>
        char.ToLowerInvariant(memberName[0]) + memberName[1..];
}
