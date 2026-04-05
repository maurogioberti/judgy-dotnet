using System.Net;
using System.Text.Json;
using FluentAssertions;
using Judgy.Providers.Http.Tests.Fakes;
using Xunit;

namespace Judgy.Providers.Http.Tests;

public class HttpProviderTests
{
    private const string TestEndpoint = "https://api.example.com/generate";
    private const string TestPrompt = "What is 2+2?";

    private static HttpProviderOptions MakeOptions(FakeHttpMessageHandler handler, string? responseJsonPath = null) =>
        new()
        {
            Endpoint = TestEndpoint,
            HttpMessageHandler = handler,
            ResponseJsonPath = responseJsonPath ?? "$.response"
        };

    [Fact]
    public async Task CompleteAsync_SendsTemplatedRequest()
    {
        var handler = new FakeHttpMessageHandler("""{"response": "4"}""");
        using var provider = new HttpProvider(MakeOptions(handler));
        var request = new Judgy.Providers.LlmRequest(TestPrompt);

        await provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        handler.LastRequestBody.Should().NotBeNullOrEmpty();
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        doc.RootElement.GetProperty("prompt").GetString().Should().Be(TestPrompt);
        handler.LastRequest.Should().NotBeNull();
        handler.LastRequest!.Method.Should().Be(HttpMethod.Post);
    }

    [Fact]
    public async Task CompleteAsync_ExtractsResponseViaJsonPath()
    {
        var handler = new FakeHttpMessageHandler("""{"response": "The answer is 4"}""");
        using var provider = new HttpProvider(MakeOptions(handler));
        var request = new Judgy.Providers.LlmRequest(TestPrompt);

        var response = await provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        response.Text.Should().Be("The answer is 4");
        response.ProviderName.Should().Be("Http");
    }

    [Fact]
    public async Task CompleteAsync_ExtractsResponseViaNestedJsonPath()
    {
        var handler = new FakeHttpMessageHandler("""{"data": {"text": "nested result"}}""");
        var options = MakeOptions(handler, "$.data.text");
        using var provider = new HttpProvider(options);
        var request = new Judgy.Providers.LlmRequest(TestPrompt);

        var response = await provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        response.Text.Should().Be("nested result");
    }

    [Fact]
    public async Task CompleteAsync_ExtractsResponseViaJsonPathWithArrayIndex()
    {
        var handler = new FakeHttpMessageHandler("""{"choices": [{"message": {"content": "array result"}}]}""");
        var options = MakeOptions(handler, "$.choices[0].message.content");
        using var provider = new HttpProvider(options);
        var request = new Judgy.Providers.LlmRequest(TestPrompt);

        var response = await provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        response.Text.Should().Be("array result");
    }

    [Fact]
    public async Task CompleteAsync_ExtractsResponseViaRegexPattern()
    {
        var handler = new FakeHttpMessageHandler("""{"result": "extracted: hello world"}""");
        var options = MakeOptions(handler);
        options.RegexPattern = @"extracted: (.+?)""";
        using var provider = new HttpProvider(options);
        var request = new Judgy.Providers.LlmRequest(TestPrompt);

        var response = await provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        response.Text.Should().Be("hello world");
    }

    [Fact]
    public async Task CompleteAsync_RegexPatternWithoutCaptureGroup_ReturnsFullMatch()
    {
        var handler = new FakeHttpMessageHandler("""{"result": "hello world"}""");
        var options = MakeOptions(handler);
        options.RegexPattern = "hello world";
        using var provider = new HttpProvider(options);
        var request = new Judgy.Providers.LlmRequest(TestPrompt);

        var response = await provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        response.Text.Should().Be("hello world");
    }

    [Fact]
    public async Task CompleteAsync_JsonEscapesPromptInTemplate()
    {
        var handler = new FakeHttpMessageHandler("""{"response": "ok"}""");
        using var provider = new HttpProvider(MakeOptions(handler));
        var request = new Judgy.Providers.LlmRequest("Say \"hello\"\nnewline");

        await provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        handler.LastRequestBody.Should().NotBeNullOrEmpty();
        handler.LastRequestBody.Should().NotContain("\"hello\"");
        handler.LastRequestBody.Should().Contain("\\u0022hello\\u0022");
    }

    [Fact]
    public async Task CompleteAsync_IncludesCustomHeaders()
    {
        var handler = new FakeHttpMessageHandler("""{"response": "ok"}""");
        var options = MakeOptions(handler);
        options.Headers = new Dictionary<string, string>
        {
            ["Authorization"] = "Bearer my-token",
            ["X-Custom"] = "custom-value"
        };
        using var provider = new HttpProvider(options);
        var request = new Judgy.Providers.LlmRequest(TestPrompt);

        await provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        handler.LastRequest.Should().NotBeNull();
        handler.LastRequest!.Headers.GetValues("Authorization").Should().Contain("Bearer my-token");
        handler.LastRequest.Headers.GetValues("X-Custom").Should().Contain("custom-value");
    }

    [Fact]
    public async Task CompleteAsync_UsesCustomRequestTemplate()
    {
        var handler = new FakeHttpMessageHandler("""{"output": "result"}""");
        var options = MakeOptions(handler, "$.output");
        options.RequestTemplate = """{"inputs": "{{prompt}}", "parameters": {"max_new_tokens": 100}}""";
        using var provider = new HttpProvider(options);
        var request = new Judgy.Providers.LlmRequest("test input");

        await provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        handler.LastRequestBody.Should().Contain("test input");
        handler.LastRequestBody.Should().Contain("max_new_tokens");
    }

    [Fact]
    public async Task CompleteAsync_NullRequest_ThrowsArgumentNullException()
    {
        var handler = new FakeHttpMessageHandler("""{"response": "ok"}""");
        using var provider = new HttpProvider(MakeOptions(handler));

        var act = () => provider.CompleteAsync(null!, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .Where(ex => ex.ParamName == "request");
    }

    [Fact]
    public async Task CompleteAsync_HttpError_ThrowsHttpRequestException()
    {
        var handler = new FakeHttpMessageHandler("{}", HttpStatusCode.InternalServerError);
        using var provider = new HttpProvider(MakeOptions(handler));
        var request = new Judgy.Providers.LlmRequest(TestPrompt);

        var act = () => provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        var act = () => new HttpProvider(null!);

        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("options");
    }

    [Fact]
    public void Constructor_EmptyEndpoint_ThrowsArgumentException()
    {
        var act = () => new HttpProvider(new HttpProviderOptions { Endpoint = "" });

        act.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be("options");
    }

    [Fact]
    public void Options_DefaultValues()
    {
        var options = new HttpProviderOptions();

        options.Endpoint.Should().BeEmpty();
        options.Headers.Should().BeEmpty();
        options.RequestTemplate.Should().Contain("{{prompt}}");
        options.ResponseJsonPath.Should().Be("$.response");
        options.RegexPattern.Should().BeNull();
        options.Timeout.Should().Be(TimeSpan.FromSeconds(60));
        options.HttpMessageHandler.Should().BeNull();
    }

    [Fact]
    public async Task CompleteAsync_RegexPatternNoMatch_ThrowsInvalidOperationException()
    {
        var handler = new FakeHttpMessageHandler("""{"result": "no match here"}""");
        var options = MakeOptions(handler);
        options.RegexPattern = "nonexistent_pattern_xyz";
        using var provider = new HttpProvider(options);
        var request = new LlmRequest(TestPrompt);

        var act = () => provider.CompleteAsync(request, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
