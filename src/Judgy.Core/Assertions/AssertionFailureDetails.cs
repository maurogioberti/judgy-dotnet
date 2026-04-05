namespace Judgy.Assertions;

public sealed record AssertionFailureDetails
{
    public AssertionFailureKind Kind { get; }
    public string? Expectation { get; }
    public string? ActualOutput { get; }
    public double? ActualScore { get; }
    public double? MinimumScore { get; }
    public TimeSpan? ActualDuration { get; }
    public TimeSpan? MaximumDuration { get; }
    public string? Reasoning { get; }

    internal AssertionFailureDetails(
        AssertionFailureKind kind,
        string? expectation,
        string? actualOutput,
        double? actualScore,
        double? minimumScore,
        TimeSpan? actualDuration,
        TimeSpan? maximumDuration,
        string? reasoning)
    {
        Kind = kind;
        Expectation = expectation;
        ActualOutput = actualOutput;
        ActualScore = actualScore;
        MinimumScore = minimumScore;
        ActualDuration = actualDuration;
        MaximumDuration = maximumDuration;
        Reasoning = reasoning;
    }

    internal static AssertionFailureDetails ForSemantic(
        string expectation,
        string actualOutput,
        double actualScore,
        double minimumScore,
        string reasoning)
    {
        return new AssertionFailureDetails(
            AssertionFailureKind.SemanticExpectation,
            expectation,
            actualOutput,
            actualScore,
            minimumScore,
            actualDuration: null,
            maximumDuration: null,
            reasoning);
    }

    internal static AssertionFailureDetails ForScore(
        double actualScore,
        double minimumScore)
    {
        return new AssertionFailureDetails(
            AssertionFailureKind.Score,
            expectation: null,
            actualOutput: null,
            actualScore,
            minimumScore,
            actualDuration: null,
            maximumDuration: null,
            reasoning: null);
    }

    internal static AssertionFailureDetails ForDuration(
        TimeSpan actualDuration,
        TimeSpan maximumDuration)
    {
        return new AssertionFailureDetails(
            AssertionFailureKind.Duration,
            expectation: null,
            actualOutput: null,
            actualScore: null,
            minimumScore: null,
            actualDuration,
            maximumDuration,
            reasoning: null);
    }
}
