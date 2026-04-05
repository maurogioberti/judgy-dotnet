using System.Net;
using System.Text.Json;
using FluentAssertions;
using Judgy.Providers.Google.Tests.Fakes;
using Xunit;

namespace Judgy.Providers.Google.Tests;

public class GoogleProviderTests
{
    private const string ValidApiKey = "google-test-key-12345";
    private const string DefaultModel = "gemini-2.5-flash";
    private const string TestPrompt = "What is 2+2?";
    private const string TestSystemPrompt = "You are a math tutor.";
    private const string TestResponseText = "4";

    private static string MakeResponseJson(
        string content = TestResponseText,
        int promptTokens = 10,
        int candidatesTokens = 5) =>
        $$"""
        {
            "candidates": [
                {
                    "content": {
                        "parts": [
                            {
                                "text": "{{content}}"
                            }
                        ]
                    }
                }
            ],
            "usageMetadata": {
                "promptTokenCount": {{promptTokens}},
                "candidatesTokenCount": {{candidatesTokens}}
            }
        }
        """;

    private static GoogleProviderOptions MakeOptions(FakeHttpMessageHandler handler) =>
        new()
        {
            ApiKey = ValidApiKey,
            HttpMessageHandler = handler
        };

    [Fact]
    public async Task CompleteAsync_SendsCorrectRequest()
    {
        var handler = new FakeHttpMessageHandler(MakeResponseJson());
        using var provider = new GoogleProvider(MakeOptions(handler));
        var request = new Judgy.Providers.LlmRequest(TestPrompt);

        await provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        handler.LastRequest.Should().NotBeNull();
        handler.LastRequest!.Method.Should().Be(HttpMethod.Post);
        handler.LastRequest.RequestUri!.PathAndQuery.Should().Be("/v1beta/models/gemini-2.5-flash:generateContent");
        handler.LastRequest.Headers.TryGetValues("x-goog-api-key", out var values).Should().BeTrue();
        values!.Single().Should().Be(ValidApiKey);
    }

    [Fact]
    public async Task CompleteAsync_SendsCorrectRequestBody()
    {
        var handler = new FakeHttpMessageHandler(MakeResponseJson());
        using var provider = new GoogleProvider(MakeOptions(handler));
        var request = new Judgy.Providers.LlmRequest(TestPrompt);

        await provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        handler.LastRequestBody.Should().NotBeNullOrEmpty();
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        var root = doc.RootElement;
        root.TryGetProperty("systemInstruction", out _).Should().BeFalse();

        var contents = root.GetProperty("contents");
        contents.GetArrayLength().Should().Be(1);
        contents[0].GetProperty("role").GetString().Should().Be("user");
        contents[0].GetProperty("parts")[0].GetProperty("text").GetString().Should().Be(TestPrompt);

        var generationConfig = root.GetProperty("generationConfig");
        generationConfig.GetProperty("temperature").GetDouble().Should().Be(0.0);
        generationConfig.GetProperty("maxOutputTokens").GetInt32().Should().Be(1024);
    }

    [Fact]
    public async Task CompleteAsync_SystemPromptIncludedWhenProvided()
    {
        var handler = new FakeHttpMessageHandler(MakeResponseJson());
        using var provider = new GoogleProvider(MakeOptions(handler));
        var request = new Judgy.Providers.LlmRequest(TestPrompt, systemPrompt: TestSystemPrompt);

        await provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        doc.RootElement.GetProperty("systemInstruction")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString()
            .Should().Be(TestSystemPrompt);
    }

    [Fact]
    public async Task CompleteAsync_ParsesResponseCorrectly()
    {
        var handler = new FakeHttpMessageHandler(MakeResponseJson("Hello world", 15, 8));
        using var provider = new GoogleProvider(MakeOptions(handler));
        var request = new Judgy.Providers.LlmRequest(TestPrompt);

        var response = await provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        response.Text.Should().Be("Hello world");
        response.ProviderName.Should().Be("Google");
        response.PromptTokens.Should().Be(15);
        response.CompletionTokens.Should().Be(8);
    }

    [Fact]
    public async Task CompleteAsync_UsesRequestTemperatureOverOptionsDefault()
    {
        var handler = new FakeHttpMessageHandler(MakeResponseJson());
        using var provider = new GoogleProvider(MakeOptions(handler));
        var request = new Judgy.Providers.LlmRequest(TestPrompt, temperature: 0.7);

        await provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        doc.RootElement.GetProperty("generationConfig").GetProperty("temperature").GetDouble().Should().Be(0.7);
    }

    [Fact]
    public async Task CompleteAsync_UsesRequestMaxTokensOverOptionsDefault()
    {
        var handler = new FakeHttpMessageHandler(MakeResponseJson());
        using var provider = new GoogleProvider(MakeOptions(handler));
        var request = new Judgy.Providers.LlmRequest(TestPrompt, maxTokens: 512);

        await provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        doc.RootElement.GetProperty("generationConfig").GetProperty("maxOutputTokens").GetInt32().Should().Be(512);
    }

    [Fact]
    public async Task CompleteAsync_NullRequest_ThrowsArgumentNullException()
    {
        var handler = new FakeHttpMessageHandler(MakeResponseJson());
        using var provider = new GoogleProvider(MakeOptions(handler));

        var act = () => provider.CompleteAsync(null!, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .Where(ex => ex.ParamName == "request");
    }

    [Fact]
    public async Task CompleteAsync_HttpError_ThrowsHttpRequestException()
    {
        var handler = new FakeHttpMessageHandler("{}", HttpStatusCode.InternalServerError);
        using var provider = new GoogleProvider(MakeOptions(handler));
        var request = new Judgy.Providers.LlmRequest(TestPrompt);

        var act = () => provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task CompleteAsync_MissingCandidates_ThrowsInvalidOperationException()
    {
        var handler = new FakeHttpMessageHandler("""{"candidates": [], "usageMetadata": null}""");
        using var provider = new GoogleProvider(MakeOptions(handler));
        var request = new Judgy.Providers.LlmRequest(TestPrompt);

        var act = () => provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        var act = () => new GoogleProvider(null!);

        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("options");
    }

    [Fact]
    public void Constructor_EmptyApiKey_ThrowsArgumentException()
    {
        var act = () => new GoogleProvider(new GoogleProviderOptions { ApiKey = "" });

        act.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be("options");
    }

    [Fact]
    public void Constructor_EmptyModel_ThrowsArgumentException()
    {
        var act = () => new GoogleProvider(new GoogleProviderOptions
        {
            ApiKey = ValidApiKey,
            Model = ""
        });

        act.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be("options");
    }

    [Fact]
    public void Options_DefaultValues()
    {
        var options = new GoogleProviderOptions();

        options.Model.Should().Be("gemini-2.5-flash");
        options.MaxOutputTokens.Should().Be(1024);
        options.Temperature.Should().Be(0.0f);
        options.Timeout.Should().Be(TimeSpan.FromSeconds(60));
        options.ApiKey.Should().BeEmpty();
        options.HttpMessageHandler.Should().BeNull();
    }

    [Fact]
    public async Task CompleteAsync_UsesCustomModel()
    {
        var handler = new FakeHttpMessageHandler(MakeResponseJson());
        var options = MakeOptions(handler);
        options.Model = "gemini-2.5-pro";
        using var provider = new GoogleProvider(options);
        var request = new Judgy.Providers.LlmRequest(TestPrompt);

        await provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        handler.LastRequest!.RequestUri!.PathAndQuery.Should().Be("/v1beta/models/gemini-2.5-pro:generateContent");
    }

    [Fact]
    public async Task CompleteAsync_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var handler = new FakeHttpMessageHandler(MakeResponseJson());
        using var provider = new GoogleProvider(MakeOptions(handler));
        var request = new Judgy.Providers.LlmRequest(TestPrompt);

        var act = () => provider.CompleteAsync(request, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
