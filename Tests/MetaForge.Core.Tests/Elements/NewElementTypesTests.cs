// ---------------------------------------------------------------------------
// MetaForge.Core.Tests — NewElementTypesTests
// Tests for DelegateElement, EventElement, OperatorElement (PROP-037).
// ---------------------------------------------------------------------------

using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Types;

namespace MetaForge.Core.Tests.Elements;

public class DelegateElementTests
{
    [Fact]
    public void Kind_IsDelegate()
    {
        var d = new DelegateElement { Name = "MyFunc" };
        Assert.Equal("delegate", d.Kind);
    }

    [Fact]
    public void Basic_Factory_SetsNameAndReturnType()
    {
        var d = DelegateElement.Basic("MyFunc", TypeModel.String);
        Assert.Equal("MyFunc", d.Name);
        Assert.Equal(TypeModel.String, d.ReturnType);
    }

    [Fact]
    public void Generic_Factory_SetsTypeParameters()
    {
        var d = DelegateElement.Generic("Func", TypeModel.Void, "T", "TResult");
        Assert.Equal(2, d.TypeParameters.Count);
        Assert.Contains("T", d.TypeParameters);
        Assert.Contains("TResult", d.TypeParameters);
    }

    [Fact]
    public void TotalCoin_IncludesParameters()
    {
        var d = new DelegateElement { Name = "MyFunc", Coin = 5 };
        d.Parameters.Add(new ParameterElement { Name = "x", Type = TypeModel.Int32, Coin = 1 });
        d.Parameters.Add(new ParameterElement { Name = "y", Type = TypeModel.Int32, Coin = 1 });
        Assert.Equal(7, d.TotalCoin);
    }

    [Fact]
    public void WithAccess_SetsAccessModifier()
    {
        var d = DelegateElement.Basic("MyFunc", TypeModel.Void)
            .WithAccess(AccessModifier.Internal);
        Assert.Equal(AccessModifier.Internal, d.AccessModifier);
    }

    [Fact]
    public void WithParameter_AddsParameter()
    {
        var d = DelegateElement.Basic("MyFunc", TypeModel.Void)
            .WithParameter(new ParameterElement { Name = "x", Type = TypeModel.Int32 });
        Assert.Single(d.Parameters);
        Assert.Equal("x", d.Parameters[0].Name);
    }

    [Fact]
    public void WithConstraint_AddsTypeConstraint()
    {
        var d = DelegateElement.Generic("Func", TypeModel.Void, "T")
            .WithConstraint(GenericConstraint.Class("T"));
        Assert.Single(d.TypeConstraints);
    }
}

public class EventElementTests
{
    [Fact]
    public void Basic_Factory_SetsNameAndType()
    {
        var evt = EventElement.Basic("MyEvent", TypeModel.String);
        Assert.Equal("MyEvent", evt.Name);
        Assert.Equal(TypeModel.String, evt.EventType);
    }

    [Fact]
    public void Static_Factory_SetsIsStatic()
    {
        var evt = EventElement.Static("MyEvent", TypeModel.String);
        Assert.True(evt.IsStatic);
    }

    [Fact]
    public void Default_AccessModifier_IsPublic()
    {
        var evt = new EventElement { Name = "MyEvent" };
        Assert.Equal(AccessModifier.Public, evt.AccessModifier);
    }

    [Fact]
    public void WithAccess_SetsAccessModifier()
    {
        var evt = EventElement.Basic("MyEvent", TypeModel.String)
            .WithAccess(AccessModifier.Private);
        Assert.Equal(AccessModifier.Private, evt.AccessModifier);
    }

    [Fact]
    public void WithAddRemove_SetsAccessors()
    {
        var evt = EventElement.Basic("MyEvent", TypeModel.String)
            .WithAddRemove(AccessModifier.Private, AccessModifier.Public);
        Assert.Equal(AccessModifier.Private, evt.AddAccessor);
        Assert.Equal(AccessModifier.Public, evt.RemoveAccessor);
    }

    [Fact]
    public void Attributes_Defaults_Empty()
    {
        var evt = new EventElement { Name = "MyEvent" };
        Assert.Empty(evt.Attributes);
    }

    [Fact]
    public void Coin_Defaults_To2()
    {
        var evt = new EventElement { Name = "MyEvent" };
        Assert.Equal(2, evt.Coin);
    }
}

public class OperatorElementTests
{
    [Fact]
    public void Unary_Factory_SetsKindAndOperand()
    {
        var op = OperatorElement.Unary(OperatorKind.UnaryMinus,
            TypeModel.Int32,
            new ParameterElement { Name = "value", Type = TypeModel.Int32 });

        Assert.Equal(OperatorKind.UnaryMinus, op.OperatorKind);
        Assert.Single(op.Parameters);
        Assert.Equal("value", op.Parameters[0].Name);
    }

    [Fact]
    public void Binary_Factory_SetsKindAndTwoOperands()
    {
        var op = OperatorElement.Binary(OperatorKind.Addition,
            TypeModel.Int32,
            new ParameterElement { Name = "a", Type = TypeModel.Int32 },
            new ParameterElement { Name = "b", Type = TypeModel.Int32 });

        Assert.Equal(OperatorKind.Addition, op.OperatorKind);
        Assert.Equal(2, op.Parameters.Count);
    }

    [Fact]
    public void Conversion_Factory_ValidatesKind()
    {
        var op = OperatorElement.Conversion(OperatorKind.Implicit,
            TypeModel.String,
            new ParameterElement { Name = "value", Type = TypeModel.Int32 });

        Assert.Equal(OperatorKind.Implicit, op.OperatorKind);
    }

    [Fact]
    public void Conversion_Factory_Throws_OnInvalidKind()
    {
        Assert.Throws<ArgumentException>(() =>
            OperatorElement.Conversion(OperatorKind.Addition,
                TypeModel.String,
                new ParameterElement { Name = "value", Type = TypeModel.Int32 }));
    }

    [Fact]
    public void IsStatic_AlwaysTrue()
    {
        var op = new OperatorElement { OperatorKind = OperatorKind.Addition };
        Assert.True(op.IsStatic);
        // Even if someone tries (reflection), it's computed, not settable
    }

    [Fact]
    public void Coin_Defaults_To3()
    {
        var op = new OperatorElement { OperatorKind = OperatorKind.Addition };
        Assert.Equal(3, op.Coin);
    }

    [Fact]
    public void OperatorKind_HasExpectedCount()
    {
        // 8 unary + 10 binary + 6 comparison + 2 conversion = 26 (UnsignedRightShift removed)
        var values = Enum.GetValues<OperatorKind>();
        Assert.Equal(26, values.Length);
    }
}
