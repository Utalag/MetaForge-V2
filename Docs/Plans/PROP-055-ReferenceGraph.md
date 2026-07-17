# PROP-055: ReferenceGraph — Typový graf závislostí a detekce cyklů

> **Stav:** 📝 Navrženo (revidováno 2026-07-17 — ID-based)
> **Datum:** 2026-07-13
> **Poslední revize:** 2026-07-17
> **Oblast:** Core, Generators
> **Odhad:** ~2 dny
> **Zdroj:** IDEA-010 (ReferenceGraph)
> **Závislost:** PROP-060 (Element Identity Stabilization) — 🔴 HARD. ReferenceGraph používá `Guid Id` z `IMemberElement` a `ElementIdMapping` pro rozlišení referencí.

## Cíl

Vybudovat orientovaný graf závislostí mezi `RootElement` (Class, Interface, Enum, Struct…) pro:
1. **Detekci cirkulárních referencí** — zachycení cyklů v modelu, ne až při generování
2. **Nalezení nevyřešených typů** — `PropertyElement.Type.CustomTypeName = "XYZ"` kde XYZ neexistuje
3. **Správné pořadí generování** — base class před derived class, interface implementace před třídou
4. **Přesnější generování generik** — oprava `MakeCollection()` (dnes generuje `List<object>` místo `List<int>`)

## Motivace

### Dnešní stav

Všechny typové reference mezi elementy jsou **nevyřešené řetězce**:

| Reference | Typ dnes | Problém |
|-----------|----------|---------|
| `ClassElement.BaseClassName` | `string?` | Nelze ověřit, že base class existuje |
| `ClassElement.ImplementedInterfaces` | `List<string>` | Nelze ověřit, že interface existuje |
| `PropertyElement.Type.CustomTypeName` | `string?` | Generátor tiše vyprodukuje `object /* TODO */` |
| `MethodElement.ReturnType.CustomTypeName` | `string?` | Stejný problém |
| `GenericConstraint.BaseTypeName` | `string?` | Nelze ověřit constraint typ |
| `NewExpression.TypeName` | `string` | Nelze ověřit, že typ existuje |

Žádná vrstva nedokáže odpovědět na otázku:
- *"Existuje element s názvem Customer?"*
- *"Máme cyklus A → B → A?"*
- *"V jakém pořadí se mají elementy generovat?"*

### ReferenceGraph řešení

```csharp
var elements = translator.TranslateDocument(document);
var idMapping = translator.GetElementIdMapping();  // z PROP-060

var graph = ReferenceGraph.Build(elements, idMapping, diagnosticBag);
// Uvnitř Build():
//   - Extrahuje reference podle Guid Id (ne string Name)
//   - Rozlišuje typy referencí: Inheritance / PropertyType / MethodReturn / FieldType / GenericConstraint
//   - Hlásí cykly a nevyřešené reference přes DiagnosticBag
//   - Vrací immutable graf s předpočítanými výsledky

graph.Cycles;        // → [ReferenceCycle { ElementIds: [...], DisplayNames: ["Customer", "Order", "Customer"] }]
graph.Unresolved;    // → [UnresolvedReference { SourceId: ..., TargetId: ..., ReferencedAs: "Property 'Manager'" }]
graph.SortedElements; // → [Customer, Order, OrderItem] (topologicky seřazeno)
```

### Proč ID-based a ne name-based

| Aspekt | Name-based (původní návrh) | ID-based (revidovaný) |
|--------|---------------------------|----------------------|
| **Stabilita** | ❌ Přejmenování = broken graph | ✅ Guid se nikdy nemění |
| **Refactoring** | ❌ Nelze přejmenovat bez rozbití referencí | ✅ DisplayName se změní, ID zůstává |
| **Unresolved handling** | ❌ `null` Element — nevíme, jestli typ někdy existoval | ✅ Guid existuje v referencích, ale ne v modelu |
| **Serializace** | ❌ String — křehké napříč sessions | ✅ Guid — stabilní i přes export/import |
| **Diagnostika** | 🟡 String je čitelný | ✅ DisplayName pro čitelnost + Guid pro přesnost |
| **Integrace s PROP-060** | ❌ Žádná — ID mapping ignorován | ✅ Přímé napojení na `ElementIdMapping` |
| **Budoucí rozšíření** | ❌ Namespaces, partial classes — problém | ✅ ID je jednoznačné bez ohledu na kontext |

## Návrh

### Referenční model v Core

```csharp
// Src/MetaForge.Core/ReferenceGraph/ReferenceGraphNode.cs
public sealed record ReferenceGraphNode
{
    /// <summary>Stabilní identifikátor elementu (Guid z Core, PROP-060).</summary>
    public Guid ElementId { get; init; }

    /// <summary>Lidsky čitelný název pro debug a diagnostiku.</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Typ elementu (Class, Interface, Enum, Struct, ...).</summary>
    public string ElementKind { get; init; } = string.Empty;

    /// <summary>Odkaz na element (null = ID existuje v referencích, ale ne v modelu).</summary>
    public RootElement? Element { get; init; }

    /// <summary>ID elementů, na které tento element odkazuje.</summary>
    public IReadOnlyList<Guid> References { get; init; } = [];
}

// Src/MetaForge.Core/ReferenceGraph/ReferenceGraph.cs
public sealed class ReferenceGraph
{
    private readonly Dictionary<Guid, ReferenceGraphNode> _nodes;
    private readonly Dictionary<Guid, string> _idToName;  // Guid → DisplayName lookup

    private ReferenceGraph(Dictionary<Guid, ReferenceGraphNode> nodes) { ... }

    /// <summary>
    /// Sestaví graf z elementů a ID mappingu.
    /// Extrahuje reference, detekuje cykly, najde nevyřešené.
    /// Všechny problémy zapisuje rovnou do DiagnosticBag.
    /// </summary>
    public static ReferenceGraph Build(
        IEnumerable<RootElement> elements,
        ElementIdMapping idMapping,
        DiagnosticBag diagnostics);

    /// <summary>Nalezené cykly (prázdné = žádné).</summary>
    public IReadOnlyList<ReferenceCycle> Cycles { get; }

    /// <summary>Nevyřešené reference (prázdné = vše resolvnuto).</summary>
    public IReadOnlyList<UnresolvedReference> Unresolved { get; }

    /// <summary>Topologicky seřazené elementy pro generování.</summary>
    public IReadOnlyList<RootElement> SortedElements { get; }

    /// <summary>Přístup k uzlům podle ID.</summary>
    public ReferenceGraphNode? GetNode(Guid elementId);

    /// <summary>Počet uzlů v grafu.</summary>
    public int NodeCount { get; }

    /// <summary>Počet hran (referencí) v grafu.</summary>
    public int EdgeCount { get; }
}

// Src/MetaForge.Core/ReferenceGraph/ReferenceKind.cs
public enum ReferenceKind
{
    Inheritance,            // BaseClassName
    InterfaceImplementation, // ImplementedInterfaces
    PropertyType,           // PropertyElement.Type.CustomTypeName
    MethodReturn,           // MethodElement.ReturnType.CustomTypeName
    FieldType,              // FieldElement.Type.CustomTypeName
    GenericConstraint,      // GenericConstraint.BaseTypeName
    NewExpression,          // NewExpression.TypeName
    ParameterType,          // ParameterElement.Type.CustomTypeName
}

// Src/MetaForge.Core/ReferenceGraph/ReferenceCycle.cs
public sealed record ReferenceCycle
{
    /// <summary>ID elementů v cyklu (v pořadí závislosti).</summary>
    public IReadOnlyList<Guid> ElementIds { get; init; } = [];

    /// <summary>Lidsky čitelné názvy pro diagnostiku.</summary>
    public IReadOnlyList<string> DisplayNames { get; init; } = [];

    public override string ToString() => string.Join(" → ", DisplayNames) + " → " + DisplayNames[0];
}

// Src/MetaForge.Core/ReferenceGraph/UnresolvedReference.cs
public sealed record UnresolvedReference
{
    /// <summary>ID elementu, který referenci drží.</summary>
    public Guid SourceElementId { get; init; }

    /// <summary>Název zdrojového elementu pro diagnostiku.</summary>
    public string SourceDisplayName { get; init; } = string.Empty;

    /// <summary>ID, na které reference ukazuje (neexistuje v modelu).</summary>
    public Guid TargetId { get; init; }

    /// <summary>Jak se reference jmenuje v kódu (např. "Property 'Manager'").</summary>
    public string ReferencedAs { get; init; } = string.Empty;

    /// <summary>Typ reference.</summary>
    public ReferenceKind Kind { get; init; }
}
```

### Jak se staví graf

```csharp
var elements = translator.TranslateDocument(document);
var idMapping = translator.GetElementIdMapping();  // PROP-060 — Dictionary<businessId, Guid>

var graph = ReferenceGraph.Build(elements, idMapping, diagnosticBag);
// Uvnitř Build():
//   1. Pro každý RootElement vytvoří uzel:
//      - node.ElementId = element.Id (Guid, PROP-060)
//      - node.DisplayName = element.Name (string, pro diagnostiku)
//   2. Pro každý element extrahuje reference podle druhu:
//      - ClassElement.BaseClassName → idMapping.Resolve(baseClassName) → ReferenceKind.Inheritance
//      - ClassElement.ImplementedInterfaces → idMapping.Resolve(iface) → ReferenceKind.InterfaceImplementation
//      - PropertyElement.Type.CustomTypeName → idMapping.Resolve(typeName) → ReferenceKind.PropertyType
//      - MethodElement.ReturnType.CustomTypeName → idMapping.Resolve(typeName) → ReferenceKind.MethodReturn
//      - FieldElement.Type.CustomTypeName → idMapping.Resolve(typeName) → ReferenceKind.FieldType
//      - GenericConstraint.BaseTypeName → idMapping.Resolve(typeName) → ReferenceKind.GenericConstraint
//      - NewExpression.TypeName → idMapping.Resolve(typeName) → ReferenceKind.NewExpression
//      - ParameterElement.Type.CustomTypeName → idMapping.Resolve(typeName) → ReferenceKind.ParameterType
//   3. Pokud idMapping.Resolve() vrací null → nevyřešená reference → UnresolvedReference
//   4. Pokud ID existuje v referencích, ale ne v _nodes → nevyřešená
//   5. Topologické řazení + detekce cyklů (Kahnův algoritmus)
//   6. Vše reportováno do DiagnosticBag:
//      - Cykly → DiagnosticSeverity.Error
//      - Nevyřešené → DiagnosticSeverity.Warning
```

### Topologické řazení

Použít Kahnův algoritmus (BFS-based topological sort):
1. Vypočítat in-degree každého uzlu (kolik referencí na něj ukazuje)
2. Uzly s in-degree = 0 jdou do fronty
3. Odebírat uzly, snižovat in-degree sousedům
4. Pokud po skončení zbývají uzly → cyklus → `ReferenceCycle`

### Generování po vrstvách (volitelné rozšíření)

```csharp
// Výstup Sort() lze rozdělit do vrstev:
// Vrstva 0: elementy bez závislostí (enumy, standalone třídy)
// Vrstva 1: elementy závislé jen na Vrstvě 0 (base class, interface implementace)
// Vrstva N: elementy závislé na Vrstvě N-1
var layers = graph.GetLayers();
// → [[Color, Status], [Vehicle, INotifyPropertyChanged], [Car, Motorcycle]]
```

### Napojení na CLI generate

```bash
dotnet run --project Src/MetaForge.Cli -- generate --output ./out --check
# Výstup:
# ✅ ReferenceGraph: 12 elementů, 8 referencí, 0 cyklů
# ⚠️ Nevyřešeno: 'Employee' (Property 'Manager' na 'Customer') — CustomTypeName bez matching elementu
# ❌ Cyklus: Customer → Order → Customer (detekován, generování přerušeno)
```

## Implementační dopad

| Soubor | Typ | Popis |
|--------|-----|-------|
| `Src/MetaForge.Core/ReferenceGraph/ReferenceGraphNode.cs` | Nový | Uzel grafu — key = `Guid ElementId` |
| `Src/MetaForge.Core/ReferenceGraph/ReferenceGraph.cs` | Nový | Immutable graf — `Build()` factory, `Cycles`/`Unresolved`/`SortedElements` |
| `Src/MetaForge.Core/ReferenceGraph/ReferenceKind.cs` | Nový | Enum druhů referencí (Inheritance, PropertyType, ...) |
| `Src/MetaForge.Core/ReferenceGraph/ReferenceCycle.cs` | Nový | Strukturovaný cyklus — `ElementIds` + `DisplayNames` |
| `Src/MetaForge.Core/ReferenceGraph/UnresolvedReference.cs` | Nový | Nevyřešená reference — `SourceId` + `TargetId` + `Kind` |
| `Src/MetaForge.Translator/Translation/ElementIdMapping.cs` | Použito | PROP-060 — `Resolve(string businessId)` vrací `Guid?` |
| `Src/MetaForge.Generators/CodeGenerator.cs` | Změna | Využití `SortedElements` pro pořadí generování |
| `Src/MetaForge.Cli/Program.cs` | Změna | Volitelné `--check` s výpisem cyklů a nevyřešených |
| `Tests/MetaForge.Core.Tests/ReferenceGraph/` | Nový | Unit testy s Guid-based elementy |

## Fáze implementace

| Fáze | Co | Odhad |
|------|-----|-------|
| 1 | `ReferenceGraphNode` + `ReferenceKind` + `ReferenceCycle` + `UnresolvedReference` — 4 immutable recordy v Core | 0.5 dne |
| 2 | Extrakce referencí z RootElement — iterace properties/metod/fields přes `Guid Id`, použití `ElementIdMapping.Resolve()` z PROP-060 | 0.5 dne |
| 3 | `ReferenceGraph.Build()` — konstrukce grafu, Kahnův topological sort, detekce cyklů, reportování do DiagnosticBag | 0.5 dne |
| 4 | `GetLayers()` — volitelné rozšíření pro vrstvené generování | 0.25 dne |
| 5 | Napojení na `CodeGenerator` (SortedElements) + CLI `--check` | 0.25 dne |
| **Celkem** | | **~2 dny** |

## Validace

- **Unit test:** `ReferenceGraph.Build()` se 3 elementy (A→B, B→C přes Guid) → `SortedElements` = [A, B, C], `Cycles` = prázdné
- **Unit test:** `ReferenceGraph.Build()` s cyklem (A→B, B→A přes Guid) → `Cycles` = [ReferenceCycle s DisplayNames ["A", "B", "A"]]
- **Unit test:** `ReferenceGraph.Build()` s chybějícím typem → `Unresolved` = [UnresolvedReference { Kind = PropertyType, ReferencedAs = "Property 'X'" }]
- **Unit test:** `ReferenceGraph.GetLayers()` — 5 elementů, 3 vrstvy
- **Unit test:** `ReferenceCycle.ToString()` → "Customer → Order → Customer"
- **Integrační test:** `CodeGenerator` používá `SortedElements` — pořadí výstupu odpovídá závislostem
- **CLI test:** `generate --check` vypíše ReferenceGraph diagnostiku s DisplayNames
- **Snapshot test:** `ReferenceGraph` se serializuje do JSON (Guid-based, stabilní napříč sessions)

## Otevřené otázky

- OQ-055-01: Má `ReferenceGraph.SortedElements` vracet ploché pořadí, nebo vrstvy (`GetLayers()`)? **Návrh:** `SortedElements` vrací ploché (pro CodeGenerator), `GetLayers()` jako volitelné rozšíření.
- OQ-055-02: Jak naložit s cyklem? Přerušit generování na problematickém elementu, nebo vygenerovat stub s diagnostikou? **Návrh:** Fail-fast — přerušit. Cyklus = chyba modelu, ne generátoru.
- OQ-055-03: Co když `ElementIdMapping.Resolve()` vrací `null` pro `CustomTypeName`, který je primitivní typ (`int`, `string`)? **Návrh:** Primitiva a `TypeModel` vestavěné typy přeskočit — nejsou elementy v grafu.
- OQ-055-04: Má `ReferenceGraph.Build()` akceptovat `ElementIdMapping` jako povinný parametr, nebo jako optional? **Návrh:** Povinný — bez ID mappingu nelze rozlišit reference. Pokud PROP-060 ještě není hotový, nelze PROP-055 spustit.

## Navazuje na

- **PROP-060** (Element Identity Stabilization) — 🔴 HARD závislost. ReferenceGraph používá `Guid Id` a `ElementIdMapping.Resolve()`.
- `IDEA-010` — původní koncept (ReferenceGraph vyčleněn jako samostatný PROP)
- `CodeGenerator.cs` — dnes single-pass, bude využívat `SortedElements`
- `DiagnosticBag` — existující infrastruktura pro reportování
- `BuildResult<T>.Then()` — monadické řetězení pro pipeline
- **PROP-056** (Projection Unification) — `DependencyGraphSection` v `DocumentProjection` bude konzumovat `ReferenceGraph`
