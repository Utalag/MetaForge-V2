// ---------------------------------------------------------------------------
// MetaForge.Core.Tests — PROP-041 tests
// ConstructorElement + FieldElement
// ---------------------------------------------------------------------------

using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Types;

namespace MetaForge.Core.Tests.Elements;

public class ConstructorElementTests
{
    [Fact]
    public void Basic_Factory_CreatesPublicConstructor()
    {
        var ctor = ConstructorElement.Basic("Customer",
            new ParameterElement { Name = "id", Type = TypeModel.Guid });

        Assert.Equal("Customer", ctor.Name);
        Assert.Single(ctor.Parameters);
        Assert.Equal(AccessModifier.Public, ctor.AccessModifier);
        Assert.False(ctor.IsStatic);
    }

    [Fact]
    public void Private_Factory_CreatesPrivateConstructor()
    {
        var ctor = ConstructorElement.Private("Customer");
        Assert.Equal(AccessModifier.Private, ctor.AccessModifier);
    }

    [Fact]
    public void Static_Factory_CreatesStaticConstructor()
    {
        var ctor = ConstructorElement.Static("Customer");
        Assert.True(ctor.IsStatic);
        Assert.Empty(ctor.Parameters);
    }

    [Fact]
    public void WithInitializer_SetsInitializer()
    {
        var ctor = ConstructorElement.Basic("Customer")
            .WithInitializer("base(name)");
        Assert.Equal("base(name)", ctor.Initializer);
    }

    [Fact]
    public void WithBody_SetsBody()
    {
        var body = new MetaForge.Core.Elements.Statements.BlockStatement();
        var ctor = ConstructorElement.Basic("Customer").WithBody(body);
        Assert.Same(body, ctor.Body);
    }

    [Fact]
    public void TotalCoin_IncludesParameters()
    {
        var ctor = ConstructorElement.Basic("Customer",
            new ParameterElement { Name = "x", Type = TypeModel.Int32, Coin = 1 },
            new ParameterElement { Name = "y", Type = TypeModel.Int32, Coin = 1 });
        Assert.Equal(5, ctor.TotalCoin); // 3 base + 2 params
    }

    [Fact]
    public void Implements_IMemberElement()
    {
        var ctor = ConstructorElement.Basic("Test");
        Assert.IsAssignableFrom<IMemberElement>(ctor);
    }

    [Fact]
    public void Attributes_DefaultEmpty()
    {
        var ctor = ConstructorElement.Basic("Test");
        Assert.Empty(ctor.Attributes);
    }

    [Fact]
    public void XmlSummary_CanBeSet()
    {
        var ctor = new ConstructorElement { Name = "Test", XmlSummary = "Creates instance." };
        Assert.Equal("Creates instance.", ctor.XmlSummary);
    }
}

public class FieldElementTests
{
    [Fact]
    public void Basic_Factory_CreatesPrivateField()
    {
        var field = FieldElement.Basic("_logger", TypeModel.String);
        Assert.Equal("_logger", field.Name);
        Assert.Equal(TypeModel.String, field.Type);
        Assert.Equal(AccessModifier.Private, field.AccessModifier);
        Assert.False(field.IsReadOnly);
    }

    [Fact]
    public void ReadOnly_Factory()
    {
        var field = FieldElement.ReadOnly("_logger",
            TypeModel.Of(DataType.Entity).WithCustomName("ILogger"));
        Assert.True(field.IsReadOnly);
        Assert.False(field.IsStatic);
    }

    [Fact]
    public void StaticReadOnly_Factory()
    {
        var field = FieldElement.StaticReadOnly("_instance", TypeModel.String);
        Assert.True(field.IsReadOnly);
        Assert.True(field.IsStatic);
    }

    [Fact]
    public void Const_Factory()
    {
        var field = FieldElement.Const("MaxItems", TypeModel.Int32, "100");
        Assert.True(field.IsConst);
        Assert.Equal("100", field.DefaultValue);
    }

    [Fact]
    public void WithDefault_SetsDefaultValue()
    {
        var field = FieldElement.Basic("_count", TypeModel.Int32)
            .WithDefault("0");
        Assert.Equal("0", field.DefaultValue);
    }

    [Fact]
    public void WithAccess_ChangesModifier()
    {
        var field = FieldElement.Basic("_name", TypeModel.String)
            .WithAccess(AccessModifier.Protected);
        Assert.Equal(AccessModifier.Protected, field.AccessModifier);
    }

    [Fact]
    public void Implements_IMemberElement()
    {
        var field = FieldElement.Basic("_test", TypeModel.Int32);
        Assert.IsAssignableFrom<IMemberElement>(field);
    }

    [Fact]
    public void XmlSummary_CanBeSet()
    {
        var field = new FieldElement { Name = "_cache", XmlSummary = "Internal cache." };
        Assert.Equal("Internal cache.", field.XmlSummary);
    }
}

public class ClassElementExtendedTests
{
    [Fact]
    public void Constructors_DefaultEmpty()
    {
        var cls = ClassElement.Basic("Foo");
        Assert.Empty(cls.Constructors);
    }

    [Fact]
    public void Fields_DefaultEmpty()
    {
        var cls = ClassElement.Basic("Foo");
        Assert.Empty(cls.Fields);
    }

    [Fact]
    public void TotalCoin_IncludesConstructors()
    {
        var cls = new ClassElement { Name = "Foo" };
        cls.Coin = 10;
        var ctor = ConstructorElement.Basic("Foo",
            new ParameterElement { Name = "x", Type = TypeModel.Int32 });
        ctor.Parameters[0].Coin = 1;
        cls.Constructors.Add(ctor);
        Assert.Equal(14, cls.TotalCoin); // 10 + 3 (ctor) + 1 (param)
    }

    [Fact]
    public void TotalCoin_IncludesFields()
    {
        var cls = new ClassElement { Name = "Foo" };
        cls.Coin = 10;
        var field = FieldElement.Basic("_count", TypeModel.Int32);
        field.Coin = 1;
        cls.Fields.Add(field);
        Assert.Equal(11, cls.TotalCoin); // 10 + 1
    }
}
