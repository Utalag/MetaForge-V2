using FluentAssertions;
using MetaForge.Core.StandardLibraries;

namespace MetaForge.Core.Tests.StandardLibraries;

public class StandardLibraryTranslatorRegistryTests
{
    private readonly StandardLibraryTranslatorRegistry _registry = new();

    private sealed class TestTranslator : IStandardLibraryTranslator
    {
        public string OperationId { get; }
        public TestTranslator(string operationId) => OperationId = operationId;
        public StandardLibraryRequirements? Translate(string operationId) =>
            new(operationId, Array.Empty<string>());
    }

    /// <summary>Translator se zaregistruje.</summary>
    [Fact]
    public void Register_AddsTranslator()
    {
        _registry.Register(new TestTranslator("op1"));
        _registry.GetAll().Should().HaveCount(1);
    }

    /// <summary>Přepsání stejného klíče aktualizuje translator.</summary>
    [Fact]
    public void Register_OverwriteSameKey_UpdatesTranslator()
    {
        _registry.Register(new TestTranslator("op1"));
        _registry.Register(new TestTranslator("op1"));

        _registry.GetAll().Should().HaveCount(1);
    }

    /// <summary>Existující operationId vrátí translator.</summary>
    [Fact]
    public void Resolve_Existing_ReturnsTranslator()
    {
        _registry.Register(new TestTranslator("myOp"));

        var result = _registry.Resolve("myOp");
        result.Should().NotBeNull();
        result!.OperationId.Should().Be("myOp");
    }

    /// <summary>Neznámé operationId → null.</summary>
    [Fact]
    public void Resolve_Unknown_ReturnsNull()
    {
        var result = _registry.Resolve("nonexistent");
        result.Should().BeNull();
    }

    /// <summary>Vrátí všechny registrované.</summary>
    [Fact]
    public void GetAll_ReturnsAllRegistered()
    {
        _registry.Register(new TestTranslator("a"));
        _registry.Register(new TestTranslator("b"));

        var all = _registry.GetAll();
        all.Should().HaveCount(2);
    }

    /// <summary>Prázdný seznam pokud nic není registrováno.</summary>
    [Fact]
    public void GetAll_Empty_ReturnsEmpty()
    {
        _registry.GetAll().Should().BeEmpty();
    }
}
