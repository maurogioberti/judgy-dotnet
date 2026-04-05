using FluentAssertions;
using FluentAssertions.Execution;
using Judgy.Assertions;
using Xunit;

namespace Judgy.Core.Tests.Assertions;

public class AssertionDecisionTests
{
    [Fact]
    public void Should_ReturnSucceededTrue_When_PassIsCalled()
    {
        // Arrange

        // Act
        var decision = AssertionDecision.Pass();

        // Assert
        using (new AssertionScope())
        {
            decision.Succeeded.Should().BeTrue();
            decision.Failure.Should().BeNull();
        }
    }

    [Fact]
    public void Should_ReturnSucceededFalse_When_FailIsCalledWithDetails()
    {
        // Arrange
        var decision = MetricAssertionPolicy.EvaluateScore(0.45, 0.80);

        // Act

        // Assert
        using (new AssertionScope())
        {
            decision.Succeeded.Should().BeFalse();
            decision.Failure.Should().NotBeNull();
        }
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_FailIsCalledWithNull()
    {
        // Arrange

        // Act
        Action act = () => AssertionDecision.Fail(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
