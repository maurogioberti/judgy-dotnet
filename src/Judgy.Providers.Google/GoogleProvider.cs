using System.Net.Http.Json;
using Judgy.Providers;

namespace Judgy.Providers.Google;

public sealed class GoogleProvider : ILlmProvider, IDisposable
{
    private const string ProviderName = "Google";

    private readonly HttpClient _httpClient;
    private readonly GoogleProviderOptions _options;

    public GoogleProvider(GoogleProviderOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.ApiKey))
            throw new ArgumentException("ApiKey cannot be null or whitespace.", nameof(options));

        if (string.IsNullOrWhiteSpace(options.Model))
            throw new ArgumentException("Model cannot be null or whitespace.", nameof(options));

        _options = options;
        _httpClient = CreateHttpClient(options);
    }

    public async Task<LlmResponse> CompleteAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var temperature = request.Temperature ?? _options.Temperature;
        var maxOutputTokens = request.MaxTokens ?? _options.MaxOutputTokens;
        var path = $"v1beta/models/{Uri.EscapeDataString(_options.Model)}:generateContent";
        var contents = new[] { new GoogleContent([new GooglePart(request.Prompt)], "user") };
        var generationConfig = new GoogleGenerationConfig(temperature, maxOutputTokens);
        GoogleContent? systemInstruction = request.SystemPrompt is null
            ? null
            : new GoogleContent([new GooglePart(request.SystemPrompt)]);

        var contentRequest = new GoogleGenerateContentRequest(contents, generationConfig, systemInstruction);

        using var response = await _httpClient.PostAsJsonAsync(path, contentRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var contentResponse = await response.Content.ReadFromJsonAsync<GoogleGenerateContentResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Google returned an empty response payload.");

        var text = contentResponse.Candidates?
            .FirstOrDefault()?
            .Content?
            .Parts
            .FirstOrDefault()?
            .Text;

        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("Google response did not contain text content.");

        return new LlmResponse(
            text,
            ProviderName,
            contentResponse.UsageMetadata?.PromptTokenCount,
            contentResponse.UsageMetadata?.CandidatesTokenCount);
    }

    public void Dispose() => _httpClient.Dispose();

    private static HttpClient CreateHttpClient(GoogleProviderOptions options)
    {
        var handler = options.HttpMessageHandler ?? new HttpClientHandler();
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://generativelanguage.googleapis.com/"),
            Timeout = options.Timeout
        };

        client.DefaultRequestHeaders.TryAddWithoutValidation("x-goog-api-key", options.ApiKey);

        return client;
    }
}
