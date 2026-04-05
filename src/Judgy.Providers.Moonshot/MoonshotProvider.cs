using System.Net.Http.Headers;
using System.Net.Http.Json;
using Judgy.Providers;

namespace Judgy.Providers.Moonshot;

public sealed class MoonshotProvider : ILlmProvider, IDisposable
{
    private const string ProviderName = "Moonshot";
    private const string ChatCompletionsPath = "v1/chat/completions";

    private readonly HttpClient _httpClient;
    private readonly MoonshotProviderOptions _options;

    public MoonshotProvider(MoonshotProviderOptions options)
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

        var chatRequest = new MoonshotChatRequest(_options.Model, messages, temperature, maxTokens);

        using var response = await _httpClient.PostAsJsonAsync(ChatCompletionsPath, chatRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var chatResponse = await response.Content.ReadFromJsonAsync<MoonshotChatResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Moonshot returned an empty response payload.");

        var text = chatResponse.Choices?.FirstOrDefault()?.Message?.Content
            ?? throw new InvalidOperationException("Moonshot response did not contain message content.");

        return new LlmResponse(
            text,
            ProviderName,
            chatResponse.Usage?.PromptTokens,
            chatResponse.Usage?.CompletionTokens);
    }

    public void Dispose() => _httpClient.Dispose();

    private static MoonshotChatMessage[] BuildMessages(LlmRequest request)
    {
        if (request.SystemPrompt is not null)
        {
            return
            [
                new MoonshotChatMessage("system", request.SystemPrompt),
                new MoonshotChatMessage("user", request.Prompt)
            ];
        }

        return [new MoonshotChatMessage("user", request.Prompt)];
    }

    private static HttpClient CreateHttpClient(MoonshotProviderOptions options)
    {
        var handler = options.HttpMessageHandler ?? new HttpClientHandler();
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.moonshot.ai/"),
            Timeout = options.Timeout
        };

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);

        return client;
    }
}
