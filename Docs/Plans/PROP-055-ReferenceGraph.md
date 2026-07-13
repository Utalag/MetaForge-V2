# PROP-055: ReferenceGraph — Typový graf závislostí a detekce cyklů

> **Stav:** 📝 Navrženo
> **Datum:** 2026-07-13
> **Oblast:** Core, Generators
> **Odhad:** ~2 dny
> **Zdroj:** IDEA-010 (ReferenceGraph)

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
var graph = new ReferenceGraph(elements);

graph.DetectCycles();       // → ["Customer → Order → Customer"]
graph.FindUnresolved();     // → ["Property 'Manager' odkazuje na 'Employee' (nenalezeno)"]
graph.Sort();              // → [Customer, Order, OrderItem] (base → derived)
graph.ReportDiagnostics();  // → DiagnosticBag s chybami a varováními
```

## Návrh

### Referenční model v Core

```csharp
// Src/MetaForge.Core/ReferenceGraph/ReferenceGraphNode.cs
public sealed record ReferenceGraphNode
{
    /// <summary>Název elementu (např. "Customer").</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Odkaz na element (null = unresolved).</summary>
    public RootElement? Element { get; init; }

    /// <summary>Názvy typů, na které tento element odkazuje.</summary>
    public IReadOnlyList<string> References { get; init; } = [];
}

// Src/MetaForge.Core/ReferenceGraph/ReferenceGraph.cs
public sealed class ReferenceGraph
{
    private readonly Dictionary<string, ReferenceGraphNode> _nodes;

    public ReferenceGraph(IEnumerable<RootElement> elements) { ... }

    /// <summary>Detekuje cirkulární reference — vrací popis cyklů.</summary>
    public IReadOnlyList<string> DetectCycles();

    /// <summary>Najde nevyřešené reference (CustomTypeName bez matching elementu).</summary>
    public IReadOnlyList<string> FindUnresolvedReferences();

    /// <summary>Topologické seřazení — base class před derived class.</summary>
    public IReadOnlyList<RootElement> Sort();

    /// <summary>Zapíše nalezené problémy do DiagnosticBag.</summary>
    public void ReportDiagnostics(DiagnosticBag bag);
}
```

### Jak se staví graf

```csharp
var elements = translator.TranslateDocument(document);
var graph = new ReferenceGraph(elements);
// Uvnitř konstruktoru:
//   Pro každý element extrahuje:
//   - BaseClassName → reference
//   - ImplementedInterfaces → reference
//   - Všechny PropertyElement.Type.CustomTypeName → reference
//   - MethodElement.ReturnType.CustomTypeName → reference
//   - FieldElement.Type.CustomTypeName → reference
//   - GenericConstraint.* → reference
//   - NewExpression.TypeName → reference
```

### Topologické řazení

Použít Kahnův algoritmus (BFS-based topological sort):
1. Vypočítat in-degree každého uzlu
2. Uzly s in-degree = 0 jdou do fronty
3. Odebírat uzly, snižovat in-degree sousedům
4. Pokud po skončení zbývají uzly → cyklus

### Napojení na CLI generate

```bash
dotnet run --project Src/MetaForge.Cli -- generate --output ./out --check
# Výstup:
# ✅ Grafu: 12 elementů, 8 závislostí
# ⚠️ Cyklus: Customer → Order → Customer
# ❌ Nevyřešeno: Property 'Manager' → 'Employee'
```

## Implementační dopad

| Soubor | Typ | Popis |
|--------|-----|-------|
| `Src/MetaForge.Core/ReferenceGraph/ReferenceGraphNode.cs` | Nový | Uzel grafu |
| `Src/MetaForge.Core/ReferenceGraph/ReferenceGraph.cs` | Nový | Graf + algoritmy |
| `Src/MetaForge.Core/ReferenceGraph/ReferenceGraphDiagnostics.cs` | Nový | Reportování do DiagnosticBag |
| `Src/MetaForge.Generators/CodeGenerator.cs` | Změna | Využití `Sort()` pro pořadí |
| `Src/MetaForge.Cli/Program.cs` | Změna | Volitelné `--check` |
| `Tests/MetaForge.Core.Tests/ReferenceGraph/` | Nový | Unit testy |

## Fáze implementace

| Fáze | Co | Odhad |
|------|-----|-------|
| 1 | `ReferenceGraphNode` + `ReferenceGraph` model v Core | 0.5 dne |
| 2 | Extrakce referencí z RootElement (Class, Interface, Enum, Struct…) | 0.5 dne |
| 3 | TopologicalSort — Kahnův algoritmus | 0.5 dne |
| 4 | Detekce cyklů + nevyřešených referencí | 0.5 dne |
| 5 | Diagnostika → `DiagnosticBag`, napojení na CLI | 0.5 dne |
| **Celkem** | | **~2.5 dne** |

## Validace

- **Unit test:** ReferenceGraph s 3 elementy (A→B, B→C) → Sort = [A, B, C]
- **Unit test:** ReferenceGraph s cyklem (A→B, B→A) → DetectCycles = ["A → B → A"]
- **Unit test:** ReferenceGraph s chybějícím typem → FindUnresolved = ["'X' nenalezen"]
- **Integrační test:** `CodeGenerator` používá `Sort()` — pořadí výstupu odpovídá závislostem
- **CLI test:** `generate --check` vypíše diagnostiku

## Otevřené otázky

- OQ-055-01: Má `ReferenceGraph.Sort()` vracet pouze seřazené elementy, nebo i informaci o řezech (vrstvách)?
- OQ-055-02: Jak naložit s cyklem? Přerušit generování na problematickém elementu, nebo vygenerovat stub s diagnostikou?

## Navazuje na

- `IDEA-010` — původní koncept (ReferenceGraph vyčleněn jako samostatný PROP)
- `CodeGenerator.cs` — dnes single-pass, bude využívat `Sort()`
- `DiagnosticBag` — existující infrastruktura pro reportování
- `BuildResult<T>.Then()` — monadické řetězení pro pipeline
