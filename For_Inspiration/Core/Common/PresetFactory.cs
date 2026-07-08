using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Modifiers;
using MetaForge.Core.Elements.Primitives;
using MetaForge.Core.Elements.Types;
using MetaForge.Core.ValueObjects;

namespace MetaForge.Core.Common;

/// <summary>
/// Preset Factory - inteligentní továrna na validní metadata.
/// Implementuje Conflict Management a Logickou Integritu.
/// </summary>
public static class PresetFactory
{
    /// <summary>
    /// Vytvoří preset pro jednoduchý POCO model.
    /// </summary>
    public static Class CreatePocoModel(string name, string namespaceName)
    {
        var cls = new Class
        {
            Name = name,
            Namespace = namespaceName,
            AccessModifier = AccessModifier.Public,
            CreditScore = 10 // Základní skóre pro POCO
        };

        // Přidej základní usings
        cls.Usings.Add("System");
        cls.Usings.Add("System.Collections.Generic");

        return cls;
    }

    /// <summary>
    /// Vytvoří preset pro doménový model s ID property.
    /// </summary>
    public static Class CreateDomainModel(string name, string namespaceName)
    {
        var cls = CreatePocoModel(name, $"{namespaceName}.Domain");
        cls.CreditScore = 20; // Vyšší skóre pro doménový model

        // Přidej ID property
        var idProperty = new Property
        {
            Name = "Id",
            StrongType = new StrongType { UnderlyingType = new TypeModel { BaseType = DataType.Guid, TargetLanguage = cls.TargetLanguage } },
            AccessModifier = AccessModifier.Public,
            HasGetter = true,
            HasSetter = true
        };

        cls.Properties.Add(idProperty);

        return cls;
    }

    /// <summary>
    /// Vytvoří preset pro DTO (Data Transfer Object).
    /// </summary>
    public static Class CreateDto(string name, string namespaceName)
    {
        var cls = CreatePocoModel(name, $"{namespaceName}.Contracts");
        cls.CreditScore = 15;

        // DTOs jsou často sealed
        cls.IsSealed = true;

        return cls;
    }

    /// <summary>
    /// Vytvoří preset pro Repository interface.
    /// </summary>
    public static Class CreateRepositoryInterface(string entityName, string namespaceName)
    {
        var cls = new Class
        {
            Name = $"I{entityName}Repository",
            Namespace = $"{namespaceName}.Repositories",
            AccessModifier = AccessModifier.Public,
            CreditScore = 25
        };

        cls.Usings.Add("System");
        cls.Usings.Add("System.Collections.Generic");
        cls.Usings.Add("System.Threading.Tasks");

        // Poznámka: Pro interface bychom potřebovali samostatnou třídu Interface
        // Pro nyní použijeme Class s abstract metodami

        return cls;
    }

    /// <summary>
    /// Vytvoří preset pro statickou utility třídu.
    /// </summary>
    public static Class CreateStaticUtility(string name, string namespaceName)
    {
        var cls = new Class
        {
            Name = name,
            Namespace = namespaceName,
            AccessModifier = AccessModifier.Public,
            IsStatic = true, // Automaticky vyřeší konflikty s abstract/sealed
            CreditScore = 15
        };

        cls.Usings.Add("System");

        return cls;
    }

    /// <summary>
    /// Vytvoří preset pro partial třídu (pro rozšiřování stávajícího kódu).
    /// </summary>
    public static Class CreatePartialExtension(string name, string namespaceName)
    {
        var cls = new Class
        {
            Name = name,
            Namespace = namespaceName,
            AccessModifier = AccessModifier.Public,
            IsPartial = true, // Bezpečné rozšiřování
            CreditScore = 5 // Nižší skóre, protože rozšiřuje existující
        };

        return cls;
    }

    /// <summary>
    /// Přidá standardní property do třídy s automatickou validací.
    /// </summary>
    public static Property AddProperty(Class cls, string name, DataType type, bool isNullable = false)
    {
        var property = new Property
        {
            Name = name,
            StrongType = new StrongType { UnderlyingType = new TypeModel { BaseType = type, IsNullable = isNullable, TargetLanguage = cls.TargetLanguage } },
            AccessModifier = AccessModifier.Public,
            HasGetter = true,
            HasSetter = true
        };

        cls.Properties.Add(property);
        return property;
    }

    /// <summary>
    /// Přidá private readonly field do třídy.
    /// </summary>
    public static Field AddPrivateField(Class cls, string name, DataType type)
    {
        var field = new Field
        {
            Name = $"_{name}",
            StrongType = new StrongType { UnderlyingType = new TypeModel { BaseType = type, TargetLanguage = cls.TargetLanguage } },
            AccessModifier = AccessModifier.Private,
            IsReadOnly = true
        };

        cls.Fields.Add(field);
        return field;
    }

    /// <summary>
    /// Přidá property s inline backing fieldem.
    /// Typ a jazyk backing fieldu se automaticky synchronizují z property.
    /// Backing field se automaticky přidá do Fields kolekce třídy.
    /// </summary>
    public static Property AddPropertyWithBackingField(
        Class cls,
        string name,
        DataType type,
        bool isReadOnly = false,
        bool isNullable = false,
        string? backingFieldName = null)
    {
        var property = new Property
        {
            Name = name,
            StrongType = new StrongType { UnderlyingType = new TypeModel { BaseType = type, IsNullable = isNullable, TargetLanguage = cls.TargetLanguage } },
            AccessModifier = AccessModifier.Public,
            HasGetter = true,
            HasSetter = !isReadOnly,
            BackingField = new Field
            {
                Name = backingFieldName ?? $"_{char.ToLower(name[0])}{name[1..]}",
                AccessModifier = AccessModifier.Private,
                IsReadOnly = isReadOnly
            }
        };

        // BackingField se automaticky přidá do Fields přes CollectionChanged
        cls.Properties.Add(property);
        return property;
    }
}
