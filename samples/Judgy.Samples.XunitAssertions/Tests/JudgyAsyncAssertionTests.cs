using Xunit;

namespace Judgy.Samples.XunitAssertions;

public sealed class JudgyAsyncAssertionTests : JudgeTestBase
{
    [Fact]
    public async Task EvaluatingValidAnswer_PassesJudgyAsync()
    {
        // Arrange
        const string answer = "The capital of France is Paris.";
        const string expectation = "The answer identifies Paris as the capital of France";
        var evaluator = CreateJudgeEvaluator();

        // Act & Assert
        await Assert.JudgyAsync(answer, expectation, evaluator, cancellationToken: CancellationToken);
    }
}
