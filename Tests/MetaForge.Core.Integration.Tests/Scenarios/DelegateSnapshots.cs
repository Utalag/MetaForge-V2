using FluentAssertions;
using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Types;
using MetaForge.Generators;

namespace MetaForge.Core.Integration.Tests.Scenarios;

/// <summary>
/// Snapshot testy pro Delegate generování.
/// Ověřuje, že generátor produkuje korektní C# delegate deklarace.
/// PROP-052 — Follow-up na PROP-037 + PROP-043.
/// </summary>
public class DelegateSnapshots
{
    private readonly CodeGenerator _generator = new();

    [Fact]
    public void D1_BasicDelegate()
    {
        var del = DelegateElement.Basic("ActionHandler", TypeModel.Void);
        del.WithParameter(new ParameterElement { Name = "message", Type = TypeModel.String });

        var result = _generator.Generate(del);

        SnapshotComparer.Verify("Delegate", nameof(D1_BasicDelegate), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.SourceCode.Should().Contain("delegate void ActionHandler(string message)");
    }

    [Fact]
    public void D2_GenericDelegate()
    {
        var del = DelegateElement.Generic("Transformer", TypeModel.Of(DataType.Entity).WithCustomName("TResult"), "TInput");
        del.WithParameter(new ParameterElement { Name = "input", Type = TypeModel.Of(DataType.Entity).WithCustomName("TInput") });

        var result = _generator.Generate(del);

        SnapshotComparer.Verify("Delegate", nameof(D2_GenericDelegate), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.SourceCode.Should().Contain("delegate TResult Transformer<TInput>(TInput input)");
    }

    [Fact]
    public void D3_DelegateWithMultipleParams()
    {
        var del = DelegateElement.Basic("Filter", TypeModel.Bool);
        del.WithParameter(new ParameterElement { Name = "id", Type = TypeModel.Int32 });
        del.WithParameter(new ParameterElement { Name = "name", Type = TypeModel.String });

        var result = _generator.Generate(del);

        SnapshotComparer.Verify("Delegate", nameof(D3_DelegateWithMultipleParams), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.SourceCode.Should().Contain("delegate bool Filter(int id, string name)");
    }

    [Fact]
    public void D4_InternalDelegate()
    {
        var del = DelegateElement.Basic("InternalCallback", TypeModel.Void);
        del.WithAccess(AccessModifier.Internal);
        del.WithParameter(new ParameterElement { Name = "data", Type = TypeModel.Object });

        var result = _generator.Generate(del);

        SnapshotComparer.Verify("Delegate", nameof(D4_InternalDelegate), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.SourceCode.Should().Contain("internal delegate void InternalCallback(object data)");
    }
}
