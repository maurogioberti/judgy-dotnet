using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Judgy.Providers;

namespace Judgy.Providers.Http;

public sealed class HttpProvider : ILlmProvider, IDisposable
{
    private const string ProviderName = "Http";
    private const string PromptPlaceholder = "{{prompt}}";

    private readonly HttpClient _httpClient;
    private readonly HttpProviderOptions _options;

    public HttpProvider(HttpProviderOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.Endpoint))
            throw new ArgumentException("Endpoint cannot be null or whitespace.", nameof(options));

        _options = options;
        _httpClient = CreateHttpClient(options);
    }

    public async Task<LlmResponse> CompleteAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var escapedPrompt = JsonEncodedText.Encode(request.Prompt).ToString();
        var body = _options.RequestTemplate.Replace(PromptPlaceholder, escapedPrompt);

        using var content = new StringContent(body, Encoding.UTF8, new MediaTypeHeaderValue("application/json"));
        using var response = await _httpClient.PostAsync(_options.Endpoint, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        var text = ExtractResponse(responseBody);

        return new LlmResponse(text, ProviderName);
    }

    public void Dispose() => _httpClient.Dispose();

    private string ExtractResponse(string responseBody)
    {
        if (_options.RegexPattern is not null)
            return ExtractViaRegex(responseBody, _options.RegexPattern);

        return JsonPathExtractor.Extract(responseBody, _options.ResponseJsonPath);
    }

    private static string ExtractViaRegex(string responseBody, string pattern)
    {
        var match = Regex.Match(responseBody, pattern);

        if (!match.Success)
            throw new InvalidOperationException(
                $"Regex pattern '{pattern}' did not match the response body.");

        return match.Groups.Count > 1 ? match.Groups[1].Value : match.Value;
    }

    private static HttpClient CreateHttpClient(HttpProviderOptions options)
    {
        var handler = options.HttpMessageHandler ?? new HttpClientHandler();
        var client = new HttpClient(handler)
        {
            Timeout = options.Timeout
        };

        foreach (var header in options.Headers)
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }

        return client;
    }
}
