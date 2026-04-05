namespace Judgy.Evaluation;

public interface ISemanticEvaluator
{
    Task<EvaluationResult> EvaluateAsync(string actualOutput, string expectation, CancellationToken cancellationToken = default);
}
