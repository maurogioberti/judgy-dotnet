namespace Judgy.Evaluation;

public record EvaluationResult
{
    private const string ActualOutputNullOrWhiteSpaceMessage = "ActualOutput cannot be null or whitespace.";
    private const string ExpectationNullOrWhiteSpaceMessage = "Expectation cannot be null or whitespace.";
    private const string EvaluationDurationNegativeMessage = "EvaluationDuration cannot be negative.";

    public EvaluationEvidence Evidence { get; }
    public string ActualOutput { get; }
    public string Expectation { get; }
    public TimeSpan EvaluationDuration { get; }

    public EvaluationResult(EvaluationEvidence evidence, string actualOutput, string expectation, TimeSpan evaluationDuration)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        ValidateRequiredText(actualOutput, nameof(actualOutput), ActualOutputNullOrWhiteSpaceMessage);
        ValidateRequiredText(expectation, nameof(expectation), ExpectationNullOrWhiteSpaceMessage);
        ValidateEvaluationDuration(evaluationDuration);

        Evidence = evidence;
        ActualOutput = actualOutput;
        Expectation = expectation;
        EvaluationDuration = evaluationDuration;
    }

    private static void ValidateRequiredText(string value, string paramName, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(message, paramName);
    }

    private static void ValidateEvaluationDuration(TimeSpan evaluationDuration)
    {
        if (evaluationDuration < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(evaluationDuration), evaluationDuration, EvaluationDurationNegativeMessage);
    }
}
