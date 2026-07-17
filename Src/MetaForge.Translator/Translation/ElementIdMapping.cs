// ---------------------------------------------------------------------------
// MetaForge.Translator — ElementIdMapping
// Maps BusinessModel string IDs → Core Guid IDs for cross-layer traceability.
// Vrstva: Translator / Translation
//
// PROPOSAL: PROP-060 — Element Identity Stabilization
// ---------------------------------------------------------------------------

using MetaForge.BusinessModel.Models;
using MetaForge.Core.Abstractions;

namespace MetaForge.Translator.Translation;

/// <summary>
/// Uchovává mapování BusinessModel ID → Core ID.
/// Vytvořeno během TranslateDocument(), použito pro traceabilitu, projekce (PROP-056),
/// a referenční graf (PROP-055).
/// </summary>
public sealed class ElementIdMapping : IElementIdResolver
{
    private readonly Dictionary<string, Guid> _entityIds = new();
    private readonly Dictionary<string, Guid> _attributeIds = new();
    private readonly Dictionary<string, Guid> _behaviorIds = new();
    private readonly Dictionary<string, Guid> _parameterIds = new();

    /// <summary>Počet namapovaných entit.</summary>
    public int EntityCount => _entityIds.Count;

    /// <summary>Počet namapovaných atributů.</summary>
    public int AttributeCount => _attributeIds.Count;

    /// <summary>Počet namapovaných chování.</summary>
    public int BehaviorCount => _behaviorIds.Count;

    /// <summary>Počet namapovaných parametrů.</summary>
    public int ParameterCount => _parameterIds.Count;

    /// <summary>Celkový počet namapovaných elementů.</summary>
    public int TotalCount => EntityCount + AttributeCount + BehaviorCount + ParameterCount;

    /// <summary>
    /// Namapuje BusinessEntityNode.Id → ClassElement.Id (Guid).
    /// </summary>
    public void MapEntity(BusinessEntityNode entity, Guid coreId)
    {
        _entityIds[entity.Id] = coreId;
    }

    /// <summary>
    /// Namapuje BusinessAttributeNode.Id → PropertyElement.Id (Guid).
    /// </summary>
    public void MapAttribute(BusinessAttributeNode attribute, Guid coreId)
    {
        _attributeIds[attribute.Id] = coreId;
    }

    /// <summary>
    /// Namapuje BusinessBehaviorNode.Id → MethodElement.Id (Guid).
    /// </summary>
    public void MapBehavior(BusinessBehaviorNode behavior, Guid coreId)
    {
        _behaviorIds[behavior.Id] = coreId;
    }

    /// <summary>
    /// Namapuje BusinessParameterNode.Id → ParameterElement.Id (Guid).
    /// </summary>
    public void MapParameter(BusinessParameterNode parameter, Guid coreId)
    {
        _parameterIds[parameter.Id] = coreId;
    }

    /// <summary>
    /// Vyhledá Core Guid podle BusinessModel string ID.
    /// Prohledává všechny slovníky. Vrací null, pokud ID není namapováno.
    /// </summary>
    public Guid? Resolve(string businessId)
    {
        if (_entityIds.TryGetValue(businessId, out var id)) return id;
        if (_attributeIds.TryGetValue(businessId, out id)) return id;
        if (_behaviorIds.TryGetValue(businessId, out id)) return id;
        if (_parameterIds.TryGetValue(businessId, out id)) return id;
        return null;
    }

    /// <summary>
    /// Ověří konzistenci mapování — žádné duplicitní Core ID.
    /// </summary>
    public bool IsConsistent()
    {
        var allCoreIds = new HashSet<Guid>();
        foreach (var id in _entityIds.Values)
            if (!allCoreIds.Add(id)) return false;
        foreach (var id in _attributeIds.Values)
            if (!allCoreIds.Add(id)) return false;
        foreach (var id in _behaviorIds.Values)
            if (!allCoreIds.Add(id)) return false;
        foreach (var id in _parameterIds.Values)
            if (!allCoreIds.Add(id)) return false;
        return true;
    }
}
