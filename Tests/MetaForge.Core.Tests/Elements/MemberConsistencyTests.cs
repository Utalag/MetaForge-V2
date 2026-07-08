// ---------------------------------------------------------------------------
// MetaForge.Core.Tests — PROP-040 tests
// IMemberElement, PropertyElement Attributes, XmlSummary
// ---------------------------------------------------------------------------

using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;

namespace MetaForge.Core.Tests.Elements;

public class MemberConsistencyTests
{
    [Fact]
    public void MethodElement_Implements_IMemberElement()
    {
        var m = new MethodElement { Name = "Test" };
        Assert.IsAssignableFrom<IMemberElement>(m);
    }

    [Fact]
    public void PropertyElement_Implements_IMemberElement()
    {
        var p = new PropertyElement { Name = "Test" };
        Assert.IsAssignableFrom<IMemberElement>(p);
    }

    [Fact]
    public void EventElement_Implements_IMemberElement()
    {
        var e = new EventElement { Name = "Test" };
        Assert.IsAssignableFrom<IMemberElement>(e);
    }

    [Fact]
    public void OperatorElement_Implements_IMemberElement()
    {
        var o = new OperatorElement { OperatorKind = OperatorKind.Addition };
        Assert.IsAssignableFrom<IMemberElement>(o);
    }

    [Fact]
    public void IMemberElement_Polymorphic_Iteration()
    {
        IMemberElement[] members =
        {
            new MethodElement { Name = "M1", Coin = 5 },
            new PropertyElement { Name = "P1", Coin = 2 },
            new EventElement { Name = "E1", Coin = 2 },
            new OperatorElement { OperatorKind = OperatorKind.Addition, Coin = 3 }
        };

        Assert.Equal(4, members.Length);
        var totalCoin = members.Sum(m => m.Coin);
        Assert.Equal(12, totalCoin);
    }

    [Fact]
    public void PropertyElement_HasAttributes()
    {
        var p = new PropertyElement { Name = "Test" };
        Assert.NotNull(p.Attributes);
        Assert.Empty(p.Attributes);
    }

    [Fact]
    public void PropertyElement_CanAddAttributes()
    {
        var p = new PropertyElement { Name = "Test" };
        p.Attributes.Add(new AttributeElement { Name = "Required" });
        Assert.Single(p.Attributes);
        Assert.Equal("Required", p.Attributes[0].Name);
    }

    [Fact]
    public void PropertyElement_HasXmlSummary()
    {
        var p = new PropertyElement { Name = "Test", XmlSummary = "The test property." };
        Assert.Equal("The test property.", p.XmlSummary);
    }

    [Fact]
    public void MethodElement_HasXmlSummary()
    {
        var m = new MethodElement { Name = "Test", XmlSummary = "Does something." };
        Assert.Equal("Does something.", m.XmlSummary);
    }

    [Fact]
    public void EventElement_HasXmlSummary()
    {
        var e = new EventElement { Name = "Test", XmlSummary = "Raised when..." };
        Assert.Equal("Raised when...", e.XmlSummary);
    }

    [Fact]
    public void OperatorElement_HasXmlSummary()
    {
        var o = new OperatorElement { OperatorKind = OperatorKind.Addition, XmlSummary = "Adds two values." };
        Assert.Equal("Adds two values.", o.XmlSummary);
    }
}
