# PROP-057: ElementContract + VerificationModel

Typ výsledku: Candidate Proposal
Zdroj podnětu: Perplexity konverzace e2801d78 (2026-07-16) — návrhy #1, #2
Stav životního cyklu: Candidate
Rozhodovací owner:
Poslední revize: 2026-07-16

Priorita: High
Oblast: Core, BusinessModel, Infrastructure
Owner:
Datum vytvoření: 2026-07-16
Aktualizováno: 2026-07-16

Navazuje na:
- **PROP-024** (StrongType + Expression Record — hotovo) — StrongType pattern je základem pro ElementContract
- **PROP-036** (Core Specification Layer — hotovo) — `InvariantDefinition` a `InvariantExpression` jsou stavební kameny
- **PROP-039** (Core Composability — hotovo) — `ElementFingerprint` existuje pro dirty-tracking; PROP-057 ho rozšiřuje o `ContractHash` a `VerificationSchemaVersion` pro verifikační účely
- **PROP-051** (Support Matrix Contract Map — hotovo) — jiný koncept "contract" (API stabilita, ne sémantický kontrakt); PROP-057 doplňuje, nekonkuruje

Blokuje:
- **PROP-058** (Sandbox Preview Runner) — potřebuje `ElementContract` pro definici scénářů a `VerificationStateStore` pro skipování
- **PROP-059** (Resilience & Healing Layer) — potřebuje `VerificationModel` pro určení, co je ještě validní oprava

Související soubory:
- `Src/MetaForge.Core/Elements/ClassElement.cs`
- `Src/MetaForge.Core/Elements/MethodElement.cs`
- `Src/MetaForge.Core/Types/StrongType.cs`
- `Src/MetaForge.Core/Metadata/MetadataBag.cs`
- `Src/MetaForge.Core/Specifications/InvariantDefinition.cs` (PROP-036)
- `Src/MetaForge.Core/Composability/ElementFingerprint.cs` (PROP-039)

---

## 1. Kontext

MetaForge je **metadata-first** platforma. `StrongType` (PROP-024) již dnes nese validační pravidla a konverzní chování — ukazuje pattern: typ není jen jméno, ale **významový objekt s kontraktem**. `InvariantDefinition` (PROP-036) umožňuje deklarovat pravidla typu "IsAsync && IsAbstract → invalid". `ElementFingerprint` (PROP-039) umožňuje detekovat změny pro dirty-tracking.

Perplexity konverzace identifikovala mezeru: MetaForge nemá **jednotný koncept "sémantického kontraktu elementu"**. `MetadataBag` je moc volný (string-based klíče), `StrongType` pattern je moc úzký (jen value objekty), `InvariantDefinition` je moc technický (nízkoúrovňová pravidla). Chybí střední vrstva: "co pro tento element znamená být validní, jaké má chování, jaké scénáře ho ověřují?"

Zároveň konverzace identifikovala potřebu **typovaného VerificationModel** — ne další sady `MetadataBag` klíčů, ale first-class model pro verifikační scénáře — a **fingerprint-based verifikačních stavů**, které umožní skipovat již ověřené elementy.

### Vztah k existujícím konceptům

| Existující koncept | PROP | Vztah k PROP-057 |
|-------------------|------|------------------|
| `StrongType.ValidationRules` | PROP-024 | ElementContract **zobecňuje** — StrongType je value-level, ElementContract je element-level |
| `InvariantDefinition` | PROP-036 | ElementContract **využívá** — může odkazovat na invarianty jako součást kontraktu |
| `ElementFingerprint` | PROP-039 | PROP-057 **rozšiřuje** — přidává `ContractHash` a `VerificationSchemaVersion` |
| `MetadataBag` | PROP-038 | ElementContract **doplňuje** — metadata zůstávají pro anotace, kontrakt je pro sémantiku |
| Support Matrix "contract" | PROP-051 | **Jiný koncept** — PROP-051 = API stabilita (public/experimental), PROP-057 = sémantický kontrakt |

---

## 2. Problém dnes

- **Není jednotný způsob**, jak element deklaruje "co je pro měj validní/nevalidní".
- `MetadataBag` se používá pro `Validation.*`, `Docs.*`, `Generation.*`, `Ai.*`, `Domain.*` — ale pro testovací/verifikační účely je **moc měkký** (string klíče, žádná typová bezpečnost).
- `StrongType` má `ValidationRules`, ale `ClassElement`, `MethodElement`, `PropertyElement` nic podobného nemají.
- Chybí mechanismus pro "tento element se nezměnil, přeskoč verifikaci" — `ElementFingerprint` (PROP-039) řeší dirty-tracking pro build, ale ne pro verifikační pipeline.
- Verifikační stav nemá kde být uložen — Core je read-only derivace, BusinessModel je source of truth, stav verifikace je **provozní, ne sémantický**.
- Když se změní generátor nebo validační pravidla, neexistuje způsob jak říct "všechny dřívější verifikace jsou neplatné".

---

## 3. Cíl

### ElementContract

- Každý významný element (Entity, Method v Fázi 1) **může** nést svůj sémantický kontrakt.
- Kontrakt obsahuje: **invarianty, validní/nevalidní pravidla, kanonické příklady, očekávané chování**.
- Navazuje na `StrongType` pattern, ale **zobecňuje ho** — není to "všechno je value object", ale "všechno může nést svůj význam".
- Je **volitelný** — ne každý element musí mít kontrakt (aby neblokoval rychlý prototyping).

### VerificationModel

- **Typovaný** model verifikace (ne `MetadataBag` klíče).
- **Oddělený od Core elementů** — Core nese "co má být pravda", VerificationModel "jak to ověřit".
- Navázaný na ElementContract — z kontraktu se odvozují verifikační scénáře.

### VerificationStateStore

- Stavy: `Unknown`, `Running`, `Passed`, `Failed`, `Stale`.
- **Rozšířený fingerprint**: `ElementId + StructureHash + ContractHash + GeneratorVersion + VerificationSchemaVersion`.
- Uložen v **Infrastructure** (stejný pattern jako `IProjectionCache`).
- Umožňuje **přeskočit** verifikaci u nezměněných `Passed` elementů.
- **Invalidace**: změna generatoru nebo verification schema → všechny `Passed` → `Stale`.

---

## 4. Architektonické invarianty

- **BusinessAuthoringDocument zůstává source of truth.** ElementContract vzniká z Business modelu přes Translator, stejně jako zbytek Core.
- **Core je read-only derivace** Business modelu — ElementContract je součástí Core, ale nevzniká přímou mutací.
- **Verifikační stav NENÍ v Core** — je v Infrastructure (stejný pattern jako checkpoint caching v PROP-028).
- **CommandLog zůstává append-only.**
- **AI je volitelná** — verifikace musí fungovat i bez AI. ElementContract se dá definovat ručně nebo přes AI asistenci.
- **Změny elementů jdou přes Business → Facade → CommandLog**, ne přímou mutací Core.

---

## 5. Scope

### In scope (Fáze 1)

- `ElementContract` — abstraktní base record
- `EntityContract` — pro `ClassElement`: doménové invarianty, valid/invalid archetypy, property-level pravidla
- `MethodContract` — pro `MethodElement`: vstupní kontrakt, výstupní kontrakt, side effects, scénářové hinty
- `VerificationModel` — typovaný model verifikačních scénářů
- `VerificationScenario` — jednotlivý scénář s očekáváním
- `VerificationState` enum + `VerificationRecord`
- `IVerificationStateStore` + implementace v Infrastructure
- Rozšíření `ElementFingerprint` o `ContractHash` a `VerificationSchemaVersion`

### Out of scope

- `PropertyContract` (Fáze 2) — samostatný kontrakt pro property; Fáze 1 řeší property rules přes `EntityContract`
- Automatické odvozování scénářů z pravidel (→ PROP-060, budoucí)
- Sandbox execution (→ PROP-058)
- Healing (→ PROP-059)


### Související IDEAs
- **IDEA-008** (Incremental Dirty-Tracking) — fingerprint overlap; dirty-tracking **graph** (propagace staleness) → kandidát na Fázi 2
- **IDEA-010** (SymbolTable) — sémantická verifikace ve Fázi 2: **použít Roslyn SemanticModel**, nestavět vlastní SymbolTable
- **IDEA-016** (Output Readiness Model) — konzumuje verifikační stavy; Fáze 4 syntéza
- **IDEA-018** (Execution Trace Recorder) — trasování verifikační pipeline přes `ILogger` se strukturovanými eventy, ne samostatný recorder
- **IDEA-029** (ProductionHub) — akceptační testovací sada: `ProductionHub_Verification`

---

## 6. Návrh řešení

### 6.1 ElementContract (Core)

```csharp
// Src/MetaForge.Core/Contracts/ElementContract.cs

/// <summary>
/// Sémantický kontrakt elementu — co pro tento element znamená být validní,
/// jaké má chování, jaké scénáře ho ověřují.
/// Zobecňuje StrongType pattern na všechny významné elementy.
/// </summary>
public abstract record ElementContract
{
    /// <summary>ID elementu, ke kterému kontrakt patří.</summary>
    public string ElementId { get; init; } = string.Empty;

    /// <summary>Doménové invarianty — pravidla, která musí vždy platit.</summary>
    public IReadOnlyList<ContractInvariant> Invariants { get; init; } = [];

    /// <summary>Kanonické příklady — validní a nevalidní exempláře.</summary>
    public IReadOnlyList<CanonicalExample> Examples { get; init; } = [];

    /// <summary>Rozšiřitelná metadata — doplněk k pevným polím.</summary>
    public MetadataBag Metadata { get; init; } = new();
}

/// <summary>Doménový invariant v rámci kontraktu.</summary>
public record ContractInvariant
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    /// <summary>Odkaz na InvariantDefinition z PROP-036 (volitelný).</summary>
    public string? InvariantDefinitionId { get; init; }
    public ValidationSeverity Severity { get; init; } = ValidationSeverity.Error;
}

/// <summary>Kanonický příklad — validní nebo nevalidní exemplář.</summary>
public record CanonicalExample
{
    public string Name { get; init; } = string.Empty;
    public bool IsValid { get; init; }
    public string? Description { get; init; }
    public IReadOnlyDictionary<string, object?> Values { get; init; }
        = new Dictionary<string, object?>();
}
```

```csharp
// Src/MetaForge.Core/Contracts/EntityContract.cs

/// <summary>
/// Kontrakt pro entitu (ClassElement).
/// Rozšiřuje ElementContract o property-level pravidla a vztahové constrainty.
/// </summary>
public sealed record EntityContract : ElementContract
{
    /// <summary>Pravidla pro jednotlivé property entity.</summary>
    public IReadOnlyList<PropertyRule> PropertyRules { get; init; } = [];

    /// <summary>Omezení na relace mezi entitami.</summary>
    public IReadOnlyList<RelationConstraint> RelationConstraints { get; init; } = [];
}

public record PropertyRule
{
    public string PropertyName { get; init; } = string.Empty;
    public IReadOnlyList<string> Constraints { get; init; } = [];
    // Např. "Required", "MaxLength(100)", "Range(0, 999)"
}

public record RelationConstraint
{
    public string RelationName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    // Např. "Každá Order musí mít právě jednoho Customer"
}
```

```csharp
// Src/MetaForge.Core/Contracts/MethodContract.cs

/// <summary>
/// Kontrakt pro metodu (MethodElement).
/// Rozšiřuje ElementContract o vstupní/výstupní očekávání,
/// side effects a scénářové hinty.
/// </summary>
public sealed record MethodContract : ElementContract
{
    /// <summary>Očekávání na vstupní parametry.</summary>
    public IReadOnlyList<ParameterExpectation> InputContract { get; init; } = [];

    /// <summary>Očekávání na výstup.</summary>
    public OutputExpectation? OutputContract { get; init; }

    /// <summary>Hinty o vedlejších efektech metody.</summary>
    public IReadOnlyList<SideEffectHint> SideEffects { get; init; } = [];

    /// <summary>Scénářové hinty pro generování testů.</summary>
    public IReadOnlyList<ScenarioHint> ScenarioHints { get; init; } = [];
}

public record ParameterExpectation
{
    public string ParameterName { get; init; } = string.Empty;
    public string? TypeConstraint { get; init; }      // "not null", "> 0"
    public IReadOnlyList<string> ValidExamples { get; init; } = [];
    public IReadOnlyList<string> InvalidExamples { get; init; } = [];
}

public record OutputExpectation
{
    public string? TypeConstraint { get; init; }
    public string? Description { get; init; }
    // Např. "vrací kladné číslo", "nikdy nevrací null"
}

public enum SideEffectKind { None, DatabaseWrite, FileWrite, NetworkCall, StateMutation }

public record SideEffectHint
{
    public SideEffectKind Kind { get; init; }
    public string? Description { get; init; }
}

public record ScenarioHint
{
    public string Name { get; init; } = string.Empty;
    public ScenarioKind Kind { get; init; }
    public string Description { get; init; } = string.Empty;
}

public enum ScenarioKind { HappyPath, Boundary, ErrorCase, EdgeCase }
```

### 6.2 VerificationModel (Core)

```csharp
// Src/MetaForge.Core/Verification/VerificationModel.cs

/// <summary>
/// Typovaný model verifikace — co a jak ověřit.
/// Oddělený od Core elementů samotných.
/// </summary>
public sealed record VerificationModel
{
    public string ElementId { get; init; } = string.Empty;
    public VerificationKind Kind { get; init; }
    public IReadOnlyList<VerificationScenario> Scenarios { get; init; } = [];
}

public enum VerificationKind
{
    SyntaxOnly,     // Jen syntaktická validace
    CompileOnly,    // Syntax + kompilace
    BehaviorFull    // Syntax + kompilace + spuštění testů
}

public sealed record VerificationScenario
{
    public string Name { get; init; } = string.Empty;
    public ScenarioExpectation Expectation { get; init; } = new();
    public IReadOnlyDictionary<string, object?> Inputs { get; init; }
        = new Dictionary<string, object?>();
}

public sealed record ScenarioExpectation
{
    public bool ShouldSucceed { get; init; } = true;
    public string? ExpectedException { get; init; }
    public object? ExpectedReturnValue { get; init; }
    public IReadOnlyList<string> ExpectedDiagnostics { get; init; } = [];
}
```

### 6.3 VerificationStateStore (Infrastructure)

```csharp
// Src/MetaForge.Infrastructure/Verification/VerificationState.cs

public enum VerificationState
{
    Unknown,    // Nikdy neběželo
    Running,    // Právě probíhá
    Passed,     // Fingerprint sedí, poslední běh OK
    Failed,     // Fingerprint sedí, poslední běh selhal
    Stale       // Element nebo pravidla se změnily, nutno znovu
}

public sealed record VerificationRecord
{
    public string ElementId { get; init; } = string.Empty;
    public string Fingerprint { get; init; } = string.Empty;
    public VerificationState State { get; init; }
    public DateTimeOffset LastVerified { get; init; }
    public string? FailureDiagnostics { get; init; }
}
```

```csharp
// Src/MetaForge.Infrastructure/Verification/IVerificationStateStore.cs

public interface IVerificationStateStore
{
    Task<VerificationRecord?> GetAsync(string elementId, CancellationToken ct = default);
    Task SetAsync(VerificationRecord record, CancellationToken ct = default);
    Task InvalidateAsync(string elementId, string reason, CancellationToken ct = default);
}
```

### 6.4 Rozšíření ElementFingerprint (PROP-039)

```csharp
// Rozšíření existujícího ElementFingerprint z PROP-039
// Původní: StructuralHash + PipelineVersion
// Nově: StructuralHash + ContractHash + GeneratorVersion + VerificationSchemaVersion

public sealed record ElementFingerprint
{
    // ... existující pole z PROP-039 ...
    public string StructuralHash { get; init; }     // PROP-039 — struktura elementu
    public string PipelineVersion { get; init; }    // PROP-039 — verze pipeline

    // === NOVÉ pro PROP-057 ===
    public string? ContractHash { get; init; }      // Hash ElementContractu (null = nemá kontrakt)
    public string? GeneratorVersion { get; init; }  // Verze generátoru, který element zpracoval
    public string? VerificationSchemaVersion { get; init; } // Verze verification schema
}
```

### 6.5 Napojení na ClassElement a MethodElement

```csharp
// ClassElement — nová volitelná property
public EntityContract? Contract { get; init; }

// MethodElement — nová volitelná property
public MethodContract? Contract { get; init; }
```

Obě property jsou **volitelné** (`null` = element nemá explicitní kontrakt).
Kontrakt vzniká během **Translator fáze** (Business → Core), může být:
- Ručně definovaný v BusinessAuthoringDocument
- Odvozený z `StrongType` pravidel (AI constraint inference)
- Prázdný/null (žádný kontrakt)

---

## 7. Implementační dopad

### Nové soubory

| Soubor | Obsah |
|--------|-------|
| `Src/MetaForge.Core/Contracts/ElementContract.cs` | `ElementContract`, `ContractInvariant`, `CanonicalExample` |
| `Src/MetaForge.Core/Contracts/EntityContract.cs` | `EntityContract`, `PropertyRule`, `RelationConstraint` |
| `Src/MetaForge.Core/Contracts/MethodContract.cs` | `MethodContract`, `ParameterExpectation`, `OutputExpectation`, `SideEffectHint`, `ScenarioHint`, `ScenarioKind` |
| `Src/MetaForge.Core/Verification/VerificationModel.cs` | `VerificationModel`, `VerificationScenario`, `ScenarioExpectation`, `VerificationKind` |
| `Src/MetaForge.Core/Verification/VerificationState.cs` | `VerificationState` enum, `VerificationRecord` |
| `Src/MetaForge.Infrastructure/Verification/IVerificationStateStore.cs` | Interface |
| `Src/MetaForge.Infrastructure/Verification/VerificationStateStore.cs` | File-based implementace |

### Změněné soubory

| Soubor | Změna |
|--------|-------|
| `Src/MetaForge.Core/Elements/ClassElement.cs` | Přidat `EntityContract? Contract` property |
| `Src/MetaForge.Core/Elements/MethodElement.cs` | Přidat `MethodContract? Contract` property |
| `Src/MetaForge.Core/Composability/ElementFingerprint.cs` | Přidat `ContractHash`, `GeneratorVersion`, `VerificationSchemaVersion` |

### API a kontrakty

- **ClassElement, MethodElement**: nové volitelné property (opt-in, backward compatible)
- **ElementFingerprint**: rozšíření existujícího — existující klienti (dirty-tracking) musí být updatováni, ale nová pole mají default `null`
- **MetadataBag**: beze změny — kontrakt je doplněk, ne náhrada

### Testy

| Test | Typ | Ověřuje |
|------|-----|---------|
| `ElementContract_Create_Entity` | Unit | EntityContract lze vytvořit s pravidly |
| `ElementContract_Create_Method` | Unit | MethodContract lze vytvořit se scénáři |
| `Fingerprint_IncludesContractHash` | Unit | Změna kontraktu → změna fingerprintu |
| `Fingerprint_SameInput_SameOutput` | Unit | Deterministický výpočet |
| `VerificationStateStore_CRUD` | Unit | Get/Set/Invalidate workflow |
| `VerificationStateStore_StaleOnFingerprintChange` | Unit | Změna fingerprintu → Stale |

---

## 8. Implementační fáze

### Fáze 1: ElementContract base (1–2 dny)

- `ElementContract`, `EntityContract`, `MethodContract` záznamy v Core
- `ContractInvariant`, `CanonicalExample`, `PropertyRule`, `ParameterExpectation` atd.
- Napojení na `ClassElement.Contract` a `MethodElement.Contract` (volitelné, default `null`)
- Základní validace kontraktů (povinná pole)

### Fáze 2: VerificationModel + Fingerprint rozšíření (1 den)

- `VerificationModel`, `VerificationScenario`, `ScenarioExpectation`
- `VerificationState` enum, `VerificationRecord`
- Rozšíření `ElementFingerprint` o 3 nová pole

### Fáze 3: VerificationStateStore (1 den)

- `IVerificationStateStore` interface
- File-based implementace v Infrastructure
- Integrace s existujícím patternem (checkpointy, `IProjectionCache`)

---

## 9. Otevřené otázky

- **OQ-057-01**: Má být `ElementContract` povinný pro každý element, nebo volitelný?
  - *Doporučení: volitelný v Fázi 1, vyhodnotit podle adopce.*
- **OQ-057-02**: Jak hluboko má sahat `PropertyContract` — má každá property mít vlastní kontrakt?
  - *Doporučení: Fáze 1 řeší property rules přes `EntityContract.PropertyRules`. Samostatný `PropertyContract` až ve Fázi 2.*
- **OQ-057-03**: Má se `VerificationStateStore` integrovat s `IProjectionCache`, nebo být samostatný?
  - *Doporučení: samostatný — jiný lifecycle, jiná data.*

---

## 10. Rizika a trade-offy

- **Riziko přehnané abstrakce**: ElementContract se stane "všechno musí mít kontrakt" a zablokuje rychlý prototyping.
  - *Mitigace: Kontrakt je volitelný (`null`). Bez kontraktu = verifikace se nedá skipovat (vždy `Unknown`).*
- **Riziko pomalého fingerprintu**: Výpočet hashe u velkých modelů.
  - *Mitigace: Cacheovat fingerprinty, počítat inkrementálně. PROP-039 již má `StructuralHash` — rozšíření je levné.*
- **Vědomý kompromis**: `PropertyContract` odložen na Fázi 2, ale `PropertyRule` je už součástí `EntityContract`.
- **Nekoliduje s PROP-039**: `ElementFingerprint` se rozšiřuje, ne přepisuje. Původní `StructuralHash + PipelineVersion` zůstává pro dirty-tracking.

---

## 11. Validace

- **Build**: `dotnet build` projde (rozšíření existujících typů, ne breaking změny)
- **Testy**: 6 unit testů (viz sekce Testy)
- **Smoke scénář**: Vytvoř `ClassElement` s `EntityContract`, vypočti fingerprint, změň kontrakt → fingerprint se změní
- **Ruční kontrola**: ElementContract musí být čitelný a srozumitelný — ne přehnaně abstraktní. Porovnat s `StrongType` patternem pro konzistenci.

---

## 12. Výsledek po dokončení

*(Vyplní se při uzavření návrhu.)*
