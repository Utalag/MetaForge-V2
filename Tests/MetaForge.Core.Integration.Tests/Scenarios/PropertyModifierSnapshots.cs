using FluentAssertions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Types;
using MetaForge.Generators;

namespace MetaForge.Core.Integration.Tests.Scenarios;

/// <summary>
/// Snapshot testy pro Property modifikátory — P1-P5,P8 z matice.
/// </summary>
public class PropertyModifierSnapshots
{
    private readonly CodeGenerator _generator = new();

    [Fact]
    public void P1_GetSet()
    {
        var cls = ClassElement.Basic("Customer");
        cls.Properties.Add(PropertyElement.GetSet("Name", TypeModel.String));
        var result = _generator.Generate(cls);

        SnapshotComparer.Verify("Property", nameof(P1_GetSet), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.SourceCode.Should().Contain("get;").And.Contain("set;");
    }

    [Fact]
    public void P2_GetOnly()
    {
        var cls = ClassElement.Basic("Customer");
        cls.Properties.Add(PropertyElement.GetOnly("Id", TypeModel.Guid));
        var result = _generator.Generate(cls);

        SnapshotComparer.Verify("Property", nameof(P2_GetOnly), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.SourceCode.Should().Contain("get;").And.NotContain("set;");
    }

    [Fact]
    public void P3_InitOnly()
    {
        var cls = ClassElement.Basic("Customer");
        cls.Properties.Add(PropertyElement.InitOnly("Email", TypeModel.String));
        var result = _generator.Generate(cls);

        SnapshotComparer.Verify("Property", nameof(P3_InitOnly), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.SourceCode.Should().Contain("init;");
    }

    [Fact]
    public void P4_Required()
    {
        var cls = ClassElement.Basic("Customer");
        cls.Properties.Add(PropertyElement.Required("Name", TypeModel.String));
        var result = _generator.Generate(cls);

        SnapshotComparer.Verify("Property", nameof(P4_Required), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.SourceCode.Should().Contain("required");
    }

    [Fact]
    public void P5_StaticProperty()
    {
        var cls = ClassElement.Basic("Config");
        cls.Properties.Add(PropertyElement.Static("Instance", TypeModel.Object));
        var result = _generator.Generate(cls);

        SnapshotComparer.Verify("Property", nameof(P5_StaticProperty), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.SourceCode.Should().Contain("static");
    }

    [Fact]
    public void P8_RequiredGetOnly()
    {
        var cls = ClassElement.Basic("Customer");
        cls.Properties.Add(PropertyElement.RequiredGetOnly("Id", TypeModel.Guid));
        var result = _generator.Generate(cls);

        SnapshotComparer.Verify("Property", nameof(P8_RequiredGetOnly), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.SourceCode.Should().Contain("required").And.Contain("get;").And.NotContain("set;");
    }
}
