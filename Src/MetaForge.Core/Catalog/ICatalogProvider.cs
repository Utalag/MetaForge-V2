namespace MetaForge.Core.Catalog;

/// <summary>
/// Poskytovatel presetů — umožňuje více zdrojů (built-in, filesystem, marketplace).
/// </summary>
public interface ICatalogProvider
{
    /// <summary>Název providera pro logování.</summary>
    string ProviderName { get; }

    /// <summary>Vrátí všechny presety z tohoto providera.</summary>
    IReadOnlyList<PresetDefinition> GetAllPresets();

    /// <summary>Vyhledá preset podle názvu. Vrací null pokud nenajde.</summary>
    PresetDefinition? ResolveType(string typeName);
}
