using FluentAssertions;
using MetaForge.Core.Abstractions;
using MetaForge.Core.Elements.Types;

namespace MetaForge.Core.Tests.Abstractions;

public class RootElementTests
{
    /// <summary>Id je vygenerovaný Guid (není prázdný).</summary>
    [Fact]
    public void Id_Default_IsNotEmptyGuid()
    {
        var element = new ClassElement();
        element.Id.Should().NotBe(Guid.Empty);
    }

    /// <summary>ClassElement.Kind vrací "class".</summary>
    [Fact]
    public void Kind_ClassElement_ReturnsClass()
    {
        var cls = new ClassElement();
        cls.Kind.Should().Be("class");
    }

    /// <summary>EnumElement.Kind vrací "enum".</summary>
    [Fact]
    public void Kind_EnumElement_ReturnsEnum()
    {
        var en = new EnumElement();
        en.Kind.Should().Be("enum");
    }

    /// <summary>InterfaceElement.Kind vrací "interface".</summary>
    [Fact]
    public void Kind_InterfaceElement_ReturnsInterface()
    {
        var iface = new InterfaceElement();
        iface.Kind.Should().Be("interface");
    }

    /// <summary>StructElement.Kind vrací "struct".</summary>
    [Fact]
    public void Kind_StructElement_ReturnsStruct()
    {
        var st = new StructElement();
        st.Kind.Should().Be("struct");
    }

    /// <summary>Bázový TotalCoin = Coin (neupravený potomkem).</summary>
    [Fact]
    public void TotalCoin_Default_ReturnsCoin()
    {
        var cls = new ClassElement { Coin = 7 };
        cls.TotalCoin.Should().Be(7);
    }

    /// <summary>Usings je prázdný seznam.</summary>
    [Fact]
    public void Usings_Default_IsEmpty()
    {
        var cls = new ClassElement();
        cls.Usings.Should().BeEmpty();
    }

    /// <summary>Attributes je prázdný seznam.</summary>
    [Fact]
    public void Attributes_Default_IsEmpty()
    {
        var cls = new ClassElement();
        cls.Attributes.Should().BeEmpty();
    }
}
