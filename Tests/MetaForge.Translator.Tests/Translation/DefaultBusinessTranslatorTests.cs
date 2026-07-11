using FluentAssertions;
using MetaForge.BusinessModel.Models;
using MetaForge.Core.Catalog;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Types;
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

    // === TranslateDocument — Strong Type Mapping (PROP-047) ===

    [Fact]
    public void TranslateDocument_WithStrongType_CreatesValueObjectElement()
    {
        // Arrange
        var ctd = new CustomTypeDefinition
        {
            Id = "phone-id",
            Name = "PhoneNumber",
            BaseType = "string",
            ValidationRules = ["not_empty", "phone_format"]
        };

        var attr = new BusinessAttributeNode
        {
            Id = "a1",
            Name = "Phone",
            Type = "string",
            CoreDetail = new BusinessAttributeCoreDetail
            {
                IsStrongType = true,
                ValueObjectName = "PhoneNumber",
            }
        };

        var entity = new BusinessEntityNode
        {
            Id = "e1",
            Name = "Customer",
            Attributes = [attr],
        };

        var doc = new BusinessAuthoringDocument
        {
            ProjectName = "Test",
            Entities = [entity],
            CustomTypes = [ctd],
        };

        // Act
        var result = _translator.TranslateDocument(doc);

        // Assert
        result.Should().HaveCount(1);
        var cls = result[0].Should().BeOfType<ClassElement>().Subject;
        cls.Name.Should().Be("Customer");
        cls.InlineStrongTypes.Should().HaveCount(1);

        var vo = cls.InlineStrongTypes[0].Should().BeOfType<ValueObjectElement>().Subject;
        vo.Name.Should().Be("PhoneNumber");
        vo.IsReadOnly.Should().BeTrue();

        // Property odkazuje na strong type
        cls.Properties.Should().HaveCount(1);
        cls.Properties[0].Name.Should().Be("Phone");
        cls.Properties[0].Type.CustomTypeName.Should().Be("PhoneNumber");
        cls.Properties[0].Type.BaseType.Should().Be(DataType.Struct);

        // Translation source metadata
        cls.Metadata.Get<string>("Generation.TranslationSource").Should().Be("Deterministic");
    }

    [Fact]
    public void TranslateDocument_WithoutCoreDetail_FallsBackToPrimitive()
    {
        // Arrange
        var attr = new BusinessAttributeNode
        {
            Id = "a1",
            Name = "Email",
            Type = "string",
            // CoreDetail je null
        };

        var entity = new BusinessEntityNode
        {
            Id = "e1",
            Name = "Customer",
            Attributes = [attr],
        };

        var doc = new BusinessAuthoringDocument
        {
            ProjectName = "Test",
            Entities = [entity],
        };

        // Act
        var result = _translator.TranslateDocument(doc);

        // Assert
        var cls = result[0].Should().BeOfType<ClassElement>().Subject;
        cls.InlineStrongTypes.Should().BeEmpty();
        cls.Properties.Should().HaveCount(1);
        cls.Properties[0].Type.BaseType.Should().Be(DataType.String);
    }

    [Fact]
    public void TranslateDocument_WithIsStrongTypeFalse_FallsBackToPrimitive()
    {
        // Arrange
        var attr = new BusinessAttributeNode
        {
            Id = "a1",
            Name = "Name",
            Type = "string",
            CoreDetail = new BusinessAttributeCoreDetail
            {
                IsStrongType = false,
            }
        };

        var entity = new BusinessEntityNode
        {
            Id = "e1",
            Name = "Customer",
            Attributes = [attr],
        };

        var doc = new BusinessAuthoringDocument
        {
            ProjectName = "Test",
            Entities = [entity],
        };

        // Act
        var result = _translator.TranslateDocument(doc);

        // Assert
        var cls = result[0].Should().BeOfType<ClassElement>().Subject;
        cls.InlineStrongTypes.Should().BeEmpty();
        cls.Properties.Should().HaveCount(1);
        cls.Properties[0].Type.BaseType.Should().Be(DataType.String);
    }

    [Fact]
    public void TranslateDocument_StrongTypeWithoutCustomType_FallsBackToPrimitive()
    {
        // Arrange — CoreDetail.IsStrongType=true, ale CustomTypeDefinition chybí
        var attr = new BusinessAttributeNode
        {
            Id = "a1",
            Name = "Phone",
            Type = "string",
            CoreDetail = new BusinessAttributeCoreDetail
            {
                IsStrongType = true,
                ValueObjectName = "MissingType",
            }
        };

        var entity = new BusinessEntityNode
        {
            Id = "e1",
            Name = "Customer",
            Attributes = [attr],
        };

        var doc = new BusinessAuthoringDocument
        {
            ProjectName = "Test",
            Entities = [entity],
            CustomTypes = [], // prázdné
        };

        // Act
        var result = _translator.TranslateDocument(doc);

        // Assert
        var cls = result[0].Should().BeOfType<ClassElement>().Subject;
        cls.InlineStrongTypes.Should().BeEmpty();
        cls.Properties.Should().HaveCount(1);
        cls.Properties[0].Type.BaseType.Should().Be(DataType.String);
    }

    [Fact]
    public void TranslateDocument_MultipleEntities_EachGetsItsOwnClassElement()
    {
        // Arrange
        var attr1 = new BusinessAttributeNode { Id = "a1", Name = "Name", Type = "string" };
        var attr2 = new BusinessAttributeNode { Id = "a2", Name = "Price", Type = "decimal" };

        var doc = new BusinessAuthoringDocument
        {
            ProjectName = "Test",
            Entities =
            [
                new BusinessEntityNode { Id = "e1", Name = "Customer", Attributes = [attr1] },
                new BusinessEntityNode { Id = "e2", Name = "Product", Attributes = [attr2] },
            ],
        };

        // Act
        var result = _translator.TranslateDocument(doc);

        // Assert
        result.Should().HaveCount(2);
        result[0].Should().BeOfType<ClassElement>().Subject.Name.Should().Be("Customer");
        result[1].Should().BeOfType<ClassElement>().Subject.Name.Should().Be("Product");
    }
}
