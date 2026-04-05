using FluentAssertions;
using FluentAssertions.Execution;
using Judgy.Assertions;
using Xunit;

namespace Judgy.Core.Tests.Assertions;

public class SemanticAssertionOptionsTests
{
    private const double ExplicitMinimumScore = 0.85;
    private const double ExpectedDefaultMinimumScore = 0.70;

    [Fact]
    public void Should_UseDefaultMinimumScore_When_ConstructedWithNoArguments()
    {
        // Arrange

        // Act
        var options = new SemanticAssertionOptions();

        // Assert
        options.MinimumScore.Should().Be(SemanticAssertionOptions.DefaultMinimumScore);
    }

    [Fact]
    public void Should_SetMinimumScore_When_ConstructedWithExplicitValue()
    {
        // Arrange

        // Act
        var options = new SemanticAssertionOptions(ExplicitMinimumScore);

        // Assert
        options.MinimumScore.Should().Be(ExplicitMinimumScore);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(1.0)]
    public void Should_AcceptBoundaryValues_When_MinimumScoreIsAtBoundary(double minimumScore)
    {
        // Arrange

        // Act
        var options = new SemanticAssertionOptions(minimumScore);

        // Assert
        options.MinimumScore.Should().Be(minimumScore);
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
        Action act = () => new SemanticAssertionOptions(minimumScore);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .Which.ParamName.Should().Be("minimumScore");
    }

    [Fact]
    public void Should_HaveDefaultConstant_When_Inspected()
    {
        // Arrange

        // Act

        // Assert
        SemanticAssertionOptions.DefaultMinimumScore.Should().Be(ExpectedDefaultMinimumScore);
    }
}
