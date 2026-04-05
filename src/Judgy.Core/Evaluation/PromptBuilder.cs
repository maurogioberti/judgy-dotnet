namespace Judgy.Evaluation;

internal static class PromptBuilder
{
    private const string SystemPromptResourceName = "Judgy.Prompts.SemanticEvaluation.system.md";
    private const string UserPromptResourceName = "Judgy.Prompts.SemanticEvaluation.user.md";
    private const string ExpectationPlaceholder = "{{EXPECTATION}}";
    private const string ActualOutputPlaceholder = "{{ACTUAL_OUTPUT}}";

    private static readonly Lazy<string> SystemPromptTemplate = new(() => LoadTemplate(SystemPromptResourceName));
    private static readonly Lazy<string> UserPromptTemplate = new(() => LoadTemplate(UserPromptResourceName));

    internal static string BuildSystemPrompt() => SystemPromptTemplate.Value;

    internal static string BuildUserPrompt(string actualOutput, string expectation) =>
        UserPromptTemplate.Value
            .Replace(ExpectationPlaceholder, expectation)
            .Replace(ActualOutputPlaceholder, actualOutput);

    private static string LoadTemplate(string resourceName)
    {
        using var stream = typeof(PromptBuilder).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded prompt template '{resourceName}' was not found.");
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd().TrimEnd('\r', '\n');
    }
}
