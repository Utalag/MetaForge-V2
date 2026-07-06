using FluentAssertions;
using MetaForge.Core.Abstractions;

namespace MetaForge.Core.Tests.Abstractions;

public class AttributeElementTests
{
    /// <summary>Výchozí Name je prázdný string.</summary>
    [Fact]
    public void Name_Default_IsEmptyString()
    {
        var attr = new AttributeElement();
        attr.Name.Should().Be(string.Empty);
    }

    /// <summary>Arguments je prázdný seznam.</summary>
    [Fact]
    public void Arguments_Default_IsEmpty()
    {
        var attr = new AttributeElement();
        attr.Arguments.Should().BeEmpty();
    }

    /// <summary>Arguments může obsahovat null hodnoty.</summary>
    [Fact]
    public void Arguments_CanContainNull()
    {
        var attr = new AttributeElement { Name = "Obsolete" };
        attr.Arguments.Add(null);
        attr.Arguments.Should().ContainSingle().Which.Should().BeNull();
    }

    /// <summary>Arguments může obsahovat smíšené typy.</summary>
    [Fact]
    public void Arguments_CanContainMixedTypes()
    {
        var attr = new AttributeElement { Name = "MyAttr" };
        attr.Arguments.Add("hello");
        attr.Arguments.Add(42);
        attr.Arguments.Add(null);
        attr.Arguments.Should().HaveCount(3);
    }
}
