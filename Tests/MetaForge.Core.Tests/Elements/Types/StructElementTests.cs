using FluentAssertions;
using MetaForge.Core.Abstractions;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Types;

namespace MetaForge.Core.Tests.Elements.Types;

public class StructElementTests
{
    /// <summary>Kind vrací "struct".</summary>
    [Fact]
    public void Kind_Always_ReturnsStruct()
    {
        var st = new StructElement();
        st.Kind.Should().Be("struct");
    }

    /// <summary>TotalCoin agreguje Properties a Methods.</summary>
    [Fact]
    public void TotalCoin_WithPropertiesAndMethods_Aggregates()
    {
        var st = new StructElement { Coin = 2 };
        st.Properties.Add(new PropertyElement { Coin = 3 });
        st.Methods.Add(new MethodElement { Coin = 4 });

        st.TotalCoin.Should().Be(9);
    }

    /// <summary>IsReadOnly je false.</summary>
    [Fact]
    public void IsReadOnly_Default_IsFalse()
    {
        var st = new StructElement();
        st.IsReadOnly.Should().BeFalse();
    }

    /// <summary>IsRecord je false.</summary>
    [Fact]
    public void IsRecord_Default_IsFalse()
    {
        var st = new StructElement();
        st.IsRecord.Should().BeFalse();
    }

    /// <summary>Properties je prázdný seznam.</summary>
    [Fact]
    public void Properties_Default_IsEmpty()
    {
        var st = new StructElement();
        st.Properties.Should().BeEmpty();
    }

    /// <summary>Methods je prázdný seznam.</summary>
    [Fact]
    public void Methods_Default_IsEmpty()
    {
        var st = new StructElement();
        st.Methods.Should().BeEmpty();
    }
}
