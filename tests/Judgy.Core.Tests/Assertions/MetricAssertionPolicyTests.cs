using FluentAssertions;
using FluentAssertions.Execution;
using Judgy.Assertions;
using Xunit;

namespace Judgy.Core.Tests.Assertions;

public class MetricAssertionPolicyTests
{
    private const double PassingScore = 0.90;
    private const double MinimumScoreThreshold = 0.70;
    private const double FailingScore = 0.45;
    private const double HighMinimumScore = 0.80;
    private const double AnyValidScore = 0.50;

    private static readonly TimeSpan ShortDuration = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan MediumDuration = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan LongDuration = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan NegativeDuration = TimeSpan.FromSeconds(-1);


    [Fact]
    public void EvaluateScore_Should_ReturnSuccess_When_ActualScoreExceedsMinimum()
    {
        // Arrange

        // Act
        var decision = MetricAssertionPolicy.EvaluateScore(PassingScore, MinimumScoreThreshold);

        // Assert
        decision.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void EvaluateScore_Should_ReturnSuccess_When_ActualScoreEqualsMinimum()
    {
        // Arrange

        // Act
        var decision = MetricAssertionPolicy.EvaluateScore(MinimumScoreThreshold, MinimumScoreThreshold);

        // Assert
        decision.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void EvaluateScore_Should_ReturnFailure_When_ActualScoreBelowMinimum()
    {
        // Arrange

        // Act
        var decision = MetricAssertionPolicy.EvaluateScore(FailingScore, HighMinimumScore);

        // Assert
        using (new AssertionScope())
        {
            decision.Succeeded.Should().BeFalse();
            decision.Failure.Should().NotBeNull();
            decision.Failure!.Kind.Should().Be(AssertionFailureKind.Score);
            decision.Failure.ActualScore.Should().Be(FailingScore);
            decision.Failure.MinimumScore.Should().Be(HighMinimumScore);
            decision.Failure.Expectation.Should().BeNull();
            decision.Failure.ActualOutput.Should().BeNull();
            decision.Failure.Reasoning.Should().BeNull();
            decision.Failure.ActualDuration.Should().BeNull();
            decision.Failure.MaximumDuration.Should().BeNull();
        }
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void EvaluateScore_Should_ThrowArgumentOutOfRangeException_When_ActualScoreIsInvalid(double actualScore)
    {
        // Arrange

        // Act
        Action act = () => MetricAssertionPolicy.EvaluateScore(actualScore, MinimumScoreThreshold);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .Which.ParamName.Should().Be("actualScore");
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void EvaluateScore_Should_ThrowArgumentOutOfRangeException_When_MinimumScoreIsInvalid(double minimumScore)
    {
        // Arrange

        // Act
        Action act = () => MetricAssertionPolicy.EvaluateScore(AnyValidScore, minimumScore);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .Which.ParamName.Should().Be("minimumScore");
    }

    // --- Duration ---

    [Fact]
    public void EvaluateDuration_Should_ReturnSuccess_When_ActualBelowMaximum()
    {
        // Arrange

        // Act
        var decision = MetricAssertionPolicy.EvaluateDuration(ShortDuration, MediumDuration);

        // Assert
        decision.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void EvaluateDuration_Should_ReturnSuccess_When_ActualEqualsMaximum()
    {
        // Arrange

        // Act
        var decision = MetricAssertionPolicy.EvaluateDuration(MediumDuration, MediumDuration);

        // Assert
        decision.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void EvaluateDuration_Should_ReturnFailure_When_ActualExceedsMaximum()
    {
        // Arrange

        // Act
        var decision = MetricAssertionPolicy.EvaluateDuration(LongDuration, MediumDuration);

        // Assert
        using (new AssertionScope())
        {
            decision.Succeeded.Should().BeFalse();
            decision.Failure.Should().NotBeNull();
            decision.Failure!.Kind.Should().Be(AssertionFailureKind.Duration);
            decision.Failure.ActualDuration.Should().Be(LongDuration);
            decision.Failure.MaximumDuration.Should().Be(MediumDuration);
            decision.Failure.Expectation.Should().BeNull();
            decision.Failure.ActualOutput.Should().BeNull();
            decision.Failure.ActualScore.Should().BeNull();
            decision.Failure.MinimumScore.Should().BeNull();
            decision.Failure.Reasoning.Should().BeNull();
        }
    }

    [Fact]
    public void EvaluateDuration_Should_ThrowArgumentOutOfRangeException_When_ActualDurationIsNegative()
    {
        // Arrange

        // Act
        Action act = () => MetricAssertionPolicy.EvaluateDuration(NegativeDuration, MediumDuration);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .Which.ParamName.Should().Be("actualDuration");
    }

    [Fact]
    public void EvaluateDuration_Should_ThrowArgumentOutOfRangeException_When_MaximumDurationIsNegative()
    {
        // Arrange

        // Act
        Action act = () => MetricAssertionPolicy.EvaluateDuration(ShortDuration, NegativeDuration);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .Which.ParamName.Should().Be("maximumDuration");
    }
}
