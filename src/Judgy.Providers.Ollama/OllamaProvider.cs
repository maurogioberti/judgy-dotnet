using System.Net.Http.Headers;
using System.Net.Http.Json;
using Judgy.Providers;

namespace Judgy.Providers.Ollama;

public sealed class OllamaProvider : ILlmProvider, IDisposable
{
    private const string ProviderName = "Ollama";
    private const string ChatEndpointPath = "api/chat";

    private readonly HttpClient _httpClient;
    private readonly OllamaProviderOptions _options;

    public OllamaProvider(OllamaProviderOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.BaseUrl))
            throw new ArgumentException("BaseUrl cannot be null or whitespace.", nameof(options));

        if (string.IsNullOrWhiteSpace(options.Model))
            throw new ArgumentException("Model cannot be null or whitespace.", nameof(options));

        _options = options;
        _httpClient = CreateHttpClient(options);
    }

    public async Task<LlmResponse> CompleteAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var messages = BuildMessages(request);
        var chatRequest = new OllamaChatRequest(_options.Model, Stream: false, messages);

        using var response = await _httpClient.PostAsJsonAsync(ChatEndpointPath, chatRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var chatResponse = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Ollama returned an empty response payload.");

        var text = chatResponse.Message?.Content
            ?? throw new InvalidOperationException("Ollama response did not contain message content.");

        return new LlmResponse(text, ProviderName, chatResponse.PromptEvalCount, chatResponse.EvalCount);
    }

    public void Dispose() => _httpClient.Dispose();

    private static OllamaChatMessage[] BuildMessages(LlmRequest request)
    {
        if (request.SystemPrompt is not null)
        {
            return
            [
                new OllamaChatMessage("system", request.SystemPrompt),
                new OllamaChatMessage("user", request.Prompt)
            ];
        }

        return [new OllamaChatMessage("user", request.Prompt)];
    }

    private static HttpClient CreateHttpClient(OllamaProviderOptions options)
    {
        var handler = options.HttpMessageHandler ?? new HttpClientHandler();
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute),
            Timeout = options.Timeout
        };

        if (!string.IsNullOrWhiteSpace(options.ApiKey))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);

        return client;
    }
}
