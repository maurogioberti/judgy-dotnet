using FluentAssertions;
using FluentAssertions.Execution;
using Judgy.Providers;
using Xunit;

namespace Judgy.Core.Tests.Providers;

public class LlmResponseTests
{
    private const string AnyValidText = "Hello world";
    private const string AlternateValidText = "Hello";
    private const string DifferentValidText = "World";
    private const string AnyValidProviderName = "OpenAI";
    private const int AnyValidPromptTokens = 10;
    private const int AnyValidCompletionTokens = 20;

    [Fact]
    public void Should_SetDefaults_When_ConstructedWithRequiredInputs()
    {
        // Arrange
        LlmResponse? response = null;

        // Act
        response = new LlmResponse(AnyValidText, AnyValidProviderName);

        // Assert
        using (new AssertionScope())
        {
            response.Should().NotBeNull();
            response!.Text.Should().Be(AnyValidText);
            response.ProviderName.Should().Be(AnyValidProviderName);
            response.PromptTokens.Should().BeNull();
            response.CompletionTokens.Should().BeNull();
        }
    }

    [Fact]
    public void Should_SetProperties_When_ConstructedWithAllInputs()
    {
        // Arrange
        LlmResponse? response = null;

        // Act
        response = new LlmResponse(
            AlternateValidText,
            AnyValidProviderName,
            AnyValidPromptTokens,
            AnyValidCompletionTokens);

        // Assert
        using (new AssertionScope())
        {
            response.Should().NotBeNull();
            response!.Text.Should().Be(AlternateValidText);
            response.ProviderName.Should().Be(AnyValidProviderName);
            response.PromptTokens.Should().Be(AnyValidPromptTokens);
            response.CompletionTokens.Should().Be(AnyValidCompletionTokens);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Should_ThrowArgumentException_When_TextIsNullOrWhiteSpace(string? text)
    {
        // Arrange

        // Act
        Action act = () => new LlmResponse(text!, AnyValidProviderName);

        // Assert
        act.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be(ToParameterName(nameof(LlmResponse.Text)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Should_ThrowArgumentException_When_ProviderNameIsNullOrWhiteSpace(string? providerName)
    {
        // Arrange

        // Act
        Action act = () => new LlmResponse(AnyValidText, providerName!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be(ToParameterName(nameof(LlmResponse.ProviderName)));
    }

    [Fact]
    public void Should_BeEqual_When_InstancesHaveIdenticalValues()
    {
        // Arrange
        var first = new LlmResponse(
            AlternateValidText,
            AnyValidProviderName,
            AnyValidPromptTokens,
            AnyValidCompletionTokens);
        var second = new LlmResponse(
            AlternateValidText,
            AnyValidProviderName,
            AnyValidPromptTokens,
            AnyValidCompletionTokens);

        // Act
        var actual = first;

        // Assert
        using (new AssertionScope())
        {
            actual.Should().Be(second);
        }
    }

    [Fact]
    public void Should_NotBeEqual_When_TextDiffers()
    {
        // Arrange
        var first = new LlmResponse(AnyValidText, AnyValidProviderName);
        var second = new LlmResponse(DifferentValidText, AnyValidProviderName);

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
