using FluentAssertions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Types;

namespace MetaForge.Core.Tests.Elements.Types;

public class DelegateElementTests
{
    [Fact]
    public void Basic_CreatesDelegateWithVoidReturn()
    {
        var del = DelegateElement.Basic("Notify");

        del.Kind.Should().Be("delegate");
        del.Name.Should().Be("Notify");
        del.ReturnType.Should().Be(TypeModel.Void);
        del.Parameters.Should().BeEmpty();
    }

    [Fact]
    public void WithParameter_AddsToList()
    {
        var del = DelegateElement.Basic("Handler")
            .WithParameter(new ParameterElement { Name = "sender", Type = TypeModel.Object });

        del.Parameters.Should().ContainSingle();
    }

    [Fact]
    public void WithTypeParameter_AddsGenericParameter()
    {
        var del = DelegateElement.Basic("Factory")
            .WithTypeParameter(TypeParameterElement.Of("T"));

        del.TypeParameters.Should().ContainSingle();
    }
}
