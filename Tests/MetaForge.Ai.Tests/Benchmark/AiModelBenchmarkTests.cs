using FluentAssertions;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Types;

namespace MetaForge.Ai.Tests.Benchmark;

/// <summary>
/// AI Model Benchmark — strukturální srovnání Ollama modelů s referenčními výstupy.
/// Testy se přeskakují, pokud Ollama není dostupná.
/// </summary>
public class AiModelBenchmarkTests
{
    private static readonly string[] ModelsToTest = ["gemma3:12b", "llama3.2:3b", "phi3:mini", "mistral:7b"];

    /// <summary>Ověří, že CoreElementComparer správně detekuje shodu.</summary>
    [Fact]
    public void Comparer_IdenticalClasses_ReturnsTrue()
    {
        var a = new ClassElement { Name = "User" };
        a.Properties.Add(PropertyElement.GetSet("Name", Core.DataTypes.TypeModel.String));
        a.Properties.Add(PropertyElement.GetSet("Age", Core.DataTypes.TypeModel.Int32));

        var b = new ClassElement { Name = "User" };
        b.Properties.Add(PropertyElement.GetSet("Name", Core.DataTypes.TypeModel.String));
        b.Properties.Add(PropertyElement.GetSet("Age", Core.DataTypes.TypeModel.Int32));

        CoreElementComparer.AreStructurallyEquivalent(a, b).Should().BeTrue();
    }

    /// <summary>Ověří, že CoreElementComparer správně detekuje rozdíl.</summary>
    [Fact]
    public void Comparer_DifferentClasses_ReturnsFalse()
    {
        var a = new ClassElement { Name = "User" };
        a.Properties.Add(PropertyElement.GetSet("Name", Core.DataTypes.TypeModel.String));

        var b = new ClassElement { Name = "Customer" };
        b.Properties.Add(PropertyElement.GetSet("Name", Core.DataTypes.TypeModel.String));

        CoreElementComparer.AreStructurallyEquivalent(a, b).Should().BeFalse();
    }

    /// <summary>Ověří Diff výstup pro rozdílné třídy.</summary>
    [Fact]
    public void Comparer_Diff_DetectsMissingProperty()
    {
        var a = new ClassElement { Name = "User" };
        a.Properties.Add(PropertyElement.GetSet("Email", Core.DataTypes.TypeModel.String));

        var b = new ClassElement { Name = "User" };
        // No Email property

        var diffs = CoreElementComparer.Diff(a, b);
        diffs.Should().Contain(d => d.Contains("Email"));
    }

    /// <summary>Ověří, že Diff detekuje rozdíl v typu property.</summary>
    [Fact]
    public void Comparer_Diff_DetectsTypeMismatch()
    {
        var a = new ClassElement { Name = "User" };
        a.Properties.Add(PropertyElement.GetSet("Age", Core.DataTypes.TypeModel.Int32));

        var b = new ClassElement { Name = "User" };
        b.Properties.Add(PropertyElement.GetSet("Age", Core.DataTypes.TypeModel.String));

        var diffs = CoreElementComparer.Diff(a, b);
        diffs.Should().Contain(d => d.Contains("Age") && d.Contains("type mismatch"));
    }
}
