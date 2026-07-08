using FluentAssertions;
using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Types;
using MetaForge.Core.Elements.Members;

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

    // === PROP-035: Nové vlastnosti ===

    /// <summary>Namespace je ve výchozím stavu null.</summary>
    [Fact]
    public void Namespace_Default_IsNull()
    {
        var cls = new ClassElement();
        cls.Namespace.Should().BeNull();
    }

    /// <summary>XmlSummary je ve výchozím stavu null.</summary>
    [Fact]
    public void XmlSummary_Default_IsNull()
    {
        var cls = new ClassElement();
        cls.XmlSummary.Should().BeNull();
    }

    /// <summary>WithNamespace nastaví Namespace a vrací fluent this.</summary>
    [Fact]
    public void WithNamespace_SetsNamespace_ReturnsThis()
    {
        var cls = new ClassElement();
        var result = cls.WithNamespace("MyApp.Models");

        result.Should().BeSameAs(cls);
        cls.Namespace.Should().Be("MyApp.Models");
    }

    /// <summary>WithXmlSummary nastaví XmlSummary a vrací fluent this.</summary>
    [Fact]
    public void WithXmlSummary_SetsSummary_ReturnsThis()
    {
        var cls = new ClassElement();
        var result = cls.WithXmlSummary("Represents a customer entity.");

        result.Should().BeSameAs(cls);
        cls.XmlSummary.Should().Be("Represents a customer entity.");
    }

    /// <summary>PrimaryRecord factory vytvoří record s primary konstruktorem.</summary>
    [Fact]
    public void PrimaryRecord_CreatesRecordWithPrimaryConstructor()
    {
        var record = ClassElement.PrimaryRecord("Point",
            new ParameterElement { Name = "X", Type = TypeModel.Int32 },
            new ParameterElement { Name = "Y", Type = TypeModel.Int32 });

        record.IsRecord.Should().BeTrue();
        record.PrimaryConstructorParameters.Should().HaveCount(2);
        record.PrimaryConstructorParameters![0].Name.Should().Be("X");
        record.PrimaryConstructorParameters![1].Name.Should().Be("Y");
    }

    /// <summary>Generic factory vytvoří generickou třídu s TypeParameters a TypeConstraints.</summary>
    [Fact]
    public void GenericFactory_CreatesGenericClass()
    {
        var constraint = GenericConstraint.ClassWithCtor("T");
        var cls = ClassElement.Generic("Repository", ["T"], [constraint]);

        cls.TypeParameters.Should().BeEquivalentTo(["T"]);
        cls.TypeConstraints.Should().HaveCount(1);
        cls.TypeConstraints[0].Constraints.Should().Contain(ConstraintKind.Class);
        cls.TypeConstraints[0].Constraints.Should().Contain(ConstraintKind.ParameterlessCtor);
    }
}
