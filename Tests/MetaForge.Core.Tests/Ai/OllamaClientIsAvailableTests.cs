using System.Net;
using FluentAssertions;
using MetaForge.Core.Ai;

namespace MetaForge.Core.Tests.Ai;

public class OllamaClientIsAvailableTests
{
    /// <summary>
    /// Ověří, že IsAvailableAsync vrátí true, když server odpoví 200 OK.
    /// </summary>
    [Fact]
    public async Task IsAvailableAsync_ServerRespondsOk_ReturnsTrue()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var client = new OllamaClient(handler.ToHttpClient());

        // Act
        var result = await client.IsAvailableAsync();

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Ověří, že IsAvailableAsync vrátí false, když server vrátí 503 Service Unavailable.
    /// </summary>
    [Fact]
    public async Task IsAvailableAsync_ServerRespondsServiceUnavailable_ReturnsFalse()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(HttpStatusCode.ServiceUnavailable);
        var client = new OllamaClient(handler.ToHttpClient());

        // Act
        var result = await client.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Ověří, že IsAvailableAsync vrátí false, když server vrátí 404 Not Found.
    /// </summary>
    [Fact]
    public async Task IsAvailableAsync_ServerRespondsNotFound_ReturnsFalse()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(HttpStatusCode.NotFound);
        var client = new OllamaClient(handler.ToHttpClient());

        // Act
        var result = await client.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Ověří, že IsAvailableAsync vrátí false, když HTTP request vyhodí výjimku (např. connection refused).
    /// Ověřuje graceful fallback — žádná výjimka se nešíří ven.
    /// </summary>
    [Fact]
    public async Task IsAvailableAsync_HttpRequestThrowsException_ReturnsFalse()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(_ =>
            throw new HttpRequestException("Connection refused"));
        var client = new OllamaClient(handler.ToHttpClient());

        // Act
        var result = await client.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Ověří, že IsAvailableAsync vrátí false, když dojde k timeoutu (TaskCanceledException).
    /// Ověřuje graceful fallback pro timeout scénář.
    /// </summary>
    [Fact]
    public async Task IsAvailableAsync_TaskCanceled_ReturnsFalse()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(_ =>
            throw new TaskCanceledException("Request timed out"));
        var client = new OllamaClient(handler.ToHttpClient());

        // Act
        var result = await client.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Ověří, že IsAvailableAsync volá GET /api/tags — správný Ollama endpoint pro health check.
    /// </summary>
    [Fact]
    public async Task IsAvailableAsync_SendsGetRequestToApiTags()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        var handler = new FakeHttpMessageHandler(request =>
        {
            capturedRequest = request;
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        var client = new OllamaClient(handler.ToHttpClient());

        // Act
        await client.IsAvailableAsync();

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Method.Should().Be(HttpMethod.Get);
        capturedRequest.RequestUri!.AbsolutePath.Should().Be("/api/tags");
    }
}
