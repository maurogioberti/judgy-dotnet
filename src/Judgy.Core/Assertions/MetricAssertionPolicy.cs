namespace Judgy.Assertions;

public static class MetricAssertionPolicy
{
    private const string ScoreOutOfRangeMessage = "Score must be between 0.0 and 1.0 inclusive.";
    private const string DurationNegativeMessage = "Duration cannot be negative.";

    private const double MinAllowedScore = 0.0;
    private const double MaxAllowedScore = 1.0;

    public static AssertionDecision EvaluateScore(double actualScore, double minimumScore)
    {
        ValidateScore(actualScore, nameof(actualScore));
        ValidateScore(minimumScore, nameof(minimumScore));

        if (actualScore >= minimumScore)
            return AssertionDecision.Pass();

        var failure = AssertionFailureDetails.ForScore(actualScore, minimumScore);
        return AssertionDecision.Fail(failure);
    }

    public static AssertionDecision EvaluateDuration(TimeSpan actualDuration, TimeSpan maximumDuration)
    {
        ValidateDuration(actualDuration, nameof(actualDuration));
        ValidateDuration(maximumDuration, nameof(maximumDuration));

        if (actualDuration <= maximumDuration)
            return AssertionDecision.Pass();

        var failure = AssertionFailureDetails.ForDuration(actualDuration, maximumDuration);
        return AssertionDecision.Fail(failure);
    }

    private static void ValidateScore(double score, string paramName)
    {
        if (double.IsNaN(score) || double.IsInfinity(score) || score < MinAllowedScore || score > MaxAllowedScore)
            throw new ArgumentOutOfRangeException(paramName, score, ScoreOutOfRangeMessage);
    }

    private static void ValidateDuration(TimeSpan duration, string paramName)
    {
        if (duration < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(paramName, duration, DurationNegativeMessage);
    }
}
