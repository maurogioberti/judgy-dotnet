using System.Text;

namespace Judgy.Assertions;

public static class AssertionFailureMessageFormatter
{
    public static string Format(AssertionFailureDetails failure)
    {
        ArgumentNullException.ThrowIfNull(failure);

        return failure.Kind switch
        {
            AssertionFailureKind.SemanticExpectation => FormatSemantic(failure),
            AssertionFailureKind.Score => FormatScore(failure),
            AssertionFailureKind.Duration => FormatDuration(failure),
            _ => throw new ArgumentOutOfRangeException(nameof(failure), failure.Kind, "Unknown failure kind.")
        };
    }

    private static string FormatSemantic(AssertionFailureDetails failure)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Judgy semantic assertion failed.");
        sb.AppendLine();
        sb.AppendLine($"  Expectation  : {failure.Expectation}");
        sb.AppendLine($"  Actual       : {failure.ActualOutput}");
        sb.AppendLine($"  Score        : {failure.ActualScore!.Value.ToString("F2")}");
        sb.AppendLine($"  MinimumScore : {failure.MinimumScore!.Value.ToString("F2")}");
        sb.Append($"  Reasoning    : {failure.Reasoning}");
        return sb.ToString();
    }

    private static string FormatScore(AssertionFailureDetails failure)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Judgy score assertion failed.");
        sb.AppendLine();
        sb.AppendLine($"  Score        : {failure.ActualScore!.Value.ToString("F2")}");
        sb.Append($"  MinimumScore : {failure.MinimumScore!.Value.ToString("F2")}");
        return sb.ToString();
    }

    private static string FormatDuration(AssertionFailureDetails failure)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Judgy duration assertion failed.");
        sb.AppendLine();
        sb.AppendLine($"  Duration        : {failure.ActualDuration!.Value}");
        sb.Append($"  MaximumDuration : {failure.MaximumDuration!.Value}");
        return sb.ToString();
    }
}
