namespace Judgy.Providers;

public record LlmRequest
{
    private const double MinimumTemperature = 0.0;
    private const int MinimumMaxTokens = 1;
    private const string PromptNullOrWhiteSpaceMessage = "Prompt cannot be null or whitespace.";
    private const string TemperatureNegativeMessage = "Temperature cannot be negative.";
    private const string MaxTokensOutOfRangeMessage = "MaxTokens must be greater than zero.";

    public string Prompt { get; }
    public string? SystemPrompt { get; }
    public double? Temperature { get; }
    public int? MaxTokens { get; }

    public LlmRequest(string prompt, string? systemPrompt = null, double? temperature = null, int? maxTokens = null)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException(PromptNullOrWhiteSpaceMessage, nameof(prompt));

        if (temperature is < MinimumTemperature)
            throw new ArgumentOutOfRangeException(nameof(temperature), temperature, TemperatureNegativeMessage);

        if (maxTokens is < MinimumMaxTokens)
            throw new ArgumentOutOfRangeException(nameof(maxTokens), maxTokens, MaxTokensOutOfRangeMessage);

        Prompt = prompt;
        SystemPrompt = systemPrompt;
        Temperature = temperature;
        MaxTokens = maxTokens;
    }
}
