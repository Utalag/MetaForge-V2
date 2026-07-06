using FluentAssertions;
using MetaForge.Core.Elements.Types;
using MetaForge.Generators;

namespace MetaForge.Core.Integration.Tests.Scenarios;

/// <summary>
/// Snapshot testy pro Struct varianty — S1-S4 z matice.
/// </summary>
public class StructVariantsSnapshots
{
    private readonly CodeGenerator _generator = new();

    [Fact]
    public void S1_BasicStruct()
    {
        var st = StructElement.Basic("Point");
        var result = _generator.Generate(st);

        SnapshotComparer.Verify("Struct", nameof(S1_BasicStruct), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.SourceCode.Should().Contain("public struct Point");
    }

    [Fact]
    public void S2_ReadOnlyStruct()
    {
        var st = StructElement.ReadOnly("Vector");
        var result = _generator.Generate(st);

        SnapshotComparer.Verify("Struct", nameof(S2_ReadOnlyStruct), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.SourceCode.Should().Contain("readonly struct Vector");
    }

    [Fact]
    public void S3_RecordStruct()
    {
        var st = StructElement.Record("Sensor");
        var result = _generator.Generate(st);

        SnapshotComparer.Verify("Struct", nameof(S3_RecordStruct), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.SourceCode.Should().Contain("record struct Sensor");
    }

    [Fact]
    public void S4_ReadOnlyRecordStruct()
    {
        var st = StructElement.ReadOnlyRecord("Position");
        var result = _generator.Generate(st);

        SnapshotComparer.Verify("Struct", nameof(S4_ReadOnlyRecordStruct), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.SourceCode.Should().Contain("readonly record struct Position");
    }
}
