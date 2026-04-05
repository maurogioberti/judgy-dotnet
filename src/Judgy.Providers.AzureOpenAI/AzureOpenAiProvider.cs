using System.Net.Http.Json;
using Judgy.Providers;

namespace Judgy.Providers.AzureOpenAI;

public sealed class AzureOpenAiProvider : ILlmProvider, IDisposable
{
    private const string ProviderName = "AzureOpenAI";

    private readonly HttpClient _httpClient;
    private readonly AzureOpenAiProviderOptions _options;

    public AzureOpenAiProvider(AzureOpenAiProviderOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.Endpoint))
            throw new ArgumentException("Endpoint cannot be null or whitespace.", nameof(options));

        if (string.IsNullOrWhiteSpace(options.ApiKey))
            throw new ArgumentException("ApiKey cannot be null or whitespace.", nameof(options));

        if (string.IsNullOrWhiteSpace(options.DeploymentName))
            throw new ArgumentException("DeploymentName cannot be null or whitespace.", nameof(options));

        if (string.IsNullOrWhiteSpace(options.ApiVersion))
            throw new ArgumentException("ApiVersion cannot be null or whitespace.", nameof(options));

        _options = options;
        _httpClient = CreateHttpClient(options);
    }

    public async Task<LlmResponse> CompleteAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var messages = BuildMessages(request);
        var temperature = request.Temperature ?? _options.Temperature;
        var maxTokens = request.MaxTokens ?? _options.MaxTokens;
        var path = $"openai/deployments/{Uri.EscapeDataString(_options.DeploymentName)}/chat/completions?api-version={Uri.EscapeDataString(_options.ApiVersion)}";

        var chatRequest = new AzureOpenAiChatRequest(messages, temperature, maxTokens);

        using var response = await _httpClient.PostAsJsonAsync(path, chatRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var chatResponse = await response.Content.ReadFromJsonAsync<AzureOpenAiChatResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Azure OpenAI returned an empty response payload.");

        var text = chatResponse.Choices?.FirstOrDefault()?.Message?.Content
            ?? throw new InvalidOperationException("Azure OpenAI response did not contain message content.");

        return new LlmResponse(
            text,
            ProviderName,
            chatResponse.Usage?.PromptTokens,
            chatResponse.Usage?.CompletionTokens);
    }

    public void Dispose() => _httpClient.Dispose();

    private static AzureOpenAiChatMessage[] BuildMessages(LlmRequest request)
    {
        if (request.SystemPrompt is not null)
        {
            return
            [
                new AzureOpenAiChatMessage("system", request.SystemPrompt),
                new AzureOpenAiChatMessage("user", request.Prompt)
            ];
        }

        return [new AzureOpenAiChatMessage("user", request.Prompt)];
    }

    private static HttpClient CreateHttpClient(AzureOpenAiProviderOptions options)
    {
        var handler = options.HttpMessageHandler ?? new HttpClientHandler();
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri(options.Endpoint, UriKind.Absolute),
            Timeout = options.Timeout
        };

        client.DefaultRequestHeaders.TryAddWithoutValidation("api-key", options.ApiKey);

        return client;
    }
}
