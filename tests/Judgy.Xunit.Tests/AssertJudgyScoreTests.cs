using FluentAssertions;
using Xunit;
using Xunit.Sdk;

namespace Judgy.Xunit.Tests;

public class AssertJudgyScoreTests
{
    private const double PassingScore = 0.90;
    private const double MinimumScoreThreshold = 0.70;
    private const double FailingScore = 0.45;
    private const double AnyValidScore = 0.50;

    [Fact]
    public void Should_NotThrow_When_ActualScoreExceedsMinimum()
    {
        // Arrange

        // Act
        var act = () => Assert.JudgyScore(PassingScore, MinimumScoreThreshold);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Should_ThrowFailException_When_ActualScoreBelowMinimum()
    {
        // Arrange

        // Act
        var act = () => Assert.JudgyScore(FailingScore, MinimumScoreThreshold);

        // Assert
        act.Should().ThrowExactly<FailException>()
            .WithMessage("*Judgy score assertion failed.*");
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Should_ThrowArgumentOutOfRangeException_When_ActualScoreIsInvalid(double actualScore)
    {
        // Arrange

        // Act
        var act = () => Assert.JudgyScore(actualScore, MinimumScoreThreshold);

        // Assert
        act.Should().ThrowExactly<ArgumentOutOfRangeException>()
            .WithParameterName("actualScore");
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Should_ThrowArgumentOutOfRangeException_When_MinimumScoreIsInvalid(double minimumScore)
    {
        // Arrange

        // Act
        var act = () => Assert.JudgyScore(AnyValidScore, minimumScore);

        // Assert
        act.Should().ThrowExactly<ArgumentOutOfRangeException>()
            .WithParameterName("minimumScore");
    }
}
