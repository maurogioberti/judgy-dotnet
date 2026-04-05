namespace Judgy.Assertions;

public record SemanticAssertionOptions
{
    private const string MinimumScoreOutOfRangeMessage = "MinimumScore must be between 0.0 and 1.0 inclusive.";

    private const double MinAllowedScore = 0.0;
    private const double MaxAllowedScore = 1.0;

    public const double DefaultMinimumScore = 0.70;

    public double MinimumScore { get; }

    public SemanticAssertionOptions(double minimumScore = DefaultMinimumScore)
    {
        ValidateMinimumScore(minimumScore);
        MinimumScore = minimumScore;
    }

    private static void ValidateMinimumScore(double minimumScore)
    {
        if (double.IsNaN(minimumScore) || double.IsInfinity(minimumScore) || minimumScore < MinAllowedScore || minimumScore > MaxAllowedScore)
            throw new ArgumentOutOfRangeException(nameof(minimumScore), minimumScore, MinimumScoreOutOfRangeMessage);
    }
}
