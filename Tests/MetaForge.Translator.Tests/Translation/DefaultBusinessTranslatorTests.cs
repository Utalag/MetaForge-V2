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
}
