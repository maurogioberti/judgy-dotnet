namespace Judgy.Providers;

public record LlmResponse
{
    private const string TextNullOrWhiteSpaceMessage = "Text cannot be null or whitespace.";
    private const string ProviderNameNullOrWhiteSpaceMessage = "ProviderName cannot be null or whitespace.";

    public string Text { get; }
    public string ProviderName { get; }
    public int? PromptTokens { get; }
    public int? CompletionTokens { get; }

    public LlmResponse(string text, string providerName, int? promptTokens = null, int? completionTokens = null)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException(TextNullOrWhiteSpaceMessage, nameof(text));

        if (string.IsNullOrWhiteSpace(providerName))
            throw new ArgumentException(ProviderNameNullOrWhiteSpaceMessage, nameof(providerName));

        Text = text;
        ProviderName = providerName;
        PromptTokens = promptTokens;
        CompletionTokens = completionTokens;
    }
}
