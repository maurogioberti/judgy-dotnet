namespace Judgy.Evaluation;

/// <summary>
/// Represents the evidence produced by an evaluator for a single evaluation.
/// </summary>
public record EvaluationEvidence
{
    private const string ConfidenceOutOfRangeMessage = "Confidence must be within the supported inclusive range.";
    private const string ReasoningNullOrWhiteSpaceMessage = "Reasoning cannot be null or whitespace.";
    private const string EvaluatorNameNullOrWhiteSpaceMessage = "EvaluatorName cannot be null or whitespace.";

    /// <summary>
    /// The minimum supported confidence value.
    /// </summary>
    public const double MinimumConfidence = 0.0;

    /// <summary>
    /// The maximum supported confidence value.
    /// </summary>
    public const double MaximumConfidence = 1.0;

    /// <summary>
    /// Gets the evaluator confidence score.
    /// </summary>
    public double Confidence { get; }

    /// <summary>
    /// Gets the evaluator reasoning associated with <see cref="Confidence"/>.
    /// </summary>
    public string Reasoning { get; }

    /// <summary>
    /// Gets the evaluator name that produced this evidence.
    /// </summary>
    public string EvaluatorName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EvaluationEvidence"/> record.
    /// </summary>
    /// <param name="confidence">
    /// The evaluator confidence score. Must be between <see cref="MinimumConfidence"/> and <see cref="MaximumConfidence"/> inclusive.
    /// </param>
    /// <param name="reasoning">The evaluator reasoning.</param>
    /// <param name="evaluatorName">The evaluator name.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="confidence"/> is not within <see cref="MinimumConfidence"/> and <see cref="MaximumConfidence"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="reasoning"/> or <paramref name="evaluatorName"/> is null, empty, or whitespace.
    /// </exception>
    public EvaluationEvidence(double confidence, string reasoning, string evaluatorName)
    {
        ValidateConfidence(confidence);
        ValidateRequiredText(reasoning, nameof(reasoning), ReasoningNullOrWhiteSpaceMessage);
        ValidateRequiredText(evaluatorName, nameof(evaluatorName), EvaluatorNameNullOrWhiteSpaceMessage);

        Confidence = confidence;
        Reasoning = reasoning;
        EvaluatorName = evaluatorName;
    }

    private static void ValidateConfidence(double confidence)
    {
        if (double.IsNaN(confidence) || double.IsInfinity(confidence) || confidence < MinimumConfidence || confidence > MaximumConfidence)
            throw new ArgumentOutOfRangeException(nameof(confidence), confidence, ConfidenceOutOfRangeMessage);
    }

    private static void ValidateRequiredText(string value, string paramName, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(message, paramName);
    }
}
