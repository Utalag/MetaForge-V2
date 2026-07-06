using FluentAssertions;
using MetaForge.Core.Catalog;
using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Tests.Catalog;

public class BuiltInCatalogProviderTests
{
    private readonly BuiltInCatalogProvider _provider = new();

    /// <summary>Vrátí všech 20 předdefinovaných presetů.</summary>
    [Fact]
    public void GetAllPresets_ReturnsAll20Presets()
    {
        var presets = _provider.GetAllPresets();
        presets.Should().HaveCount(20);
    }

    /// <summary>ProviderName je "BuiltIn".</summary>
    [Fact]
    public void ProviderName_Always_ReturnsBuiltIn()
    {
        _provider.ProviderName.Should().Be("BuiltIn");
    }

    /// <summary>"string" → TypeModel.String.</summary>
    [Fact]
    public void ResolveType_String_ReturnsString()
    {
        var result = _provider.ResolveType("string");
        result.Should().NotBeNull();
        result!.Type.BaseType.Should().Be(DataType.String);
    }

    /// <summary>"int" → TypeModel.Int32.</summary>
    [Fact]
    public void ResolveType_Int_ReturnsInt32()
    {
        var result = _provider.ResolveType("int");
        result.Should().NotBeNull();
        result!.Type.BaseType.Should().Be(DataType.Int32);
    }

    /// <summary>"decimal" → TypeModel.Decimal.</summary>
    [Fact]
    public void ResolveType_Decimal_ReturnsDecimal()
    {
        var result = _provider.ResolveType("decimal");
        result.Should().NotBeNull();
        result!.Type.BaseType.Should().Be(DataType.Decimal);
    }

    /// <summary>"bool" → TypeModel.Bool.</summary>
    [Fact]
    public void ResolveType_Bool_ReturnsBool()
    {
        var result = _provider.ResolveType("bool");
        result.Should().NotBeNull();
        result!.Type.BaseType.Should().Be(DataType.Bool);
    }

    /// <summary>"email" → TypeModel.String.</summary>
    [Fact]
    public void ResolveType_Email_ReturnsString()
    {
        var result = _provider.ResolveType("email");
        result.Should().NotBeNull();
        result!.Type.BaseType.Should().Be(DataType.String);
    }

    /// <summary>"phone" → TypeModel.String.</summary>
    [Fact]
    public void ResolveType_Phone_ReturnsString()
    {
        var result = _provider.ResolveType("phone");
        result.Should().NotBeNull();
        result!.Type.BaseType.Should().Be(DataType.String);
    }

    /// <summary>"guid" → TypeModel.Guid.</summary>
    [Fact]
    public void ResolveType_Guid_ReturnsGuid()
    {
        var result = _provider.ResolveType("guid");
        result.Should().NotBeNull();
        result!.Type.BaseType.Should().Be(DataType.Guid);
    }

    /// <summary>"datetime" → TypeModel.DateTime.</summary>
    [Fact]
    public void ResolveType_DateTime_ReturnsDateTime()
    {
        var result = _provider.ResolveType("datetime");
        result.Should().NotBeNull();
        result!.Type.BaseType.Should().Be(DataType.DateTime);
    }

    /// <summary>"url" → DataType.Uri.</summary>
    [Fact]
    public void ResolveType_Url_ReturnsUri()
    {
        var result = _provider.ResolveType("url");
        result.Should().NotBeNull();
        result!.Type.BaseType.Should().Be(DataType.Uri);
    }

    /// <summary>"money" → TypeModel.Decimal.</summary>
    [Fact]
    public void ResolveType_Money_ReturnsDecimal()
    {
        var result = _provider.ResolveType("money");
        result.Should().NotBeNull();
        result!.Type.BaseType.Should().Be(DataType.Decimal);
    }

    /// <summary>"price" → TypeModel.Decimal.</summary>
    [Fact]
    public void ResolveType_Price_ReturnsDecimal()
    {
        var result = _provider.ResolveType("price");
        result.Should().NotBeNull();
        result!.Type.BaseType.Should().Be(DataType.Decimal);
    }

    /// <summary>"text" → TypeModel.String (alias).</summary>
    [Fact]
    public void ResolveType_TextAlias_ReturnsString()
    {
        var result = _provider.ResolveType("text");
        result.Should().NotBeNull();
        result!.Type.BaseType.Should().Be(DataType.String);
    }

    /// <summary>"boolean" → TypeModel.Bool (alias).</summary>
    [Fact]
    public void ResolveType_BoolAlias_ReturnsBool()
    {
        var result = _provider.ResolveType("boolean");
        result.Should().NotBeNull();
        result!.Type.BaseType.Should().Be(DataType.Bool);
    }

    /// <summary>"uuid" → TypeModel.Guid (alias).</summary>
    [Fact]
    public void ResolveType_UuidAlias_ReturnsGuid()
    {
        var result = _provider.ResolveType("uuid");
        result.Should().NotBeNull();
        result!.Type.BaseType.Should().Be(DataType.Guid);
    }

    /// <summary>"uri" → DataType.Uri (alias).</summary>
    [Fact]
    public void ResolveType_UriAlias_ReturnsUri()
    {
        var result = _provider.ResolveType("uri");
        result.Should().NotBeNull();
        result!.Type.BaseType.Should().Be(DataType.Uri);
    }

    /// <summary>"long" → DataType.Int64.</summary>
    [Fact]
    public void ResolveType_Long_ReturnsInt64()
    {
        var result = _provider.ResolveType("long");
        result.Should().NotBeNull();
        result!.Type.BaseType.Should().Be(DataType.Int64);
    }

    /// <summary>"double" → DataType.Double.</summary>
    [Fact]
    public void ResolveType_Double_ReturnsDouble()
    {
        var result = _provider.ResolveType("double");
        result.Should().NotBeNull();
        result!.Type.BaseType.Should().Be(DataType.Double);
    }

    /// <summary>"float" → DataType.Single.</summary>
    [Fact]
    public void ResolveType_Float_ReturnsSingle()
    {
        var result = _provider.ResolveType("float");
        result.Should().NotBeNull();
        result!.Type.BaseType.Should().Be(DataType.Single);
    }

    /// <summary>"date" → DataType.DateOnly.</summary>
    [Fact]
    public void ResolveType_Date_ReturnsDateOnly()
    {
        var result = _provider.ResolveType("date");
        result.Should().NotBeNull();
        result!.Type.BaseType.Should().Be(DataType.DateOnly);
    }

    /// <summary>"time" → DataType.TimeOnly.</summary>
    [Fact]
    public void ResolveType_Time_ReturnsTimeOnly()
    {
        var result = _provider.ResolveType("time");
        result.Should().NotBeNull();
        result!.Type.BaseType.Should().Be(DataType.TimeOnly);
    }

    /// <summary>Neznámý název → null.</summary>
    [Fact]
    public void ResolveType_Unknown_ReturnsNull()
    {
        var result = _provider.ResolveType("nonexistent_type_12345");
        result.Should().BeNull();
    }

    /// <summary>Email má tagy "contact" a "validation".</summary>
    [Fact]
    public void ResolveType_Email_HasContactValidationTags()
    {
        var result = _provider.ResolveType("email");
        result.Should().NotBeNull();
        result!.Tags.Should().Contain("contact");
        result.Tags.Should().Contain("validation");
    }

    /// <summary>Phone má tag "contact".</summary>
    [Fact]
    public void ResolveType_Phone_HasContactTag()
    {
        var result = _provider.ResolveType("phone");
        result.Should().NotBeNull();
        result!.Tags.Should().Contain("contact");
    }

    /// <summary>Money má tag "finance".</summary>
    [Fact]
    public void ResolveType_Money_HasFinanceTag()
    {
        var result = _provider.ResolveType("money");
        result.Should().NotBeNull();
        result!.Tags.Should().Contain("finance");
    }

    /// <summary>Price má tag "finance".</summary>
    [Fact]
    public void ResolveType_Price_HasFinanceTag()
    {
        var result = _provider.ResolveType("price");
        result.Should().NotBeNull();
        result!.Tags.Should().Contain("finance");
    }
}
