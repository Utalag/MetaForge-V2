// ---------------------------------------------------------------------------
// MetaForge.Core — ReferenceGraph
// Oriented dependency graph between RootElements for cycle detection and sort.
// Vrstva: Core / ReferenceGraph
//
// PROPOSAL: PROP-055 — ReferenceGraph (ID-based)
// DEPENDS: PROP-060 — ElementIdMapping for Guid-based resolution
// ---------------------------------------------------------------------------

using MetaForge.Core.Abstractions;
using MetaForge.Core.Diagnostics;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Types;

namespace MetaForge.Core.ReferenceGraph;

/// <summary>
/// Orientovaný graf závislostí mezi <see cref="RootElement"/>.
/// Detekuje cykly, nevyřešené reference, a poskytuje topologické řazení.
/// Používá Guid ID-based model (PROP-060) — reference jsou stabilní i při přejmenování.
/// </summary>
public sealed class ReferenceGraph
{
    private readonly Dictionary<Guid, ReferenceGraphNode> _nodes;
    private readonly Dictionary<Guid, string> _idToName;

    private ReferenceGraph(
        Dictionary<Guid, ReferenceGraphNode> nodes,
        Dictionary<Guid, string> idToName,
        IReadOnlyList<ReferenceCycle> cycles,
        IReadOnlyList<UnresolvedReference> unresolved,
        IReadOnlyList<RootElement> sortedElements)
    {
        _nodes = nodes;
        _idToName = idToName;
        Cycles = cycles;
        Unresolved = unresolved;
        SortedElements = sortedElements;
    }

    /// <summary>Nalezené cykly (prázdné = žádné).</summary>
    public IReadOnlyList<ReferenceCycle> Cycles { get; }

    /// <summary>Nevyřešené reference (prázdné = vše resolvnuto).</summary>
    public IReadOnlyList<UnresolvedReference> Unresolved { get; }

    /// <summary>Topologicky seřazené elementy pro generování.</summary>
    public IReadOnlyList<RootElement> SortedElements { get; }

    /// <summary>Počet uzlů v grafu.</summary>
    public int NodeCount => _nodes.Count;

    /// <summary>Počet hran (referencí) v grafu.</summary>
    public int EdgeCount => _nodes.Values.Sum(n => n.References.Count);

    /// <summary>Přístup k uzlu podle ID.</summary>
    public ReferenceGraphNode? GetNode(Guid elementId)
        => _nodes.GetValueOrDefault(elementId);

    /// <summary>Vrátí DisplayName pro dané Guid (pro diagnostiku).</summary>
    public string? GetDisplayName(Guid elementId)
        => _idToName.GetValueOrDefault(elementId);

    // ========================================================================
    // Factory
    // ========================================================================

    /// <summary>
    /// Sestaví graf z elementů a ID mappingu.
    /// Extrahuje reference, detekuje cykly, najde nevyřešené.
    /// Všechny problémy zapisuje rovnou do <paramref name="diagnostics"/>.
    /// </summary>
    public static ReferenceGraph Build(
        IEnumerable<RootElement> elements,
        IElementIdResolver idResolver,
        DiagnosticBag diagnostics)
    {
        ArgumentNullException.ThrowIfNull(elements);
        ArgumentNullException.ThrowIfNull(idResolver);
        ArgumentNullException.ThrowIfNull(diagnostics);

        var nodes = new Dictionary<Guid, ReferenceGraphNode>();
        var idToName = new Dictionary<Guid, string>();
        var unresolvedList = new List<UnresolvedReference>();

        // Fáze 1: Vytvořit uzly
        foreach (var element in elements)
        {
            var node = new ReferenceGraphNode
            {
                ElementId = element.Id,
                DisplayName = element.Name ?? "(unnamed)",
                ElementKind = element.GetType().Name,
                Element = element,
                References = [],
            };
            nodes[element.Id] = node;
            idToName[element.Id] = element.Name ?? "(unnamed)";
        }

        // Fáze 2: Extrahovat reference
        foreach (var element in elements)
        {
            var references = ExtractReferences(element, idResolver, nodes, idToName, unresolvedList);
            var node = nodes[element.Id] with { References = references };
            nodes[element.Id] = node;
        }

        // Fáze 3: Topologické řazení + detekce cyklů
        var cycles = TopologicalSort(nodes, idToName);

        // Fáze 4: Reportovat do DiagnosticBag
        foreach (var cycle in cycles)
        {
            diagnostics.Report(new Diagnostic(
                "REF001",
                $"Cyklus: {cycle}",
                DiagnosticSeverity.Error,
                new ElementPath("", "")));
        }
        foreach (var unresolved in unresolvedList)
        {
            diagnostics.Report(new Diagnostic(
                "REF002",
                $"Nevyřešeno: {unresolved.ReferencedAs} ({unresolved.SourceDisplayName}) — typ nenalezen",
                DiagnosticSeverity.Warning,
                new ElementPath("", "")));
        }

        // Fáze 5: Seřadit elementy
        var sortedElements = SortElements(elements, nodes, idToName);

        return new ReferenceGraph(nodes, idToName, cycles, unresolvedList, sortedElements);
    }

    // ========================================================================
    // Referenční extrakce
    // ========================================================================

    private static IReadOnlyList<Guid> ExtractReferences(
        RootElement element,
        IElementIdResolver idResolver,
        Dictionary<Guid, ReferenceGraphNode> nodes,
        Dictionary<Guid, string> idToName,
        List<UnresolvedReference> unresolvedList)
    {
        var refs = new List<Guid>();
        var displayName = element.Name ?? "(unnamed)";

        // ClassElement specifické
        if (element is ClassElement cls)
        {
            AddRef(cls.BaseClassName, element.Id, displayName, ReferenceKind.Inheritance,
                "BaseClass", idResolver, nodes, refs, unresolvedList);
            foreach (var iface in cls.ImplementedInterfaces)
                AddRef(iface, element.Id, displayName, ReferenceKind.InterfaceImplementation,
                    $"Interface '{iface}'", idResolver, nodes, refs, unresolvedList);
            ExtractMemberRefs(cls.Properties, element.Id, displayName, idResolver, nodes, refs, unresolvedList);
        }

        // StructElement může mít fields
        if (element is StructElement st)
        {
            ExtractMemberRefs(st.Properties, element.Id, displayName, idResolver, nodes, refs, unresolvedList);
        }

        // Obecné: extrahovat z property, method, field typů
        ExtractTypeRefsFromElement(element, idResolver, element.Id, displayName, nodes, refs, unresolvedList);

        return refs;
    }

    private static void ExtractMemberRefs(
        IEnumerable<PropertyElement> properties,
        Guid sourceId,
        string sourceName,
        IElementIdResolver idResolver,
        Dictionary<Guid, ReferenceGraphNode> nodes,
        List<Guid> refs,
        List<UnresolvedReference> unresolvedList)
    {
        foreach (var prop in properties)
        {
            if (prop.Type.CustomTypeName is { } typeName)
            {
                AddRef(typeName, sourceId, sourceName, ReferenceKind.PropertyType,
                    $"Property '{prop.Name}'", idResolver, nodes, refs, unresolvedList);
            }
        }
    }

    private static void ExtractTypeRefsFromElement(
        RootElement element,
        IElementIdResolver idResolver,
        Guid sourceId,
        string sourceName,
        Dictionary<Guid, ReferenceGraphNode> nodes,
        List<Guid> refs,
        List<UnresolvedReference> unresolvedList)
    {
        // Metody
        if (element is ClassElement cls)
        {
            foreach (var method in cls.Methods)
            {
                if (method.ReturnType.CustomTypeName is { } typeName)
                    AddRef(typeName, sourceId, sourceName, ReferenceKind.MethodReturn,
                        $"Method '{method.Name}' return", idResolver, nodes, refs, unresolvedList);
                foreach (var param in method.Parameters)
                {
                    if (param.Type.CustomTypeName is { } paramTypeName)
                        AddRef(paramTypeName, sourceId, sourceName, ReferenceKind.ParameterType,
                            $"Method '{method.Name}' param '{param.Name}'", idResolver, nodes, refs, unresolvedList);
                }
            }
        }
    }

    private static void AddRef(
        string customTypeName,
        Guid sourceId,
        string sourceName,
        ReferenceKind kind,
        string referencedAs,
        IElementIdResolver idResolver,
        Dictionary<Guid, ReferenceGraphNode> nodes,
        List<Guid> refs,
        List<UnresolvedReference> unresolvedList)
    {
        // Přeskočit primitiva a vestavěné typy
        if (IsPrimitive(customTypeName)) return;

        var resolvedId = idResolver.Resolve(customTypeName);
        if (resolvedId is not null && nodes.ContainsKey(resolvedId.Value))
        {
            refs.Add(resolvedId.Value);
        }
        else
        {
            unresolvedList.Add(new UnresolvedReference
            {
                SourceElementId = sourceId,
                SourceDisplayName = sourceName,
                TargetId = resolvedId ?? Guid.Empty,
                ReferencedAs = referencedAs,
                Kind = kind,
            });
        }
    }

    private static bool IsPrimitive(string typeName) => typeName.ToLowerInvariant() switch
    {
        "string" or "int" or "int32" or "long" or "int64" or "decimal" or "double"
            or "float" or "bool" or "boolean" or "datetime" or "guid" or "object"
            or "void" or "char" or "byte" or "short" or "ushort" or "uint" or "ulong"
            or "sbyte" => true,
        _ => false,
    };

    // ========================================================================
    // Kahnův algoritmus — topological sort + detekce cyklů
    // ========================================================================

    private static IReadOnlyList<ReferenceCycle> TopologicalSort(
        Dictionary<Guid, ReferenceGraphNode> nodes,
        Dictionary<Guid, string> idToName)
    {
        // Vypočítat in-degree
        var inDegree = new Dictionary<Guid, int>();
        foreach (var (id, _) in nodes)
            inDegree[id] = 0;
        foreach (var (_, node) in nodes)
        {
            foreach (var refId in node.References)
            {
                if (inDegree.ContainsKey(refId))
                    inDegree[refId]++;
            }
        }

        // Kahn BFS
        var queue = new Queue<Guid>();
        foreach (var (id, degree) in inDegree)
            if (degree == 0)
                queue.Enqueue(id);

        var visited = new HashSet<Guid>();
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            visited.Add(current);
            if (nodes.TryGetValue(current, out var node))
            {
                foreach (var refId in node.References)
                {
                    if (inDegree.ContainsKey(refId))
                    {
                        inDegree[refId]--;
                        if (inDegree[refId] == 0)
                            queue.Enqueue(refId);
                    }
                }
            }
        }

        // Zbylé uzly = cykly
        var remaining = nodes.Keys.Where(k => !visited.Contains(k)).ToList();
        if (remaining.Count == 0)
            return [];

        // Detekovat cykly pomocí DFS
        return DetectCyclesInRemaining(remaining, nodes, idToName);
    }

    private static IReadOnlyList<ReferenceCycle> DetectCyclesInRemaining(
        List<Guid> remaining,
        Dictionary<Guid, ReferenceGraphNode> nodes,
        Dictionary<Guid, string> idToName)
    {
        var cycles = new List<ReferenceCycle>();
        var visited = new HashSet<Guid>();
        var stack = new List<Guid>();

        foreach (var startId in remaining)
        {
            if (visited.Contains(startId)) continue;
            FindCycle(startId, nodes, idToName, visited, stack, cycles);
        }

        return cycles;
    }

    private static void FindCycle(
        Guid current,
        Dictionary<Guid, ReferenceGraphNode> nodes,
        Dictionary<Guid, string> idToName,
        HashSet<Guid> visited,
        List<Guid> stack,
        List<ReferenceCycle> cycles)
    {
        if (stack.Contains(current))
        {
            // Cyklus nalezen
            var cycleStart = stack.IndexOf(current);
            var cycleIds = stack.Skip(cycleStart).Append(current).ToList();
            var cycleNames = cycleIds.Select(id => idToName.GetValueOrDefault(id, id.ToString())).ToList();
            cycles.Add(new ReferenceCycle
            {
                ElementIds = cycleIds,
                DisplayNames = cycleNames,
            });
            return;
        }

        if (visited.Contains(current)) return;

        visited.Add(current);
        stack.Add(current);

        if (nodes.TryGetValue(current, out var node))
        {
            foreach (var refId in node.References)
            {
                FindCycle(refId, nodes, idToName, visited, stack, cycles);
            }
        }

        stack.RemoveAt(stack.Count - 1);
    }

    // ========================================================================
    // Seřazení elementů
    // ========================================================================

    private static IReadOnlyList<RootElement> SortElements(
        IEnumerable<RootElement> elements,
        Dictionary<Guid, ReferenceGraphNode> nodes,
        Dictionary<Guid, string> idToName)
    {
        // Vypočítat in-degree
        var inDegree = new Dictionary<Guid, int>();
        foreach (var (id, _) in nodes)
            inDegree[id] = 0;
        foreach (var (_, node) in nodes)
        {
            foreach (var refId in node.References)
            {
                if (inDegree.ContainsKey(refId))
                    inDegree[refId]++;
            }
        }

        var queue = new Queue<Guid>();
        foreach (var (id, degree) in inDegree)
            if (degree == 0)
                queue.Enqueue(id);

        var sorted = new List<RootElement>();
        var elementLookup = elements.ToDictionary(e => e.Id);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (elementLookup.TryGetValue(current, out var element))
                sorted.Add(element);

            if (nodes.TryGetValue(current, out var node))
            {
                foreach (var refId in node.References)
                {
                    if (inDegree.ContainsKey(refId))
                    {
                        inDegree[refId]--;
                        if (inDegree[refId] == 0)
                            queue.Enqueue(refId);
                    }
                }
            }
        }

        return sorted;
    }

    // ========================================================================
    // Vrstvy — volitelné rozšíření
    // ========================================================================

    /// <summary>
    /// Rozdělí seřazené elementy do vrstev podle hloubky závislostí.
    /// Vrstva 0: elementy bez závislostí. Vrstva N: závisí na Vrstvě N-1.
    /// </summary>
    public IReadOnlyList<IReadOnlyList<RootElement>> GetLayers()
    {
        if (SortedElements.Count == 0) return [];

        var layerMap = new Dictionary<Guid, int>();
        foreach (var (id, _) in _nodes)
            layerMap[id] = 0;

        // Vypočítat hloubku: max hloubka souseda + 1
        var changed = true;
        while (changed)
        {
            changed = false;
            foreach (var (id, node) in _nodes)
            {
                foreach (var refId in node.References)
                {
                    if (layerMap.ContainsKey(refId))
                    {
                        var newDepth = layerMap[id] + 1;
                        if (newDepth > layerMap[refId])
                        {
                            layerMap[refId] = newDepth;
                            changed = true;
                        }
                    }
                }
            }
        }

        var maxDepth = layerMap.Values.DefaultIfEmpty(0).Max();
        var layers = new List<List<RootElement>>();
        for (int d = 0; d <= maxDepth; d++)
        {
            layers.Add(new List<RootElement>());
        }

        var elementLookup = SortedElements.ToDictionary(e => e.Id);
        foreach (var (id, depth) in layerMap)
        {
            if (elementLookup.TryGetValue(id, out var element))
                layers[depth].Add(element);
        }

        return layers.Where(l => l.Count > 0).Select(l => (IReadOnlyList<RootElement>)l.AsReadOnly()).ToList();
    }
}
