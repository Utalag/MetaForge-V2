using FluentAssertions;
using MetaForge.Core.Elements.Types;
using MetaForge.Generators;

namespace MetaForge.Core.Integration.Tests.Scenarios;

/// <summary>
/// Snapshot testy pro Enum varianty — E1-E4 z matice.
/// </summary>
public class EnumVariantsSnapshots
{
    private readonly CodeGenerator _generator = new();

    [Fact]
    public void E1_BasicEnum()
    {
        var enm = EnumElement.Basic("Status");
        var result = _generator.Generate(enm);

        SnapshotComparer.Verify("Enum", nameof(E1_BasicEnum), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.SourceCode.Should().Contain("public enum Status");
    }

    [Fact]
    public void E2_ByteEnum()
    {
        var enm = EnumElement.ByteEnum("Protocol");
        var result = _generator.Generate(enm);

        SnapshotComparer.Verify("Enum", nameof(E2_ByteEnum), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.SourceCode.Should().Contain("byte");
    }

    [Fact]
    public void E3_Int64Enum()
    {
        var enm = EnumElement.Int64Enum("BigFlags");
        var result = _generator.Generate(enm);

        SnapshotComparer.Verify("Enum", nameof(E3_Int64Enum), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.SourceCode.Should().Contain("long");
    }

    [Fact]
    public void E4_FlagsEnum()
    {
        var enm = EnumElement.Flags("Permissions");
        var result = _generator.Generate(enm);

        SnapshotComparer.Verify("Enum", nameof(E4_FlagsEnum), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.SourceCode.Should().Contain("[Flags]");
    }
}
