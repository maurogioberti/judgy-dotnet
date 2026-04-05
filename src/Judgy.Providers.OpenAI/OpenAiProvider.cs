using System.Net.Http.Headers;
using System.Net.Http.Json;
using Judgy.Providers;

namespace Judgy.Providers.OpenAI;

public sealed class OpenAiProvider : ILlmProvider, IDisposable
{
    private const string ProviderName = "OpenAI";
    private const string ChatCompletionsPath = "v1/chat/completions";

    private readonly HttpClient _httpClient;
    private readonly OpenAiProviderOptions _options;

    public OpenAiProvider(OpenAiProviderOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.ApiKey))
            throw new ArgumentException("ApiKey cannot be null or whitespace.", nameof(options));

        _options = options;
        _httpClient = CreateHttpClient(options);
    }

    public async Task<LlmResponse> CompleteAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var messages = BuildMessages(request);
        var temperature = request.Temperature ?? _options.Temperature;
        var maxTokens = request.MaxTokens ?? _options.MaxTokens;

        var chatRequest = new OpenAiChatRequest(_options.Model, messages, temperature, maxTokens);

        using var response = await _httpClient.PostAsJsonAsync(ChatCompletionsPath, chatRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var chatResponse = await response.Content.ReadFromJsonAsync<OpenAiChatResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("OpenAI returned an empty response payload.");

        var text = chatResponse.Choices?.FirstOrDefault()?.Message?.Content
            ?? throw new InvalidOperationException("OpenAI response did not contain message content.");

        return new LlmResponse(
            text,
            ProviderName,
            chatResponse.Usage?.PromptTokens,
            chatResponse.Usage?.CompletionTokens);
    }

    public void Dispose() => _httpClient.Dispose();

    private static OpenAiChatMessage[] BuildMessages(LlmRequest request)
    {
        if (request.SystemPrompt is not null)
        {
            return
            [
                new OpenAiChatMessage("system", request.SystemPrompt),
                new OpenAiChatMessage("user", request.Prompt)
            ];
        }

        return [new OpenAiChatMessage("user", request.Prompt)];
    }

    private static HttpClient CreateHttpClient(OpenAiProviderOptions options)
    {
        var handler = options.HttpMessageHandler ?? new HttpClientHandler();
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.openai.com/"),
            Timeout = options.Timeout
        };

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);

        return client;
    }
}
