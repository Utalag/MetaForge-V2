using MetaForge.BusinessModel.Models;
using MetaForge.Core.DataTypes;

namespace MetaForge.Translator.Host;

/// <summary>
/// Projekce business modelu pro čtení — obsahuje přeložené entity a atributy.
/// </summary>
public sealed class ProjectionView
{
    /// <summary>Název projektu.</summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>Strukturované info o projektu (pokud je k dispozici).</summary>
    public ProjectInfoView? Project { get; set; }

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

        /// <summary>CoreDetail informace, pokud je k dispozici.</summary>
        public CoreDetailInfoView? CoreDetail { get; set; }
    }

    /// <summary>Projekce BusinessProjectInfo.</summary>
    public sealed class ProjectInfoView
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public int Version { get; set; }
    }

    /// <summary>Projekce BusinessAttributeCoreDetail pro čtení.</summary>
    public sealed class CoreDetailInfoView
    {
        public CoreInfoSource Source { get; set; }
        public string? ResolvedPresetId { get; set; }
        public string? ValueObjectName { get; set; }
        public bool IsStrongType { get; set; }
        public DateTimeOffset? LastSyncedAt { get; set; }
        public AttributeSyncState SyncState { get; set; }
    }

    public List<EntityProjection> Entities { get; } = new();
}
