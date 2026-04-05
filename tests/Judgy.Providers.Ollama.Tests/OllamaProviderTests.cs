using System.Net;
using System.Text.Json;
using FluentAssertions;
using Judgy.Providers.Ollama.Tests.Fakes;
using Xunit;

namespace Judgy.Providers.Ollama.Tests;

public class OllamaProviderTests
{
    private const string DefaultModel = "llama3:8b";
    private const string TestPrompt = "What is 2+2?";
    private const string TestSystemPrompt = "You are a math tutor.";
    private const string TestResponseText = "4";

    private static string MakeResponseJson(
        string content = TestResponseText,
        int promptEvalCount = 10,
        int evalCount = 5) =>
        $$"""
        {
            "message": {
                "role": "assistant",
                "content": "{{content}}"
            },
            "prompt_eval_count": {{promptEvalCount}},
            "eval_count": {{evalCount}}
        }
        """;

    private static OllamaProviderOptions MakeOptions(FakeHttpMessageHandler handler) =>
        new()
        {
            BaseUrl = "http://localhost:11434",
            Model = DefaultModel,
            HttpMessageHandler = handler
        };

    [Fact]
    public async Task CompleteAsync_SendsCorrectRequest()
    {
        var handler = new FakeHttpMessageHandler(MakeResponseJson());
        using var provider = new OllamaProvider(MakeOptions(handler));
        var request = new Judgy.Providers.LlmRequest(TestPrompt);

        await provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        handler.LastRequest.Should().NotBeNull();
        handler.LastRequest!.Method.Should().Be(HttpMethod.Post);
        handler.LastRequest.RequestUri!.PathAndQuery.Should().Be("/api/chat");
    }

    [Fact]
    public async Task CompleteAsync_SendsCorrectRequestBody()
    {
        var handler = new FakeHttpMessageHandler(MakeResponseJson());
        using var provider = new OllamaProvider(MakeOptions(handler));
        var request = new Judgy.Providers.LlmRequest(TestPrompt);

        await provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        handler.LastRequestBody.Should().NotBeNullOrEmpty();
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        var root = doc.RootElement;
        root.GetProperty("model").GetString().Should().Be(DefaultModel);
        root.GetProperty("stream").GetBoolean().Should().BeFalse();

        var messages = root.GetProperty("messages");
        messages.GetArrayLength().Should().Be(1);
        messages[0].GetProperty("role").GetString().Should().Be("user");
        messages[0].GetProperty("content").GetString().Should().Be(TestPrompt);
    }

    [Fact]
    public async Task CompleteAsync_SystemPromptIncludedWhenProvided()
    {
        var handler = new FakeHttpMessageHandler(MakeResponseJson());
        using var provider = new OllamaProvider(MakeOptions(handler));
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
    public async Task CompleteAsync_SystemPromptOmittedWhenNull()
    {
        var handler = new FakeHttpMessageHandler(MakeResponseJson());
        using var provider = new OllamaProvider(MakeOptions(handler));
        var request = new Judgy.Providers.LlmRequest(TestPrompt);

        await provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        var messages = doc.RootElement.GetProperty("messages");
        messages.GetArrayLength().Should().Be(1);
        messages[0].GetProperty("role").GetString().Should().Be("user");
    }

    [Fact]
    public async Task CompleteAsync_ParsesResponseCorrectly()
    {
        var handler = new FakeHttpMessageHandler(MakeResponseJson("Hello world", 15, 8));
        using var provider = new OllamaProvider(MakeOptions(handler));
        var request = new Judgy.Providers.LlmRequest(TestPrompt);

        var response = await provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        response.Text.Should().Be("Hello world");
        response.ProviderName.Should().Be("Ollama");
        response.PromptTokens.Should().Be(15);
        response.CompletionTokens.Should().Be(8);
    }

    [Fact]
    public async Task CompleteAsync_NullRequest_ThrowsArgumentNullException()
    {
        var handler = new FakeHttpMessageHandler(MakeResponseJson());
        using var provider = new OllamaProvider(MakeOptions(handler));

        var act = () => provider.CompleteAsync(null!, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .Where(ex => ex.ParamName == "request");
    }

    [Fact]
    public async Task CompleteAsync_HttpError_ThrowsHttpRequestException()
    {
        var handler = new FakeHttpMessageHandler("{}", HttpStatusCode.InternalServerError);
        using var provider = new OllamaProvider(MakeOptions(handler));
        var request = new Judgy.Providers.LlmRequest(TestPrompt);

        var act = () => provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task CompleteAsync_EmptyResponsePayload_ThrowsInvalidOperationException()
    {
        var handler = new FakeHttpMessageHandler("null");
        using var provider = new OllamaProvider(MakeOptions(handler));
        var request = new Judgy.Providers.LlmRequest(TestPrompt);

        var act = () => provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        var act = () => new OllamaProvider(null!);

        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("options");
    }

    [Fact]
    public void Constructor_EmptyBaseUrl_ThrowsArgumentException()
    {
        var act = () => new OllamaProvider(new OllamaProviderOptions { BaseUrl = "", Model = "test" });

        act.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be("options");
    }

    [Fact]
    public void Constructor_EmptyModel_ThrowsArgumentException()
    {
        var act = () => new OllamaProvider(new OllamaProviderOptions { BaseUrl = "http://localhost:11434", Model = "" });

        act.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be("options");
    }

    [Fact]
    public void Options_DefaultValues()
    {
        var options = new OllamaProviderOptions();

        options.BaseUrl.Should().Be("http://localhost:11434");
        options.Model.Should().Be("llama3:8b");
        options.ApiKey.Should().BeNull();
        options.Timeout.Should().Be(TimeSpan.FromSeconds(120));
        options.HttpMessageHandler.Should().BeNull();
    }

    [Fact]
    public async Task CompleteAsync_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var handler = new FakeHttpMessageHandler(MakeResponseJson());
        using var provider = new OllamaProvider(MakeOptions(handler));
        var request = new Judgy.Providers.LlmRequest(TestPrompt);

        var act = () => provider.CompleteAsync(request, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
