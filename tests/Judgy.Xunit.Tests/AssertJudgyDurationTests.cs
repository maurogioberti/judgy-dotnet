using FluentAssertions;
using Xunit;
using Xunit.Sdk;

namespace Judgy.Xunit.Tests;

public class AssertJudgyDurationTests
{
    private static readonly TimeSpan ShortDuration = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan MediumDuration = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan LongDuration = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan NegativeDuration = TimeSpan.FromSeconds(-1);

    [Fact]
    public void Should_NotThrow_When_ActualDurationBelowMaximum()
    {
        // Arrange

        // Act
        var act = () => Assert.JudgyDuration(ShortDuration, MediumDuration);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Should_ThrowFailException_When_ActualDurationExceedsMaximum()
    {
        // Arrange

        // Act
        var act = () => Assert.JudgyDuration(LongDuration, MediumDuration);

        // Assert
        act.Should().ThrowExactly<FailException>()
            .WithMessage("*Judgy duration assertion failed.*");
    }

    [Fact]
    public void Should_ThrowArgumentOutOfRangeException_When_ActualDurationIsNegative()
    {
        // Arrange

        // Act
        var act = () => Assert.JudgyDuration(NegativeDuration, MediumDuration);

        // Assert
        act.Should().ThrowExactly<ArgumentOutOfRangeException>()
            .WithParameterName("actualDuration");
    }

    [Fact]
    public void Should_ThrowArgumentOutOfRangeException_When_MaximumDurationIsNegative()
    {
        // Arrange

        // Act
        var act = () => Assert.JudgyDuration(ShortDuration, NegativeDuration);

        // Assert
        act.Should().ThrowExactly<ArgumentOutOfRangeException>()
            .WithParameterName("maximumDuration");
    }
}
