using System.Net;
using System.Text.Json;
using FluentAssertions;
using Judgy.Providers.Moonshot.Tests.Fakes;
using Xunit;

namespace Judgy.Providers.Moonshot.Tests;

public class MoonshotProviderTests
{
    private const string ValidApiKey = "sk-test-key-12345";
    private const string DefaultModel = "kimi-k2.5";
    private const string TestPrompt = "What is 2+2?";
    private const string TestSystemPrompt = "You are a math tutor.";
    private const string TestResponseText = "4";

    private static string MakeResponseJson(
        string content = TestResponseText,
        int promptTokens = 10,
        int completionTokens = 5) =>
        $$"""
        {
            "choices": [
                {
                    "message": {
                        "content": "{{content}}"
                    }
                }
            ],
            "usage": {
                "prompt_tokens": {{promptTokens}},
                "completion_tokens": {{completionTokens}}
            }
        }
        """;

    private static MoonshotProviderOptions MakeOptions(FakeHttpMessageHandler handler) =>
        new()
        {
            ApiKey = ValidApiKey,
            HttpMessageHandler = handler
        };

    [Fact]
    public async Task CompleteAsync_SendsCorrectRequest()
    {
        var handler = new FakeHttpMessageHandler(MakeResponseJson());
        using var provider = new MoonshotProvider(MakeOptions(handler));
        var request = new Judgy.Providers.LlmRequest(TestPrompt);

        await provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        handler.LastRequest.Should().NotBeNull();
        handler.LastRequest!.Method.Should().Be(HttpMethod.Post);
        handler.LastRequest.RequestUri!.PathAndQuery.Should().Be("/v1/chat/completions");
        handler.LastRequest.Headers.Authorization!.Scheme.Should().Be("Bearer");
        handler.LastRequest.Headers.Authorization.Parameter.Should().Be(ValidApiKey);
    }

    [Fact]
    public async Task CompleteAsync_SendsCorrectRequestBody()
    {
        var handler = new FakeHttpMessageHandler(MakeResponseJson());
        using var provider = new MoonshotProvider(MakeOptions(handler));
        var request = new Judgy.Providers.LlmRequest(TestPrompt);

        await provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        handler.LastRequestBody.Should().NotBeNullOrEmpty();
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        var root = doc.RootElement;
        root.GetProperty("model").GetString().Should().Be(DefaultModel);
        root.GetProperty("temperature").GetDouble().Should().Be(0.0);
        root.GetProperty("max_tokens").GetInt32().Should().Be(1024);

        var messages = root.GetProperty("messages");
        messages.GetArrayLength().Should().Be(1);
        messages[0].GetProperty("role").GetString().Should().Be("user");
        messages[0].GetProperty("content").GetString().Should().Be(TestPrompt);
    }

    [Fact]
    public async Task CompleteAsync_SystemPromptIncludedWhenProvided()
    {
        var handler = new FakeHttpMessageHandler(MakeResponseJson());
        using var provider = new MoonshotProvider(MakeOptions(handler));
        var request = new Judgy.Providers.LlmRequest(TestPrompt, systemPrompt: TestSystemPrompt);

        await provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        var messages = doc.RootElement.GetProperty("messages");
        messages.GetArrayLength().Should().Be(2);
        messages[0].GetProperty("role").GetString().Should().Be("system");
        messages[0].GetProperty("content").GetString().Should().Be(TestSystemPrompt);
        messages[1].GetProperty("role").GetString().Should().Be("user");
        messages[1].GetProperty("content").GetString().Should().Be(TestPrompt);
    }

    [Fact]
    public async Task CompleteAsync_ParsesResponseCorrectly()
    {
        var handler = new FakeHttpMessageHandler(MakeResponseJson("Hello world", 15, 8));
        using var provider = new MoonshotProvider(MakeOptions(handler));
        var request = new Judgy.Providers.LlmRequest(TestPrompt);

        var response = await provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        response.Text.Should().Be("Hello world");
        response.ProviderName.Should().Be("Moonshot");
        response.PromptTokens.Should().Be(15);
        response.CompletionTokens.Should().Be(8);
    }

    [Fact]
    public async Task CompleteAsync_UsesRequestTemperatureOverOptionsDefault()
    {
        var handler = new FakeHttpMessageHandler(MakeResponseJson());
        using var provider = new MoonshotProvider(MakeOptions(handler));
        var request = new Judgy.Providers.LlmRequest(TestPrompt, temperature: 0.7);

        await provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        doc.RootElement.GetProperty("temperature").GetDouble().Should().Be(0.7);
    }

    [Fact]
    public async Task CompleteAsync_UsesRequestMaxTokensOverOptionsDefault()
    {
        var handler = new FakeHttpMessageHandler(MakeResponseJson());
        using var provider = new MoonshotProvider(MakeOptions(handler));
        var request = new Judgy.Providers.LlmRequest(TestPrompt, maxTokens: 512);

        await provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        doc.RootElement.GetProperty("max_tokens").GetInt32().Should().Be(512);
    }

    [Fact]
    public async Task CompleteAsync_NullRequest_ThrowsArgumentNullException()
    {
        var handler = new FakeHttpMessageHandler(MakeResponseJson());
        using var provider = new MoonshotProvider(MakeOptions(handler));

        var act = () => provider.CompleteAsync(null!, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .Where(ex => ex.ParamName == "request");
    }

    [Fact]
    public async Task CompleteAsync_HttpError_ThrowsHttpRequestException()
    {
        var handler = new FakeHttpMessageHandler("{}", HttpStatusCode.InternalServerError);
        using var provider = new MoonshotProvider(MakeOptions(handler));
        var request = new Judgy.Providers.LlmRequest(TestPrompt);

        var act = () => provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task CompleteAsync_EmptyChoices_ThrowsInvalidOperationException()
    {
        var handler = new FakeHttpMessageHandler("""{"choices": [], "usage": null}""");
        using var provider = new MoonshotProvider(MakeOptions(handler));
        var request = new Judgy.Providers.LlmRequest(TestPrompt);

        var act = () => provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        var act = () => new MoonshotProvider(null!);

        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("options");
    }

    [Fact]
    public void Constructor_EmptyApiKey_ThrowsArgumentException()
    {
        var act = () => new MoonshotProvider(new MoonshotProviderOptions { ApiKey = "" });

        act.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be("options");
    }

    [Fact]
    public void Options_DefaultValues()
    {
        var options = new MoonshotProviderOptions();

        options.Model.Should().Be("kimi-k2.5");
        options.Temperature.Should().Be(0.0f);
        options.MaxTokens.Should().Be(1024);
        options.Timeout.Should().Be(TimeSpan.FromSeconds(60));
        options.ApiKey.Should().BeEmpty();
        options.HttpMessageHandler.Should().BeNull();
    }

    [Fact]
    public async Task CompleteAsync_UsesCustomModel()
    {
        var handler = new FakeHttpMessageHandler(MakeResponseJson());
        var options = MakeOptions(handler);
        options.Model = "custom-kimi-model";
        using var provider = new MoonshotProvider(options);
        var request = new Judgy.Providers.LlmRequest(TestPrompt);

        await provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        doc.RootElement.GetProperty("model").GetString().Should().Be("custom-kimi-model");
    }

    [Fact]
    public async Task CompleteAsync_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var handler = new FakeHttpMessageHandler(MakeResponseJson());
        using var provider = new MoonshotProvider(MakeOptions(handler));
        var request = new Judgy.Providers.LlmRequest(TestPrompt);

        var act = () => provider.CompleteAsync(request, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
