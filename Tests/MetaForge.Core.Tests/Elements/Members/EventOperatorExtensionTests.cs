using FluentAssertions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Types;

namespace MetaForge.Core.Tests.Elements.Members;

public class EventOperatorExtensionTests
{
    [Fact]
    public void EventElement_Basic_HasDelegateTypeName()
    {
        var evt = EventElement.Basic("OrderPlaced", "EventHandler");

        evt.Name.Should().Be("OrderPlaced");
        evt.DelegateTypeName.Should().Be("EventHandler");
        evt.HasCustomAccessors.Should().BeFalse();
    }

    [Fact]
    public void ClassElement_WithEvent_AddsToList()
    {
        var cls = ClassElement.Basic("OrderService")
            .WithEvent(EventElement.Basic("OrderPlaced", "EventHandler"));

        cls.Events.Should().ContainSingle();
    }

    [Fact]
    public void OperatorElement_Binary_HasTwoParameters()
    {
        var op = OperatorElement.Binary(
            OperatorKind.Add,
            TypeModel.Object.WithCustomName("Money"),
            new ParameterElement { Name = "a", Type = TypeModel.Object.WithCustomName("Money") },
            new ParameterElement { Name = "b", Type = TypeModel.Object.WithCustomName("Money") });

        op.Operator.Should().Be(OperatorKind.Add);
        op.Parameters.Should().HaveCount(2);
    }

    [Fact]
    public void OperatorElement_Unary_HasOneParameter()
    {
        var op = OperatorElement.Unary(
            OperatorKind.UnaryNegation,
            TypeModel.Object.WithCustomName("Money"),
            new ParameterElement { Name = "a", Type = TypeModel.Object.WithCustomName("Money") });

        op.Parameters.Should().ContainSingle();
    }

    [Fact]
    public void ClassElement_WithOperator_AddsToList()
    {
        var cls = ClassElement.Basic("Money")
            .WithOperator(OperatorElement.Binary(
                OperatorKind.Add,
                TypeModel.Object.WithCustomName("Money"),
                new ParameterElement { Name = "a", Type = TypeModel.Object.WithCustomName("Money") },
                new ParameterElement { Name = "b", Type = TypeModel.Object.WithCustomName("Money") }));

        cls.Operators.Should().ContainSingle();
    }

    [Fact]
    public void StructElement_WithOperator_AddsToList()
    {
        var str = StructElement.Basic("Point")
            .WithOperator(OperatorElement.Binary(
                OperatorKind.Equality,
                TypeModel.Bool,
                new ParameterElement { Name = "a", Type = TypeModel.Object.WithCustomName("Point") },
                new ParameterElement { Name = "b", Type = TypeModel.Object.WithCustomName("Point") }));

        str.Operators.Should().ContainSingle();
    }

    [Fact]
    public void MethodElement_AsExtensionMethod_SetsStaticAndExtensionFlags()
    {
        var method = MethodElement.Basic("IsNullOrEmpty").AsExtensionMethod();

        method.IsStatic.Should().BeTrue();
        method.IsExtensionMethod.Should().BeTrue();
    }

    [Fact]
    public void MethodElement_Default_IsNotExtensionMethod()
    {
        var method = MethodElement.Basic("Get");
        method.IsExtensionMethod.Should().BeFalse();
    }
}
