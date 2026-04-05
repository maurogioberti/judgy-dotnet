using System.Net.Http.Json;
using Judgy.Providers;

namespace Judgy.Providers.Anthropic;

public sealed class AnthropicProvider : ILlmProvider, IDisposable
{
    private const string ProviderName = "Anthropic";
    private const string MessagesPath = "v1/messages";

    private readonly HttpClient _httpClient;
    private readonly AnthropicProviderOptions _options;

    public AnthropicProvider(AnthropicProviderOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.ApiKey))
            throw new ArgumentException("ApiKey cannot be null or whitespace.", nameof(options));

        if (string.IsNullOrWhiteSpace(options.ApiVersion))
            throw new ArgumentException("ApiVersion cannot be null or whitespace.", nameof(options));

        _options = options;
        _httpClient = CreateHttpClient(options);
    }

    public async Task<LlmResponse> CompleteAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var temperature = request.Temperature ?? _options.Temperature;
        var maxTokens = request.MaxTokens ?? _options.MaxTokens;
        AnthropicMessage[] messages = [new AnthropicMessage("user", request.Prompt)];
        var messageRequest = new AnthropicMessageRequest(_options.Model, maxTokens, messages, temperature, request.SystemPrompt);

        using var response = await _httpClient.PostAsJsonAsync(MessagesPath, messageRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var messageResponse = await response.Content.ReadFromJsonAsync<AnthropicMessageResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Anthropic returned an empty response payload.");

        var text = messageResponse.Content?
            .FirstOrDefault(block => string.Equals(block.Type, "text", StringComparison.OrdinalIgnoreCase))?
            .Text;

        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("Anthropic response did not contain text content.");

        return new LlmResponse(
            text,
            ProviderName,
            messageResponse.Usage?.InputTokens,
            messageResponse.Usage?.OutputTokens);
    }

    public void Dispose() => _httpClient.Dispose();

    private static HttpClient CreateHttpClient(AnthropicProviderOptions options)
    {
        var handler = options.HttpMessageHandler ?? new HttpClientHandler();
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.anthropic.com/"),
            Timeout = options.Timeout
        };

        client.DefaultRequestHeaders.TryAddWithoutValidation("x-api-key", options.ApiKey);
        client.DefaultRequestHeaders.TryAddWithoutValidation("anthropic-version", options.ApiVersion);

        return client;
    }
}
