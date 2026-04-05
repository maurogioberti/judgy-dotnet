namespace Judgy.Assertions;

public sealed record AssertionDecision
{
    public bool Succeeded { get; }
    public AssertionFailureDetails? Failure { get; }

    private AssertionDecision(bool succeeded, AssertionFailureDetails? failure)
    {
        Succeeded = succeeded;
        Failure = failure;
    }

    public static AssertionDecision Pass()
    {
        return new AssertionDecision(true, null);
    }

    public static AssertionDecision Fail(AssertionFailureDetails failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return new AssertionDecision(false, failure);
    }
}
