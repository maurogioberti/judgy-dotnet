using Judgy.Assertions;
using Judgy.Providers;
using Xunit;

namespace Judgy.Samples.SemanticEvaluation;

public sealed class SemanticAssertionPolicyTests : JudgeTestBase
{
    [Fact]
    public async Task EvaluatingRagAnswer_PassesSemanticPolicy()
    {
        // Arrange
        using var source = CreateSourceProvider();
        var evaluator = CreateJudgeEvaluator();

        // Act - simulate calling your RAG system via HTTP
        var response = await source.CompleteAsync(new LlmRequest("What is the capital of France?"), CancellationToken);
        var result = await evaluator.EvaluateAsync(response.Text, "The answer identifies Paris as the capital of France", CancellationToken);
        var decision = SemanticAssertionPolicy.Evaluate(result, new SemanticAssertionOptions(0.70));

        // Assert
        if (!decision.Succeeded)
            throw new InvalidOperationException(AssertionFailureMessageFormatter.Format(decision.Failure!));
    }
}
