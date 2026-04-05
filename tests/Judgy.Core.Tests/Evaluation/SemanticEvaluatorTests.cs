using FluentAssertions;
using FluentAssertions.Execution;
using Judgy.Evaluation;
using Judgy.Providers;
using Xunit;

namespace Judgy.Core.Tests.Evaluation;

public class SemanticEvaluatorTests
{
    private const string AnyActualOutput = "Hello, how can I help you?";
    private const string AnyExpectation = "A polite greeting";
    private const string ExpectedSystemPrompt = """
        You are an evaluation judge. Your job is to assess whether an actual output meets a given expectation.

        You must respond with JSON only. Do not include any other text, explanation, or markdown formatting.

        Use this exact JSON format:
        { "confidence": <number between 0.0 and 1.0>, "reasoning": "<your explanation>" }

        Confidence scale:
        - 1.0 = the actual output fully meets the expectation
        - 0.0 = the actual output does not meet the expectation at all
        - Values in between represent partial matches

        Respond with the JSON object only.
        """;

    [Fact]
    public void Should_ThrowArgumentNullException_When_ProviderIsNull()
    {
        // Arrange

        // Act
        Action act = () => new SemanticEvaluator(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Should_ThrowArgumentException_When_ActualOutputIsNullOrWhiteSpace(string? actualOutput)
    {
        // Arrange
        var provider = new FakeLlmProvider("""{"confidence": 0.5, "reasoning": "test"}""");
        var evaluator = new SemanticEvaluator(provider);

        // Act
        Func<Task> act = async () => await evaluator.EvaluateAsync(actualOutput!, AnyExpectation, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .Where(ex => ex.ParamName == "actualOutput");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Should_ThrowArgumentException_When_ExpectationIsNullOrWhiteSpace(string? expectation)
    {
        // Arrange
        var provider = new FakeLlmProvider("""{"confidence": 0.5, "reasoning": "test"}""");
        var evaluator = new SemanticEvaluator(provider);

        // Act
        Func<Task> act = async () => await evaluator.EvaluateAsync(AnyActualOutput, expectation!, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .Where(ex => ex.ParamName == "expectation");
    }

    [Fact]
    public async Task Should_ReturnValidEvidence_When_ProviderReturnsValidJson()
    {
        // Arrange
        var provider = new FakeLlmProvider("""{"confidence": 0.85, "reasoning": "Good match"}""");
        var evaluator = new SemanticEvaluator(provider);

        // Act
        var result = await evaluator.EvaluateAsync(AnyActualOutput, AnyExpectation, TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            result.Evidence.Confidence.Should().Be(0.85);
            result.Evidence.Reasoning.Should().Be("Good match");
            result.Evidence.EvaluatorName.Should().Be(SemanticEvaluator.EvaluatorName);
            result.ActualOutput.Should().Be(AnyActualOutput);
            result.Expectation.Should().Be(AnyExpectation);
            result.EvaluationDuration.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
        }
    }

    [Fact]
    public async Task Should_ReturnConfidenceZero_When_ProviderReturnsConfidenceAtLowerBound()
    {
        // Arrange
        var provider = new FakeLlmProvider("""{"confidence": 0.0, "reasoning": "No match"}""");
        var evaluator = new SemanticEvaluator(provider);

        // Act
        var result = await evaluator.EvaluateAsync(AnyActualOutput, AnyExpectation, TestContext.Current.CancellationToken);

        // Assert
        result.Evidence.Confidence.Should().Be(0.0);
    }

    [Fact]
    public async Task Should_ReturnConfidenceOne_When_ProviderReturnsConfidenceAtUpperBound()
    {
        // Arrange
        var provider = new FakeLlmProvider("""{"confidence": 1.0, "reasoning": "Perfect match"}""");
        var evaluator = new SemanticEvaluator(provider);

        // Act
        var result = await evaluator.EvaluateAsync(AnyActualOutput, AnyExpectation, TestContext.Current.CancellationToken);

        // Assert
        result.Evidence.Confidence.Should().Be(1.0);
    }

    [Fact]
    public async Task Should_ClampToOne_When_ProviderReturnsConfidenceAboveOne()
    {
        // Arrange
        var provider = new FakeLlmProvider("""{"confidence": 1.5, "reasoning": "Very confident"}""");
        var evaluator = new SemanticEvaluator(provider);

        // Act
        var result = await evaluator.EvaluateAsync(AnyActualOutput, AnyExpectation, TestContext.Current.CancellationToken);

        // Assert
        result.Evidence.Confidence.Should().Be(1.0);
    }

    [Fact]
    public async Task Should_ClampToZero_When_ProviderReturnsNegativeConfidence()
    {
        // Arrange
        var provider = new FakeLlmProvider("""{"confidence": -0.3, "reasoning": "Negative"}""");
        var evaluator = new SemanticEvaluator(provider);

        // Act
        var result = await evaluator.EvaluateAsync(AnyActualOutput, AnyExpectation, TestContext.Current.CancellationToken);

        // Assert
        result.Evidence.Confidence.Should().Be(0.0);
    }

    [Fact]
    public async Task Should_ReturnDegradedEvidence_When_ProviderReturnsPlainText()
    {
        // Arrange
        var provider = new FakeLlmProvider("This is not JSON at all");
        var evaluator = new SemanticEvaluator(provider);

        // Act
        var result = await evaluator.EvaluateAsync(AnyActualOutput, AnyExpectation, TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            result.Evidence.Confidence.Should().Be(0.0);
            result.Evidence.Reasoning.Should().NotBeNullOrWhiteSpace();
            result.Evidence.EvaluatorName.Should().Be(SemanticEvaluator.EvaluatorName);
        }
    }

    [Fact]
    public async Task Should_ReturnDegradedEvidence_When_ProviderReturnsJsonMissingConfidence()
    {
        // Arrange
        var provider = new FakeLlmProvider("""{"reasoning": "some reasoning"}""");
        var evaluator = new SemanticEvaluator(provider);

        // Act
        var result = await evaluator.EvaluateAsync(AnyActualOutput, AnyExpectation, TestContext.Current.CancellationToken);

        // Assert
        result.Evidence.Confidence.Should().Be(0.0);
    }

    [Fact]
    public async Task Should_UseFallbackReasoning_When_ProviderReturnsJsonWithEmptyReasoning()
    {
        // Arrange
        var provider = new FakeLlmProvider("""{"confidence": 0.7, "reasoning": ""}""");
        var evaluator = new SemanticEvaluator(provider);

        // Act
        var result = await evaluator.EvaluateAsync(AnyActualOutput, AnyExpectation, TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            result.Evidence.Confidence.Should().Be(0.7);
            result.Evidence.Reasoning.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public async Task Should_ParseCorrectly_When_ProviderReturnsJsonWrappedInMarkdownFences()
    {
        // Arrange
        var json = "```json\n{\"confidence\": 0.9, \"reasoning\": \"Great match\"}\n```";
        var provider = new FakeLlmProvider(json);
        var evaluator = new SemanticEvaluator(provider);

        // Act
        var result = await evaluator.EvaluateAsync(AnyActualOutput, AnyExpectation, TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            result.Evidence.Confidence.Should().Be(0.9);
            result.Evidence.Reasoning.Should().Be("Great match");
        }
    }

    [Fact]
    public async Task Should_ParseCorrectly_When_ProviderReturnsJsonWithoutLanguageTag()
    {
        // Arrange
        var json = "```\n{\"confidence\": 0.75, \"reasoning\": \"Decent match\"}\n```";
        var provider = new FakeLlmProvider(json);
        var evaluator = new SemanticEvaluator(provider);

        // Act
        var result = await evaluator.EvaluateAsync(AnyActualOutput, AnyExpectation, TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            result.Evidence.Confidence.Should().Be(0.75);
            result.Evidence.Reasoning.Should().Be("Decent match");
        }
    }

    [Fact]
    public async Task Should_ForwardCancellationToken_When_CallingProvider()
    {
        // Arrange
        var provider = new FakeLlmProvider("""{"confidence": 0.5, "reasoning": "test"}""");
        var evaluator = new SemanticEvaluator(provider);

        // Act
        await evaluator.EvaluateAsync(AnyActualOutput, AnyExpectation, TestContext.Current.CancellationToken);

        // Assert
        provider.LastCancellationToken.Should().Be(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Should_SetTemperatureToZero_When_CreatingLlmRequest()
    {
        // Arrange
        var provider = new FakeLlmProvider("""{"confidence": 0.5, "reasoning": "test"}""");
        var evaluator = new SemanticEvaluator(provider);

        // Act
        await evaluator.EvaluateAsync(AnyActualOutput, AnyExpectation, TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            provider.LastRequest.Should().NotBeNull();
            provider.LastRequest!.Temperature.Should().Be(0.0);
            provider.LastRequest.Prompt.Should().NotBeNullOrWhiteSpace();
            provider.LastRequest.SystemPrompt.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public async Task Should_CreateExpectedPrompts_When_Evaluating()
    {
        // Arrange
        var provider = new FakeLlmProvider("""{"confidence": 0.5, "reasoning": "test"}""");
        var evaluator = new SemanticEvaluator(provider);
        var expectedUserPrompt = $"""
            Evaluate whether the actual output meets the expectation.

            [Expectation]
            {AnyExpectation}

            [Actual Output]
            {AnyActualOutput}
            """;

        // Act
        await evaluator.EvaluateAsync(AnyActualOutput, AnyExpectation, TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            provider.LastRequest.Should().NotBeNull();
            provider.LastRequest!.SystemPrompt.Should().Be(ExpectedSystemPrompt);
            provider.LastRequest.Prompt.Should().Be(expectedUserPrompt);
        }
    }

    [Fact]
    public async Task Should_AlwaysUseSemanticEvaluatorName_When_ProducingEvidence()
    {
        // Arrange
        var provider = new FakeLlmProvider("""{"confidence": 0.5, "reasoning": "test"}""");
        var evaluator = new SemanticEvaluator(provider);

        // Act
        var result = await evaluator.EvaluateAsync(AnyActualOutput, AnyExpectation, TestContext.Current.CancellationToken);

        // Assert
        result.Evidence.EvaluatorName.Should().Be("SemanticEvaluator");
    }

    [Fact]
    public async Task Should_PopulateEvaluationDuration_When_EvaluationCompletes()
    {
        // Arrange
        var provider = new FakeLlmProvider("""{"confidence": 0.5, "reasoning": "test"}""");
        var evaluator = new SemanticEvaluator(provider);

        // Act
        var result = await evaluator.EvaluateAsync(AnyActualOutput, AnyExpectation, TestContext.Current.CancellationToken);

        // Assert
        result.EvaluationDuration.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
    }

    private sealed class FakeLlmProvider : ILlmProvider
    {
        private readonly string _responseText;

        public LlmRequest? LastRequest { get; private set; }
        public CancellationToken LastCancellationToken { get; private set; }

        public FakeLlmProvider(string responseText)
        {
            _responseText = responseText;
        }

        public Task<LlmResponse> CompleteAsync(LlmRequest request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            LastCancellationToken = cancellationToken;
            return Task.FromResult(new LlmResponse(_responseText, "FakeProvider"));
        }
    }
}
