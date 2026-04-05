using FluentAssertions;
using FluentAssertions.Execution;
using Judgy.Providers;
using Xunit;

namespace Judgy.Core.Tests.Providers;

public class LlmRequestTests
{
    private const string AnyValidPrompt = "What is 2+2?";
    private const string AlternateValidPrompt = "Summarize this request.";
    private const string DifferentValidPrompt = "Explain the answer.";
    private const string AnyValidSystemPrompt = "You are a judge.";
    private const double AnyValidTemperature = 0.7;
    private const double AlternateValidTemperature = 0.5;
    private const int AnyValidMaxTokens = 100;

    [Fact]
    public void Should_SetDefaults_When_ConstructedWithPromptOnly()
    {
        // Arrange
        LlmRequest? request = null;

        // Act
        request = new LlmRequest(AnyValidPrompt);

        // Assert
        using (new AssertionScope())
        {
            request.Should().NotBeNull();
            request!.Prompt.Should().Be(AnyValidPrompt);
            request.SystemPrompt.Should().BeNull();
            request.Temperature.Should().BeNull();
            request.MaxTokens.Should().BeNull();
        }
    }

    [Fact]
    public void Should_SetProperties_When_ConstructedWithAllInputs()
    {
        // Arrange
        LlmRequest? request = null;

        // Act
        request = new LlmRequest(
            AlternateValidPrompt,
            AnyValidSystemPrompt,
            AnyValidTemperature,
            AnyValidMaxTokens);

        // Assert
        using (new AssertionScope())
        {
            request.Should().NotBeNull();
            request!.Prompt.Should().Be(AlternateValidPrompt);
            request.SystemPrompt.Should().Be(AnyValidSystemPrompt);
            request.Temperature.Should().Be(AnyValidTemperature);
            request.MaxTokens.Should().Be(AnyValidMaxTokens);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Should_ThrowArgumentException_When_PromptIsNullOrWhiteSpace(string? prompt)
    {
        // Arrange

        // Act
        Action act = () => new LlmRequest(prompt!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be(ToParameterName(nameof(LlmRequest.Prompt)));
    }

    [Theory]
    [InlineData(0.0)]
    public void Should_SetTemperature_When_TemperatureIsValid(double temperature)
    {
        // Arrange
        LlmRequest? request = null;

        // Act
        request = new LlmRequest(
            AnyValidPrompt,
            AnyValidSystemPrompt,
            temperature,
            AnyValidMaxTokens);

        // Assert
        using (new AssertionScope())
        {
            request.Should().NotBeNull();
            request!.Temperature.Should().Be(temperature);
        }
    }

    [Theory]
    [InlineData(-0.1)]
    public void Should_ThrowArgumentOutOfRangeException_When_TemperatureIsNegative(double temperature)
    {
        // Arrange

        // Act
        Action act = () => new LlmRequest(
            AnyValidPrompt,
            AnyValidSystemPrompt,
            temperature,
            AnyValidMaxTokens);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .Which.ParamName.Should().Be(ToParameterName(nameof(LlmRequest.Temperature)));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void Should_SetMaxTokens_When_MaxTokensIsPositive(int maxTokens)
    {
        // Arrange
        LlmRequest? request = null;

        // Act
        request = new LlmRequest(
            AnyValidPrompt,
            AnyValidSystemPrompt,
            AlternateValidTemperature,
            maxTokens);

        // Assert
        using (new AssertionScope())
        {
            request.Should().NotBeNull();
            request!.MaxTokens.Should().Be(maxTokens);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_ThrowArgumentOutOfRangeException_When_MaxTokensIsNotPositive(int maxTokens)
    {
        // Arrange

        // Act
        Action act = () => new LlmRequest(
            AnyValidPrompt,
            AnyValidSystemPrompt,
            AlternateValidTemperature,
            maxTokens);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .Which.ParamName.Should().Be(ToParameterName(nameof(LlmRequest.MaxTokens)));
    }

    [Fact]
    public void Should_BeEqual_When_InstancesHaveIdenticalValues()
    {
        // Arrange
        var first = new LlmRequest(
            AlternateValidPrompt,
            AnyValidSystemPrompt,
            AlternateValidTemperature,
            AnyValidMaxTokens);
        var second = new LlmRequest(
            AlternateValidPrompt,
            AnyValidSystemPrompt,
            AlternateValidTemperature,
            AnyValidMaxTokens);

        // Act
        var actual = first;

        // Assert
        using (new AssertionScope())
        {
            actual.Should().Be(second);
        }
    }

    [Fact]
    public void Should_NotBeEqual_When_PromptDiffers()
    {
        // Arrange
        var first = new LlmRequest(AnyValidPrompt);
        var second = new LlmRequest(DifferentValidPrompt);

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
