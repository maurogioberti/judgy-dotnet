using System.Net;

namespace Judgy.Providers.AzureOpenAI.Tests.Fakes;

internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _responseContent;

    public HttpRequestMessage? LastRequest { get; private set; }
    public string? LastRequestBody { get; private set; }

    public FakeHttpMessageHandler(string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _responseContent = responseContent;
        _statusCode = statusCode;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        LastRequestBody = request.Content is not null
            ? await request.Content.ReadAsStringAsync(cancellationToken)
            : null;

        return new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_responseContent, System.Text.Encoding.UTF8, "application/json")
        };
    }
}
