using System.Net.Http.Headers;
using System.Net.Http.Json;
using Judgy.Providers;

namespace Judgy.Providers.DeepSeek;

public sealed class DeepSeekProvider : ILlmProvider, IDisposable
{
    private const string ProviderName = "DeepSeek";
    private const string ChatCompletionsPath = "chat/completions";

    private readonly HttpClient _httpClient;
    private readonly DeepSeekProviderOptions _options;

    public DeepSeekProvider(DeepSeekProviderOptions options)
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

        var chatRequest = new DeepSeekChatRequest(_options.Model, messages, temperature, maxTokens);

        using var response = await _httpClient.PostAsJsonAsync(ChatCompletionsPath, chatRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var chatResponse = await response.Content.ReadFromJsonAsync<DeepSeekChatResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("DeepSeek returned an empty response payload.");

        var text = chatResponse.Choices?.FirstOrDefault()?.Message?.Content
            ?? throw new InvalidOperationException("DeepSeek response did not contain message content.");

        return new LlmResponse(
            text,
            ProviderName,
            chatResponse.Usage?.PromptTokens,
            chatResponse.Usage?.CompletionTokens);
    }

    public void Dispose() => _httpClient.Dispose();

    private static DeepSeekChatMessage[] BuildMessages(LlmRequest request)
    {
        if (request.SystemPrompt is not null)
        {
            return
            [
                new DeepSeekChatMessage("system", request.SystemPrompt),
                new DeepSeekChatMessage("user", request.Prompt)
            ];
        }

        return [new DeepSeekChatMessage("user", request.Prompt)];
    }

    private static HttpClient CreateHttpClient(DeepSeekProviderOptions options)
    {
        var handler = options.HttpMessageHandler ?? new HttpClientHandler();
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.deepseek.com/"),
            Timeout = options.Timeout
        };

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);

        return client;
    }
}
