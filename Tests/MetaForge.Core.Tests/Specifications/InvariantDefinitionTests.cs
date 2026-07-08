// ---------------------------------------------------------------------------
// MetaForge.Core.Tests — InvariantDefinitionTests
// Unit tests for InvariantDefinition creation and properties.
// PROPOSAL: PROP-036 — Core Specification Layer
// ---------------------------------------------------------------------------

using MetaForge.Core.Specifications;
using static MetaForge.Core.Specifications.InvariantExpressionBuilder;

namespace MetaForge.Core.Tests.Specifications;

public class InvariantDefinitionTests
{
    [Fact]
    public void Always_CreatesInvariant_WithoutWhen()
    {
        var def = InvariantDefinition.Always(
            code: "TEST_001",
            targetKind: "MethodElement",
            description: "Test invariant",
            must: Prop("$.Name").Eq("ValidName"));

        Assert.Equal("TEST_001", def.Code);
        Assert.Equal("MethodElement", def.TargetKind);
        Assert.Equal("Test invariant", def.Description);
        Assert.Equal(InvariantSeverity.Error, def.Severity);
        Assert.Equal(InvariantScope.Local, def.Scope);
        Assert.Null(def.When);
        Assert.NotNull(def.Must);
    }

    [Fact]
    public void WhenCondition_CreatesInvariant_WithWhen()
    {
        var def = InvariantDefinition.WhenCondition(
            code: "TEST_002",
            targetKind: "ClassElement",
            description: "Conditional test",
            when: Prop("$.IsAbstract").Eq(true),
            must: Prop("$.IsSealed").Eq(false));

        Assert.Equal("TEST_002", def.Code);
        Assert.NotNull(def.When);
        Assert.NotNull(def.Must);
        Assert.IsType<EqExpression>(def.When);
        Assert.IsType<EqExpression>(def.Must);
    }

    [Fact]
    public void Always_RespectsSeverityAndScope()
    {
        var def = InvariantDefinition.Always(
            code: "WARN_001",
            targetKind: "EnumElement",
            description: "Warning invariant",
            must: Const(true),
            severity: InvariantSeverity.Warning,
            scope: InvariantScope.Global);

        Assert.Equal(InvariantSeverity.Warning, def.Severity);
        Assert.Equal(InvariantScope.Global, def.Scope);
    }

    [Fact]
    public void GeneratorIntent_StoresTestGenerationHints()
    {
        var intent = new GeneratorIntent
        {
            AvoidInValidGenerator = true,
            GenerateInvalidTest = true,
            TestPriority = 5
        };

        Assert.True(intent.AvoidInValidGenerator);
        Assert.True(intent.GenerateInvalidTest);
        Assert.Equal(5, intent.TestPriority);
    }

    [Fact]
    public void InvariantProvenance_StoresAiMetadata()
    {
        var provenance = new InvariantProvenance
        {
            Source = "AI",
            CreatedAt = new DateTimeOffset(2026, 7, 8, 12, 0, 0, TimeSpan.Zero),
            Prompt = "Generate invariants for MethodElement",
            ModelVersion = "llama3.2:3b",
            FalsePositiveCount = 2
        };

        Assert.Equal("AI", provenance.Source);
        Assert.Equal("llama3.2:3b", provenance.ModelVersion);
        Assert.Equal(2, provenance.FalsePositiveCount);
    }

    [Fact]
    public void Metadata_StoresCustomKeyValues()
    {
        var def = InvariantDefinition.Always("META_001", "ClassElement", "With metadata", Const(true));
        def = def with { Metadata = new Dictionary<string, string> { ["source"] = "test", ["version"] = "1.0" } };

        Assert.NotNull(def.Metadata);
        Assert.Equal("test", def.Metadata["source"]);
        Assert.Equal("1.0", def.Metadata["version"]);
    }
}
