using FluentAssertions;
using FluentAssertions.Execution;
using Judgy.Assertions;
using Judgy.Evaluation;
using Judgy.Xunit.Tests.Fakes;
using Xunit;
using Xunit.Sdk;

namespace Judgy.Xunit.Tests;

public class AssertJudgyAsyncTests
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
    public async Task Should_NotThrow_When_ConfidenceExceedsMinimumScore()
    {
        // Arrange
        var fake = new FakeSemanticEvaluator(MakeResult(PassingConfidence));

        // Act
        var act = () => Assert.JudgyAsync(
            AnyValidActualOutput,
            AnyValidExpectation,
            fake,
            MinimumScoreThreshold,
            TestContext.Current.CancellationToken);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Should_ThrowFailException_When_ConfidenceBelowMinimumScore()
    {
        // Arrange
        var fake = new FakeSemanticEvaluator(MakeResult(FailingConfidence));

        // Act
        var act = () => Assert.JudgyAsync(
            AnyValidActualOutput,
            AnyValidExpectation,
            fake,
            MinimumScoreThreshold,
            TestContext.Current.CancellationToken);

        // Assert
        var ex = await act.Should().ThrowExactlyAsync<FailException>();
        ex.WithMessage("*Judgy semantic assertion failed.*");
    }

    [Fact]
    public async Task Should_ThrowArgumentNullException_When_ActualOutputIsNull()
    {
        // Arrange
        var fake = new FakeSemanticEvaluator(MakeResult(PassingConfidence));

        // Act
        var act = () => Assert.JudgyAsync(
            null!,
            AnyValidExpectation,
            fake,
            MinimumScoreThreshold,
            TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowExactlyAsync<ArgumentNullException>()
            .WithParameterName("actualOutput");
    }

    [Fact]
    public async Task Should_ThrowArgumentNullException_When_ExpectationIsNull()
    {
        // Arrange
        var fake = new FakeSemanticEvaluator(MakeResult(PassingConfidence));

        // Act
        var act = () => Assert.JudgyAsync(
            AnyValidActualOutput,
            null!,
            fake,
            MinimumScoreThreshold,
            TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowExactlyAsync<ArgumentNullException>()
            .WithParameterName("expectation");
    }

    [Fact]
    public async Task Should_ThrowArgumentNullException_When_EvaluatorIsNull()
    {
        // Arrange

        // Act
        var act = () => Assert.JudgyAsync(
            AnyValidActualOutput,
            AnyValidExpectation,
            null!,
            MinimumScoreThreshold,
            TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowExactlyAsync<ArgumentNullException>()
            .WithParameterName("evaluator");
    }

    [Fact]
    public async Task Should_ForwardCancellationToken_When_Called()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        var fake = new FakeSemanticEvaluator(MakeResult(PassingConfidence));

        // Act
        await Assert.JudgyAsync(
            AnyValidActualOutput,
            AnyValidExpectation,
            fake,
            MinimumScoreThreshold,
            token);

        // Assert
        fake.CapturedCancellationToken.Should().Be(token);
    }

    [Fact]
    public async Task Should_CallEvaluatorExactlyOnce_When_Called()
    {
        // Arrange
        var fake = new FakeSemanticEvaluator(MakeResult(PassingConfidence));

        // Act
        await Assert.JudgyAsync(
            AnyValidActualOutput,
            AnyValidExpectation,
            fake,
            MinimumScoreThreshold,
            TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            fake.CallCount.Should().Be(1);
            fake.CapturedActualOutput.Should().Be(AnyValidActualOutput);
            fake.CapturedExpectation.Should().Be(AnyValidExpectation);
        }
    }

    [Fact]
    public async Task Should_RespectCustomMinimumScore_When_Provided()
    {
        // Arrange — confidence below default (0.70) but above custom (0.50)
        var fake = new FakeSemanticEvaluator(MakeResult(0.60));

        // Act
        var act = () => Assert.JudgyAsync(
            AnyValidActualOutput,
            AnyValidExpectation,
            fake,
            CustomMinimumScore,
            TestContext.Current.CancellationToken);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
