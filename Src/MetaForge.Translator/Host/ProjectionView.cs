using MetaForge.Core.DataTypes;

namespace MetaForge.Translator.Host;

/// <summary>
/// Projekce business modelu pro čtení — obsahuje přeložené entity a atributy.
/// </summary>
public sealed class ProjectionView
{
    /// <summary>Název projektu.</summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>Entita v projekci.</summary>
    public sealed class EntityProjection
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<AttributeProjection> Attributes { get; } = new();
    }

    /// <summary>Atribut v projekci — přeložený do Core typů.</summary>
    public sealed class AttributeProjection
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public TypeModel CoreType { get; set; } = TypeModel.Object;
        public bool IsRequired { get; set; }
        public int? MaxLength { get; set; }
        public string? DefaultValue { get; set; }
    }

    public List<EntityProjection> Entities { get; } = new();
}
