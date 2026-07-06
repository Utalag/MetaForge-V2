using FluentAssertions;
using MetaForge.Core.Abstractions;
using MetaForge.Core.Elements.Types;
using MetaForge.Generators;

namespace MetaForge.Core.Integration.Tests.Scenarios;

/// <summary>
/// Snapshot testy pro Class modifikátory — validní kombinace C1-C8 z matice.
/// Používá factory metody z PROP-033.
/// </summary>
public class ClassModifierSnapshots
{
    private readonly CodeGenerator _generator = new();

    /// <summary>C1 — Basic class.</summary>
    [Fact]
    public void C1_BasicClass()
    {
        var cls = ClassElement.Basic("Customer");
        var result = _generator.Generate(cls);

        SnapshotComparer.Verify("Class", nameof(C1_BasicClass), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.FileName.Should().Be("Customer.cs");
        result.SourceCode.Should().Contain("public class Customer");
    }

    /// <summary>C2 — Abstract class.</summary>
    [Fact]
    public void C2_AbstractClass()
    {
        var cls = ClassElement.Abstract("Report");
        var result = _generator.Generate(cls);

        SnapshotComparer.Verify("Class", nameof(C2_AbstractClass), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.SourceCode.Should().Contain("public abstract class Report");
    }

    /// <summary>C3 — Sealed class.</summary>
    [Fact]
    public void C3_SealedClass()
    {
        var cls = ClassElement.Sealed("Validator");
        var result = _generator.Generate(cls);

        SnapshotComparer.Verify("Class", nameof(C3_SealedClass), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.SourceCode.Should().Contain("public sealed class Validator");
    }

    /// <summary>C4 — Static class.</summary>
    [Fact]
    public void C4_StaticClass()
    {
        var cls = ClassElement.Static("MathUtils");
        var result = _generator.Generate(cls);

        SnapshotComparer.Verify("Class", nameof(C4_StaticClass), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.SourceCode.Should().Contain("public static class MathUtils");
    }

    /// <summary>C5 — Partial class.</summary>
    [Fact]
    public void C5_PartialClass()
    {
        var cls = ClassElement.Partial("Entity");
        var result = _generator.Generate(cls);

        SnapshotComparer.Verify("Class", nameof(C5_PartialClass), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.SourceCode.Should().Contain("public partial class Entity");
    }

    /// <summary>C6 — Record class.</summary>
    [Fact]
    public void C6_RecordClass()
    {
        var cls = ClassElement.Record("Person");
        var result = _generator.Generate(cls);

        SnapshotComparer.Verify("Class", nameof(C6_RecordClass), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.SourceCode.Should().Contain("public record class Person");
    }

    /// <summary>C7 — Abstract record class.</summary>
    [Fact]
    public void C7_AbstractRecordClass()
    {
        var cls = ClassElement.AbstractRecord("Entity");
        var result = _generator.Generate(cls);

        SnapshotComparer.Verify("Class", nameof(C7_AbstractRecordClass), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.SourceCode.Should().Contain("public abstract record class Entity");
    }

    /// <summary>C8 — Sealed record class.</summary>
    [Fact]
    public void C8_SealedRecordClass()
    {
        var cls = ClassElement.SealedRecord("Value");
        var result = _generator.Generate(cls);

        SnapshotComparer.Verify("Class", nameof(C8_SealedRecordClass), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.SourceCode.Should().Contain("public sealed record class Value");
    }
}
