# PROP-060: Element Identity Stabilization — ID-first foundation

Typ výsledku: Candidate Proposal
Zdroj podnětu: Identity audit 2026-07-17 (Perplexity konverzace e2801d78 — požadavek na ID-first reference)
Stav životního cyklu: Candidate
Rozhodovací owner:
Poslední revize: 2026-07-17

Priorita: 🔴 Critical — prerekvizita pro PROP-057
Oblast: Core, BusinessModel, Translator
Owner:
Datum vytvoření: 2026-07-17
Aktualizováno: 2026-07-17
Odhad: 1–2 dny

Navazuje na:
- **PROP-024** (StrongType — hotovo)
- **PROP-031** (Core Statement System — hotovo)
- **PROP-040** (Core Member Consistency — hotovo) — `IMemberElement` interface již existuje

Blokuje:
- **PROP-057** (ElementContract + VerificationModel) — 🔴 HARD dependency. `ContractScenario.InputsByElementId` vyžaduje stabilní ID pro Property/Method/Parameter.
- **PROP-058** (Sandbox Preview Runner) — nepřímo (přes PROP-057)

---

## 1. Kontext

**Identity audit (2026-07-17) odhalil kritickou mezeru:**

| Vrstva | Top-level elementy | Member elementy |
|--------|-------------------|-----------------|
| **BusinessModel** | ✅ Všechny kromě `BusinessParameterNode` mají `string Id` | ❌ `BusinessParameterNode` NEMÁ Id |
| **Core** | ✅ `RootElement` potomci mají `Guid Id` | ❌ `IMemberElement` typy NEMAJÍ Id — identifikovány jen `Name` |
| **Translator** | ❌ Mapování Business→Core ID je **zahozeno** — `BusinessEntityNode.Id` → `ClassElement.Id` (nové random Guid) | ❌ Žádné mapování neexistuje |

**Bez stabilních ID nelze:**
- Referencovat property/metodu/parametr v `ElementContract` (PROP-057)
- Sestavit `ContractScenario.InputsByElementId` — není co použít jako klíč
- Zachovat traceabilitu z Core zpět do BusinessModel

---

## 2. Problém dnes

```csharp
// BusinessModel — má ID
var entity = new BusinessEntityNode { Id = "a1b2c3d4", Name = "Auto" };
var attr = new BusinessAttributeNode { Id = "e5f6g7h8", Name = "Spz" };

// Translator — ID zahozeno
var classElement = new ClassElement { Name = entity.Name };  // Id = nové random Guid
var propertyElement = new PropertyElement { Name = attr.Name };  // NEMÁ Id vůbec

// ❌ Není způsob, jak zpětně zjistit, že propertyElement vznikl z attr "e5f6g7h8"
// ❌ Není způsob, jak v ElementContract říct "pravidlo patří k property e5f6g7h8"
```

---

## 3. Cíl

1. **Přidat `Guid Id` do `IMemberElement`** — `PropertyElement`, `MethodElement`, `ParameterElement`, `FieldElement`, `ConstructorElement` získají stabilní identitu
2. **Přidat `string Id` do `BusinessParameterNode`** — konzistence s ostatními BusinessModel typy
3. **Opravit Business → Core ID mapping v Translatoru** — `DefaultBusinessTranslator` bude ukládat `Dictionary<string, Guid>` mapující Business ID na Core ID
4. **Exponovat mapping přes projekci** — `ProjectionReadService` bude vracet mapování pro host surfaces

---

## 4. Architektonické invarianty

- **BusinessAuthoringDocument zůstává source of truth** — ID jsou generována v BusinessModel vrstvě
- **Core zůstává read-only derivace** — ID v Core jsou přiřazena Translátorem, ne Core samotným
- **CommandLog zůstává append-only**
- **Existující chování se nemění** — `Name` zůstává primární identifikátor pro uživatele; `Id` je strojová reference

---

## 5. Scope

### In scope

| # | Akce | Vrstva | Dopad |
|---|------|--------|-------|
| 1 | Přidat `Guid Id { get; init; }` do `IMemberElement` | Core | `PropertyElement`, `MethodElement`, `ParameterElement`, `FieldElement`, `ConstructorElement` |
| 2 | Přidat `string Id { get; init; }` do `BusinessParameterNode` | BusinessModel | Konzistence — všechny BusinessModel typy mají Id |
| 3 | `DefaultBusinessTranslator`: ukládat `Dictionary<string, Guid>` (BusinessId → CoreId) | Translator | Traceabilita Business → Core |
| 4 | `DefaultBusinessTranslator`: přiřazovat `IMemberElement.Id` podle `BusinessAttributeNode.Id` / `BusinessBehaviorNode.Id` | Translator | Stabilní mapování |
| 5 | Exponovat `ElementIdMapping` přes `ProjectionReadService` | Translator | Host surfaces můžou číst mapování |
| 6 | `BusinessIdAllocator` rozšířit o member-level identitu (volitelné) | BusinessModel | Alternativní alokátor pro lidsky čitelné ID |

### Out of scope

- Zpětná kompatibilita starých JSONL souborů bez ID (mitigace: default `Guid.NewGuid()` při deserializaci)
- Obousměrné Core → Business mapování (Fáze 1 jen Business → Core)

---

## 6. Návrh řešení

### 6.1 IMemberElement — přidat Id

```csharp
// Src/MetaForge.Core/Abstractions/IMemberElement.cs

public interface IMemberElement
{
    Guid Id { get; init; }        // NOVÉ — stabilní identita
    string Name { get; init; }
    IReadOnlyList<AttributeElement> Attributes { get; init; }
    MetadataBag Metadata { get; init; }
    string? XmlSummary { get; init; }
    int Coin { get; init; }
}
```

Implementace ve všech member typech:

```csharp
// PropertyElement, MethodElement, ParameterElement, FieldElement, ConstructorElement
public Guid Id { get; init; } = Guid.NewGuid();
```

### 6.2 BusinessParameterNode — přidat Id

```csharp
// Src/MetaForge.BusinessModel/Models/BusinessBehaviorNode.cs

public sealed record BusinessParameterNode
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..8];  // NOVÉ
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = "string";
    public bool IsRequired { get; init; }
    public string? DefaultValue { get; init; }
    public string? Summary { get; init; }
}
```

### 6.3 Translator — ElementIdMapping

```csharp
// Src/MetaForge.Translator/Translation/ElementIdMapping.cs

/// <summary>
/// Uchovává mapování BusinessModel ID → Core ID.
/// Vytvořeno během TranslateDocument(), použito pro traceabilitu a projekce.
/// </summary>
public sealed class ElementIdMapping
{
    public Dictionary<string, Guid> EntityIds { get; init; } = new();
    public Dictionary<string, Guid> AttributeIds { get; init; } = new();
    public Dictionary<string, Guid> BehaviorIds { get; init; } = new();
    public Dictionary<string, Guid> ParameterIds { get; init; } = new();

    public Guid? ResolveBusinessId(string businessId)
    {
        if (EntityIds.TryGetValue(businessId, out var id)) return id;
        if (AttributeIds.TryGetValue(businessId, out id)) return id;
        if (BehaviorIds.TryGetValue(businessId, out id)) return id;
        if (ParameterIds.TryGetValue(businessId, out id)) return id;
        return null;
    }
}
```

### 6.4 DefaultBusinessTranslator — oprava mapování

```csharp
// PŮVODNÍ (chybné):
var classElement = new ClassElement { Name = entity.Name };
// entity.Id zahozeno, classElement.Id = nové random Guid

// NOVÉ:
var mapping = new ElementIdMapping();
var classElement = new ClassElement { Name = entity.Name };
mapping.EntityIds[entity.Id] = classElement.Id;  // Uložit mapování

foreach (var attr in entity.Attributes)
{
    var property = new PropertyElement { Name = attr.Name };
    mapping.AttributeIds[attr.Id] = property.Id;  // Uložit mapování
    classElement.Properties.Add(property);
}
```

### 6.5 Projektová ID konvence

Pro čitelnost a debug se používá prefixová konvence:

| Prefix | Typ | Příklad |
|--------|-----|---------|
| `entity:` | BusinessEntityNode / ClassElement | `entity:auto:a1b2c3d4` |
| `attr:` | BusinessAttributeNode / PropertyElement | `attr:auto-spz:e5f6g7h8` |
| `method:` | BusinessBehaviorNode / MethodElement | `method:auto-registruj:9i0j1k2l` |
| `param:` | BusinessParameterNode / ParameterElement | `param:auto-registruj-spz:3m4n5o6p` |

> ⚠️ Tato konvence je pro debug/display, NE pro strojové reference. Strojově se používá `Guid Id`.

---

## 7. Implementační dopad

### Změněné soubory

| Soubor | Změna |
|--------|-------|
| `Src/MetaForge.Core/Abstractions/IMemberElement.cs` | Přidat `Guid Id` |
| `Src/MetaForge.Core/Elements/Members/PropertyElement.cs` | Implementovat `Id` |
| `Src/MetaForge.Core/Elements/Members/MethodElement.cs` | Implementovat `Id` |
| `Src/MetaForge.Core/Elements/Members/ParameterElement.cs` | Implementovat `Id` |
| `Src/MetaForge.Core/Elements/Members/FieldElement.cs` | Implementovat `Id` |
| `Src/MetaForge.Core/Elements/Members/ConstructorElement.cs` | Implementovat `Id` |
| `Src/MetaForge.BusinessModel/Models/BusinessBehaviorNode.cs` | Přidat `Id` do `BusinessParameterNode` |
| `Src/MetaForge.Translator/Translation/ElementIdMapping.cs` | **Nový** — mapování |
| `Src/MetaForge.Translator/Translation/DefaultBusinessTranslator.cs` | Ukládat mapování + přiřazovat členská ID |
| `Src/MetaForge.Translator/Host/ProjectionReadService.cs` | Exponovat mapping |

### Testy

| Test | Ověřuje |
|------|---------|
| `MemberElement_HasId_AfterConstruction` | Property/Method/Parameter mají Id |
| `IdMapping_ResolveBusinessId_ReturnsCoreId` | Business ID → Core ID |
| `BusinessParameterNode_HasId` | Parametr má Id |
| `Translator_PreservesIdMapping` | Po překladu mapování obsahuje všechny elementy |

---

## 8. Rizika

- **Riziko: breaking change** — přidání `Guid Id` do `IMemberElement` vyžaduje úpravu všech implementací.
  - *Mitigace: default `Guid.NewGuid()` — existující kód nemusí explicitně nastavovat.*
- **Riziko: staré JSONL soubory** — staré dokumenty nemají Id pro member typy.
  - *Mitigace: při deserializaci generovat nové Guid.*
- **Riziko: performance** — Dictionary lookup pro každý element.
  - *Mitigace: O(1) lookup, zanedbatelné.*

---

## 9. Validace

- **Build**: `dotnet build` projde (všechny implementace `IMemberElement` mají `Id`)
- **Testy**: 4 unit testy
- **Smoke**: Přeložit BusinessModel → Core, ověřit `ElementIdMapping.ResolveBusinessId()`
