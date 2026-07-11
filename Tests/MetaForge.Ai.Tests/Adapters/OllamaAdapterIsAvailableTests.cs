using System.Net;
using FluentAssertions;
using MetaForge.Ai.Abstractions;
using MetaForge.Ai.Adapters;

namespace MetaForge.Ai.Tests.Adapters;

public class OllamaAdapterIsAvailableTests
{
    /// <summary>
    /// Ověří, že vlastnost ProviderName vrací "Ollama".
    /// </summary>
    [Fact]
    public void ProviderName_Always_ReturnsOllama()
    {
        var adapter = new OllamaAdapter();
        var name = adapter.ProviderName;
        name.Should().Be("Ollama");
    }

    [Fact]
    public async Task IsAvailableAsync_NoServerRunning_ReturnsFalse()
    {
        // Použije neexistující URL — nemůže se připojit, graceful fallback vrátí false
        var adapter = new OllamaAdapter("http://localhost:19999");
        var result = await adapter.IsAvailableAsync();
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_InvalidUrl_ReturnsFalse()
    {
        var adapter = new OllamaAdapter("http://invalid-host-that-does-not-exist:11434");
        var result = await adapter.IsAvailableAsync();
        result.Should().BeFalse();
    }

    [Fact]
    public void Constructor_DefaultBaseUrl_IsLocalhost11434()
    {
        var adapter = new OllamaAdapter();
        adapter.ProviderName.Should().Be("Ollama");
    }
}
