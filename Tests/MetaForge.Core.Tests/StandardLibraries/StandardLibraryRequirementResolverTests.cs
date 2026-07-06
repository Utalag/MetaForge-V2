using FluentAssertions;
using MetaForge.Core.StandardLibraries;

namespace MetaForge.Core.Tests.StandardLibraries;

public class StandardLibraryRequirementResolverTests
{
    private sealed class TestTranslator : IStandardLibraryTranslator
    {
        private readonly string[]? _namespaces;
        public string OperationId { get; }
        public TestTranslator(string operationId, string[]? namespaces = null)
        {
            OperationId = operationId;
            _namespaces = namespaces;
        }
        public StandardLibraryRequirements? Translate(string operationId) =>
            new(operationId, _namespaces ?? Array.Empty<string>());
    }

    private static StandardLibraryRequirementResolver CreateResolver(
        params IStandardLibraryTranslator[] translators)
    {
        var registry = new StandardLibraryTranslatorRegistry();
        foreach (var t in translators)
            registry.Register(t);
        return new StandardLibraryRequirementResolver(registry);
    }

    /// <summary>Existující operace vrátí požadavky.</summary>
    [Fact]
    public void Resolve_ExistingOperation_ReturnsRequirements()
    {
        var resolver = CreateResolver(new TestTranslator("concat", new[] { "System.Linq" }));
        var result = resolver.Resolve("concat");

        result.Should().NotBeNull();
        result!.OperationId.Should().Be("concat");
        result.RequiredNamespaces.Should().Contain("System.Linq");
    }

    /// <summary>Neznámá operace → null.</summary>
    [Fact]
    public void Resolve_UnknownOperation_ReturnsNull()
    {
        var resolver = CreateResolver();
        var result = resolver.Resolve("nonexistent");
        result.Should().BeNull();
    }

    /// <summary>Prázdný vstup → prázdný seznam.</summary>
    [Fact]
    public void GetRequiredNamespaces_EmptyInput_ReturnsEmpty()
    {
        var resolver = CreateResolver(new TestTranslator("op", new[] { "System" }));
        var result = resolver.GetRequiredNamespaces(Array.Empty<string>());
        result.Should().BeEmpty();
    }

    /// <summary>Více operací → unikátní namespaces.</summary>
    [Fact]
    public void GetRequiredNamespaces_MultipleOperations_AggregatesUnique()
    {
        var resolver = CreateResolver(
            new TestTranslator("op1", new[] { "System", "System.Collections" }),
            new TestTranslator("op2", new[] { "System.Linq", "System.Text" }));

        var result = resolver.GetRequiredNamespaces(new[] { "op1", "op2" });
        result.Should().Contain("System");
        result.Should().Contain("System.Collections");
        result.Should().Contain("System.Linq");
        result.Should().Contain("System.Text");
    }

    /// <summary>Duplicitní namespaces se deduplikují.</summary>
    [Fact]
    public void GetRequiredNamespaces_DuplicateNamespaces_Deduplicates()
    {
        var resolver = CreateResolver(
            new TestTranslator("op1", new[] { "System", "System.Linq" }),
            new TestTranslator("op2", new[] { "System", "System.Linq" }));

        var result = resolver.GetRequiredNamespaces(new[] { "op1", "op2" });
        result.Should().HaveCount(2); // "System" a "System.Linq" jen jednou
    }

    /// <summary>Když RequiredNamespaces je null, přeskočí se.</summary>
    [Fact]
    public void GetRequiredNamespaces_OperationWithNullNamespaces_Skipped()
    {
        var resolver = CreateResolver(
            new TestTranslator("op1", new[] { "System" }),
            new TestTranslator("op2", null)); // null namespaces

        var result = resolver.GetRequiredNamespaces(new[] { "op1", "op2" });
        result.Should().ContainSingle().Which.Should().Be("System");
    }
}
