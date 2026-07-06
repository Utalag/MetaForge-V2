using System.Net;

namespace MetaForge.Ai.Tests;

/// <summary>
/// Fake HttpMessageHandler pro testování — vrací předdefinovanou odpověď.
/// Nahrazuje mocking frameworky (dle testovacích konvencí — preferuj fakes).
/// </summary>
public sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage _response;
    private Func<HttpRequestMessage, HttpResponseMessage>? _responseFactory;

    public FakeHttpMessageHandler(HttpStatusCode statusCode, string? content = null)
    {
        _response = new HttpResponseMessage(statusCode);
        if (content is not null)
            _response.Content = new StringContent(content);
    }

    public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
    {
        _responseFactory = responseFactory;
        _response = null!;
    }

    /// <summary>
    /// Vytvoří HttpClient s tímto handlerem.
    /// </summary>
    public HttpClient ToHttpClient()
    {
        var client = new HttpClient(this);
        client.BaseAddress = new Uri("http://localhost:11434");
        return client;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = _responseFactory is not null
            ? _responseFactory(request)
            : _response;
        return Task.FromResult(response);
    }
}
