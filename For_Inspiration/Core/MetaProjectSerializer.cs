using System.Text.Json;
using System.Text.Json.Serialization;
using MetaForge.Core.Common;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Modifiers;
using MetaForge.Core.Elements.Primitives;
using MetaForge.Core.Elements.Types;
using MetaForge.Core.ValueObjects;

namespace MetaForge.Core;

/// <summary>
/// JSON serialization/deserialization pro MetaProject.
/// Používá System.Text.Json s camelCase konvencí.
/// </summary>
public static class MetaProjectSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializuje MetaProject do JSON stringu.
    /// </summary>
    public static string ToJson(MetaProject project)
    {
        var dto = ToDto(project);
        return JsonSerializer.Serialize(dto, JsonOptions);
    }

    /// <summary>
    /// Deserializuje MetaProject z JSON stringu.
    /// </summary>
    public static MetaProject FromJson(string json)
    {
        var dto = JsonSerializer.Deserialize<MetaProjectDto>(json, JsonOptions)
            ?? throw new JsonException("Failed to deserialize MetaProject JSON.");
        return FromDto(dto);
    }

    private static MetaProjectDto ToDto(MetaProject project) => new()
    {
        Name = project.Name,
        Description = project.Description,
        Icon = project.Icon,
        Version = project.Version,
        TargetLanguage = project.TargetLanguage.ToString(),
        IsCustomized = project.IsCustomized,
        Classes = project.Classes.Select(ToClassDto).ToList(),
        Enums = project.Enums.Select(ToEnumDto).ToList()
    };

    private static MetaProject FromDto(MetaProjectDto dto)
    {
        var lang = Enum.TryParse<ProgramLanguage>(dto.TargetLanguage, true, out var l)
            ? l : ProgramLanguage.CSharp;

        var project = new MetaProject
        {
            Name = dto.Name,
            Description = dto.Description,
            Icon = dto.Icon,
            Version = dto.Version,
            TargetLanguage = lang,
            IsCustomized = dto.IsCustomized
        };

        foreach (var c in dto.Classes)
            project.Classes.Add(FromClassDto(c, lang));

        foreach (var e in dto.Enums)
            project.Enums.Add(FromEnumDto(e, lang));

        return project;
    }

    #region Class DTO mapping

    private static ClassDto ToClassDto(Class cls) => new()
    {
        Name = cls.Name,
        Namespace = cls.Namespace,
        AccessModifier = cls.AccessModifier.ToString(),
        IsStatic = cls.IsStatic,
        IsAbstract = cls.IsAbstract,
        IsSealed = cls.IsSealed,
        IsPartial = cls.IsPartial,
        BaseClass = cls.BaseClass,
        IsCustomized = cls.IsCustomized,
        Interfaces = cls.Interfaces.ToList(),
        Properties = cls.Properties.Select(ToPropertyDto).ToList(),
        Fields = cls.Fields
            .Where(f => !cls.Properties.Any(p => p.BackingField == f))
            .Select(ToFieldDto).ToList()
    };

    private static Class FromClassDto(ClassDto dto, ProgramLanguage lang)
    {
        var cls = new Class
        {
            Name = dto.Name,
            Namespace = dto.Namespace ?? string.Empty,
            AccessModifier = Enum.TryParse<AccessModifier>(dto.AccessModifier, true, out var am)
                ? am : AccessModifier.Public,
            IsStatic = dto.IsStatic,
            IsAbstract = dto.IsAbstract,
            IsSealed = dto.IsSealed,
            IsPartial = dto.IsPartial,
            BaseClass = dto.BaseClass,
            IsCustomized = dto.IsCustomized,
            TargetLanguage = lang
        };

        foreach (var i in dto.Interfaces ?? [])
            cls.Interfaces.Add(i);

        foreach (var p in dto.Properties ?? [])
            cls.Properties.Add(FromPropertyDto(p, lang));

        foreach (var f in dto.Fields ?? [])
            cls.Fields.Add(FromFieldDto(f, lang));

        return cls;
    }

    #endregion

    #region Property DTO mapping

    private static PropertyDto ToPropertyDto(Property prop) => new()
    {
        Name = prop.Name,
        StrongType = prop.StrongType != null ? ToStrongTypeDto(prop.StrongType) : null,
        AccessModifier = prop.AccessModifier.ToString(),
        IsCustomized = prop.IsCustomized
    };

    private static Property FromPropertyDto(PropertyDto dto, ProgramLanguage lang)
    {
        var prop = new Property
        {
            Name = dto.Name,
            AccessModifier = Enum.TryParse<AccessModifier>(dto.AccessModifier, true, out var am)
                ? am : AccessModifier.Public,
            IsCustomized = dto.IsCustomized,
            TargetLanguage = lang
        };

        if (dto.StrongType != null)
            prop.StrongType = FromStrongTypeDto(dto.StrongType, lang);

        return prop;
    }

    #endregion

    #region Field DTO mapping

    private static FieldDto ToFieldDto(Field field) => new()
    {
        Name = field.Name,
        StrongType = field.StrongType != null ? ToStrongTypeDto(field.StrongType) : null,
        AccessModifier = field.AccessModifier.ToString(),
        IsCustomized = field.IsCustomized
    };

    private static Field FromFieldDto(FieldDto dto, ProgramLanguage lang)
    {
        var field = new Field
        {
            Name = dto.Name,
            AccessModifier = Enum.TryParse<AccessModifier>(dto.AccessModifier, true, out var am)
                ? am : AccessModifier.Public,
            IsCustomized = dto.IsCustomized,
            TargetLanguage = lang
        };

        if (dto.StrongType != null)
            field.StrongType = FromStrongTypeDto(dto.StrongType, lang);

        return field;
    }

    #endregion

    #region StrongType DTO mapping

    private static StrongTypeDto ToStrongTypeDto(StrongType st) => new()
    {
        Name = string.IsNullOrEmpty(st.Name) ? null : st.Name,
        UnderlyingType = st.UnderlyingType.BaseType.ToString()
    };

    private static StrongType FromStrongTypeDto(StrongTypeDto dto, ProgramLanguage lang)
    {
        var baseType = Enum.TryParse<DataType>(dto.UnderlyingType, true, out var dt)
            ? dt : DataType.String;

        return new StrongType
        {
            Name = dto.Name ?? string.Empty,
            UnderlyingType = new TypeModel { BaseType = baseType, TargetLanguage = lang }
        };
    }

    #endregion

    #region Enum DTO mapping

    private static EnumDto ToEnumDto(EnumMF enm) => new()
    {
        Name = enm.Name,
        Namespace = enm.Namespace,
        AccessModifier = enm.AccessModifier.ToString(),
        UnderlyingType = enm.UnderlyingType.ToString(),
        IsCustomized = enm.IsCustomized,
        Values = enm.Values.Select(v => new EnumValueDto
        {
            Name = v.Name,
            ExplicitValue = v.ExplicitValue,
            Description = string.IsNullOrEmpty(v.Description) ? null : v.Description
        }).ToList()
    };

    private static EnumMF FromEnumDto(EnumDto dto, ProgramLanguage lang)
    {
        var enm = new EnumMF
        {
            Name = dto.Name,
            Namespace = dto.Namespace ?? string.Empty,
            AccessModifier = Enum.TryParse<AccessModifier>(dto.AccessModifier, true, out var am)
                ? am : AccessModifier.Public,
            UnderlyingType = Enum.TryParse<DataType>(dto.UnderlyingType, true, out var dt)
                ? dt : DataType.Int,
            IsCustomized = dto.IsCustomized,
            TargetLanguage = lang
        };

        foreach (var v in dto.Values ?? [])
        {
            enm.Values.Add(new EnumValue
            {
                Name = v.Name,
                ExplicitValue = v.ExplicitValue,
                Description = v.Description ?? string.Empty
            });
        }

        return enm;
    }

    #endregion

    #region DTO classes

    private sealed class MetaProjectDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public int Version { get; set; } = 1;
        public string TargetLanguage { get; set; } = "CSharp";
        public bool IsCustomized { get; set; }
        public List<ClassDto> Classes { get; set; } = [];
        public List<EnumDto> Enums { get; set; } = [];
    }

    private sealed class ClassDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Namespace { get; set; }
        public string AccessModifier { get; set; } = "Public";
        public bool IsStatic { get; set; }
        public bool IsAbstract { get; set; }
        public bool IsSealed { get; set; }
        public bool IsPartial { get; set; }
        public string? BaseClass { get; set; }
        public bool IsCustomized { get; set; }
        public List<string>? Interfaces { get; set; }
        public List<PropertyDto>? Properties { get; set; }
        public List<FieldDto>? Fields { get; set; }
    }

    private sealed class PropertyDto
    {
        public string Name { get; set; } = string.Empty;
        public StrongTypeDto? StrongType { get; set; }
        public string AccessModifier { get; set; } = "Public";
        public bool IsCustomized { get; set; }
    }

    private sealed class FieldDto
    {
        public string Name { get; set; } = string.Empty;
        public StrongTypeDto? StrongType { get; set; }
        public string AccessModifier { get; set; } = "Public";
        public bool IsCustomized { get; set; }
    }

    private sealed class StrongTypeDto
    {
        public string? Name { get; set; }
        public string UnderlyingType { get; set; } = "String";
    }

    private sealed class EnumDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Namespace { get; set; }
        public string AccessModifier { get; set; } = "Public";
        public string UnderlyingType { get; set; } = "Int";
        public bool IsCustomized { get; set; }
        public List<EnumValueDto>? Values { get; set; }
    }

    private sealed class EnumValueDto
    {
        public string Name { get; set; } = string.Empty;
        public int? ExplicitValue { get; set; }
        public string? Description { get; set; }
    }

    #endregion
}
