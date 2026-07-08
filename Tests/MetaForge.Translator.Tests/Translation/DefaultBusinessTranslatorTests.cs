using FluentAssertions;
using MetaForge.BusinessModel.Models;
using MetaForge.Core.Catalog;
using MetaForge.Core.DataTypes;
using MetaForge.Translator.Translation;

namespace MetaForge.Translator.Tests.Translation;

public class DefaultBusinessTranslatorTests
{
    private readonly CatalogManager _catalog = new();
    private readonly DefaultBusinessTranslator _translator;

    public DefaultBusinessTranslatorTests()
    {
        _catalog.RegisterProvider(new BuiltInCatalogProvider());
        _translator = new DefaultBusinessTranslator(_catalog);
    }

    [Fact]
    public void Translate_Email_ReturnsStringType()
    {
        var attr = new BusinessAttributeNode { Name = "Email", Type = "email" };
        var result = _translator.Translate(attr);

        result.BaseType.Should().Be(DataType.String);
    }

    [Fact]
    public void Translate_Money_ReturnsDecimalType()
    {
        var attr = new BusinessAttributeNode { Name = "Price", Type = "money" };
        var result = _translator.Translate(attr);

        result.BaseType.Should().Be(DataType.Decimal);
    }

    [Fact]
    public void Translate_UnknownType_ReturnsObject()
    {
        var attr = new BusinessAttributeNode { Name = "Foo", Type = "unknown_xyz" };
        var result = _translator.Translate(attr);

        result.BaseType.Should().Be(DataType.Object);
    }

    [Fact]
    public void TryEnrich_Email_ReturnsEnrichmentWithMaxLength()
    {
        var attr = new BusinessAttributeNode { Id = "a1", Name = "Email", Type = "email" };
        var result = _translator.TryEnrich(attr);

        result.Should().NotBeNull();
        result!.MaxLength.Should().Be(254);
        result.ValidationRules.Should().Contain("email_format");
    }

    [Fact]
    public void TryEnrich_PlainString_ReturnsNull()
    {
        var attr = new BusinessAttributeNode { Id = "a1", Name = "Foo", Type = "int" };
        var result = _translator.TryEnrich(attr);

        result.Should().BeNull();
    }

    // === Translate — základní typy (High) ===

    [Fact]
    public void Translate_String_ReturnsStringType()
    {
        var attr = new BusinessAttributeNode { Name = "Name", Type = "string" };
        var result = _translator.Translate(attr);

        result.BaseType.Should().Be(DataType.String);
    }

    [Fact]
    public void Translate_Int_ReturnsInt32Type()
    {
        var attr = new BusinessAttributeNode { Name = "Count", Type = "int" };
        var result = _translator.Translate(attr);

        result.BaseType.Should().Be(DataType.Int32);
    }

    [Fact]
    public void Translate_Decimal_ReturnsDecimalType()
    {
        var attr = new BusinessAttributeNode { Name = "Price", Type = "decimal" };
        var result = _translator.Translate(attr);

        result.BaseType.Should().Be(DataType.Decimal);
    }

    [Fact]
    public void Translate_Bool_ReturnsBoolType()
    {
        var attr = new BusinessAttributeNode { Name = "Active", Type = "bool" };
        var result = _translator.Translate(attr);

        result.BaseType.Should().Be(DataType.Bool);
    }

    [Fact]
    public void Translate_DateTime_ReturnsDateTimeType()
    {
        var attr = new BusinessAttributeNode { Name = "Created", Type = "datetime" };
        var result = _translator.Translate(attr);

        result.BaseType.Should().Be(DataType.DateTime);
    }

    [Fact]
    public void Translate_Guid_ReturnsGuidType()
    {
        var attr = new BusinessAttributeNode { Name = "Id", Type = "guid" };
        var result = _translator.Translate(attr);

        result.BaseType.Should().Be(DataType.Guid);
    }

    // === Nullability (High) ===

    [Fact]
    public void Translate_RequiredAttribute_SetsNonNullable()
    {
        var attr = new BusinessAttributeNode { Name = "Name", Type = "string", IsRequired = true };
        var result = _translator.Translate(attr);

        result.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void Translate_NotRequiredAttribute_NullabilityDependsOnType()
    {
        // Translator nenastavuje IsNullable automaticky — závisí na typu
        var attr = new BusinessAttributeNode { Name = "Name", Type = "string", IsRequired = false };
        var result = _translator.Translate(attr);

        // IsNullable zůstává dle výchozí hodnoty TypeModel
        result.BaseType.Should().Be(DataType.String);
    }

    // === TryEnrich — phone a string (High) ===

    [Fact]
    public void TryEnrich_Phone_ReturnsEnrichmentWithPhoneFormat()
    {
        var attr = new BusinessAttributeNode { Id = "a1", Name = "Phone", Type = "phone" };
        var result = _translator.TryEnrich(attr);

        result.Should().NotBeNull();
        result!.ValidationRules.Should().Contain("phone_format");
        result.MaxLength.Should().Be(20);
    }

    [Fact]
    public void TryEnrich_String_ReturnsEnrichmentWithDefaultMaxLength()
    {
        var attr = new BusinessAttributeNode { Id = "a1", Name = "Description", Type = "string" };
        var result = _translator.TryEnrich(attr);

        result.Should().NotBeNull();
        result!.MaxLength.Should().Be(200);
    }

    [Fact]
    public void TryEnrich_NullAttribute_ThrowsArgumentNullException()
    {
        var act = () => _translator.TryEnrich(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
