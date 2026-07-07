using FluentAssertions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Types;

namespace MetaForge.Core.Tests.Elements.Members;

public class TypeParameterElementTests
{
    /// <summary>TypeParameterElement.Of vytvoří parametr bez omezení.</summary>
    [Fact]
    public void Of_NoConstraints_HasEmptyConstraintList()
    {
        var tp = TypeParameterElement.Of("T");

        tp.Name.Should().Be("T");
        tp.Constraints.Should().BeEmpty();
        tp.Variance.Should().Be(GenericVariance.None);
    }

    /// <summary>WithConstraint přidává omezení a vrací this (fluent).</summary>
    [Fact]
    public void WithConstraint_AddsToList()
    {
        var tp = TypeParameterElement.Of("T")
            .WithConstraint(GenericConstraint.Class())
            .WithConstraint(GenericConstraint.NewConstructor());

        tp.Constraints.Should().HaveCount(2);
        tp.Constraints[0].Kind.Should().Be(GenericConstraintKind.Class);
        tp.Constraints[1].Kind.Should().Be(GenericConstraintKind.NewConstructor);
    }

    /// <summary>GenericConstraint.BaseType nese název typu.</summary>
    [Fact]
    public void GenericConstraint_BaseType_HasTypeName()
    {
        var constraint = GenericConstraint.BaseType("Entity");

        constraint.Kind.Should().Be(GenericConstraintKind.BaseType);
        constraint.TypeName.Should().Be("Entity");
    }

    /// <summary>GenericConstraint.Interface nese název typu.</summary>
    [Fact]
    public void GenericConstraint_Interface_HasTypeName()
    {
        var constraint = GenericConstraint.Interface("IComparable<T>");

        constraint.Kind.Should().Be(GenericConstraintKind.Interface);
        constraint.TypeName.Should().Be("IComparable<T>");
    }

    /// <summary>ClassElement.TypeParameters lze naplnit přes WithTypeParameter.</summary>
    [Fact]
    public void ClassElement_WithTypeParameter_AddsToList()
    {
        var cls = ClassElement.Basic("Repository")
            .WithTypeParameter(TypeParameterElement.Of("T").WithConstraint(GenericConstraint.Class()));

        cls.TypeParameters.Should().ContainSingle();
        cls.TypeParameters[0].Name.Should().Be("T");
    }

    /// <summary>InterfaceElement.TypeParameters lze naplnit přes WithTypeParameter.</summary>
    [Fact]
    public void InterfaceElement_WithTypeParameter_AddsToList()
    {
        var iface = InterfaceElement.Basic("IRepository")
            .WithTypeParameter(TypeParameterElement.Of("T"));

        iface.TypeParameters.Should().ContainSingle();
    }

    /// <summary>StructElement.TypeParameters lze naplnit přes WithTypeParameter.</summary>
    [Fact]
    public void StructElement_WithTypeParameter_AddsToList()
    {
        var str = StructElement.Basic("Pair")
            .WithTypeParameter(TypeParameterElement.Of("T"));

        str.TypeParameters.Should().ContainSingle();
    }

    /// <summary>MethodElement.TypeParameters lze naplnit přes WithTypeParameter.</summary>
    [Fact]
    public void MethodElement_WithTypeParameter_AddsToList()
    {
        var method = MethodElement.Basic("Get")
            .WithTypeParameter(TypeParameterElement.Of("T"));

        method.TypeParameters.Should().ContainSingle();
    }

    /// <summary>Variance Out/In se nastaví korektně.</summary>
    [Fact]
    public void Variance_OutAndIn_SetCorrectly()
    {
        var covariant = new TypeParameterElement { Name = "T", Variance = GenericVariance.Out };
        var contravariant = new TypeParameterElement { Name = "T", Variance = GenericVariance.In };

        covariant.Variance.Should().Be(GenericVariance.Out);
        contravariant.Variance.Should().Be(GenericVariance.In);
    }
}
