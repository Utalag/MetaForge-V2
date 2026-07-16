# PROP-057: ElementContract + VerificationModel

Typ výsledku: Candidate Proposal
Zdroj podnětu: Perplexity konverzace e2801d78 (2026-07-16) — návrhy #1, #2
Stav životního cyklu: Candidate
Rozhodovací owner:
Poslední revize: 2026-07-16 (elegantní revize — zjednodušení na 7 typů)

Priorita: High
Oblast: Core, BusinessModel, Infrastructure
Owner:
Datum vytvoření: 2026-07-16
Aktualizováno: 2026-07-16

Navazuje na:
- **PROP-024** (StrongType + Expression Record — hotovo) — `StrongType.ValidationRules` jako zdroj property-level constraintů
- **PROP-036** (Core Specification Layer — hotovo) — `InvariantDefinition` a `InvariantExpression` jako zdroj pravidel
- **PROP-039** (Core Composability — hotovo) — `ElementFingerprint` a `PipelineVersion` pro verifikační cache

Blokuje:
- **PROP-058** (Sandbox Preview Runner) — potřebuje `ElementContract` pro definici scénářů a `VerificationStateStore` pro skipování
- **PROP-059** (Resilience & Healing Layer) — potřebuje `VerificationModel` pro určení, co je ještě validní oprava

Související IDEAs:
- **IDEA-008** (Incremental Dirty-Tracking) — fingerprint overlap → Fáze 2
- **IDEA-010** (SymbolTable) — sémantická verifikace přes Roslyn SemanticModel
- **IDEA-016** (Output Readiness) — Fáze 4 syntéza
- **IDEA-018** (Execution Trace) — přes `ILogger`, ne samostatný recorder
- **IDEA-029** (ProductionHub) — akceptační testy

---

## 1. Kontext

MetaForge je **metadata-first** platforma. `StrongType` (PROP-024) nese `ValidationRules` — typ není jen jméno, ale významový objekt s kontraktem. `InvariantDefinition` (PROP-036) deklaruje pravidla. `ElementFingerprint` (PROP-039) detekuje změny.

**Mezera**: Chybí jednotný koncept "sémantického kontraktu elementu". `MetadataBag` je moc volný, `StrongType` pattern je moc úzký (jen value objekty), `InvariantDefinition` je moc technický.

**Klíčový princip**: Constrainty patří typům a property elementům. **Kontrakt je NEDuplikuje.** `EntityContract` nese jen cross-property invarianty. `MethodContract` nese jen cross-parameter invarianty, side effects a scénáře. Parametry dědí validaci ze svých typů.

---

## 2. Problém dnes

- Není jednotný způsob, jak element deklaruje "co je pro měj validní".
- `MetadataBag` je pro verifikační účely moc měkký (string klíče, žádná typová bezpečnost).
- Chybí fingerprint-based verifikační stavy pro skipování.
- Verifikační stav nemá kde být uložen (Core je read-only, musí být v Infrastructure).
- `object?` v hodnotách scénářů rozbíjí serializaci, hash a sandbox invocation.

---

## 3. Architektonické invarianty

- **BusinessAuthoringDocument zůstává source of truth.** ElementContract vzniká přes Translator.
- **Core je read-only derivace.**
- **Verifikační stav NENÍ v Core** — je v Infrastructure (pattern jako checkpoint caching).
- **AI je volitelná.**
- **Constrainty patří typům, ne kontraktu.** `Spz` vlastní regex. `EntityContract` vlastní "Elektromobil nesmí mít nádrž".
- **Reference přes stabilní ID.** `DisplayName` je debug-only, neautoritativní.

---

## 4. Scope — revidováno

### In scope (Fáze 1 — 7 typů v Core)

| Typ | Popis |
|-----|-------|
| `ContractValue` | Typovaný nosič hodnoty — sealed record potomci (String, Int32, Decimal, Boolean, Guid, DateTimeOffset, Null, Enum, StrongType) |
| `ElementContract` | Abstraktní base — invarianty, scénáře, metadata |
| `EntityContract` | **Pouze** cross-property invarianty + vztahové constrainty. Property-level validace patří StrongType. |
| `MethodContract` | **Pouze** cross-parameter invarianty, output očekávání, side effects, scénářové hinty. Parametry dědí validaci z typů. |
| `ContractInvariant` | Odkaz na `InvariantDefinition` (PROP-036) |
| `ContractScenario` | **Unifikovaný** scénář pro entity i metody — `InputsByElementId` s `ContractValue`, `DisplayNameSnapshots` pro debug |
| `ScenarioExpectation` | `ShouldSucceed`, `ExpectedReturnValue` (`ContractValue`), `ExpectedException` |

Infrastructure:
| Typ | Popis |
|-----|-------|
| `VerificationState` | Unknown / Running / Passed / Failed / Stale |
| `VerificationRecord` | Stav + fingerprint + timestamp |
| `IVerificationStateStore` | Get/Set/Invalidate |

### Out of scope

- `PropertyContract` (Fáze 2)
- Automatické odvozování scénářů z pravidel (→ PROP-060)
- Sandbox execution (→ PROP-058)
- Healing (→ PROP-059)
- Komplexní hodnoty v `ContractValue` (Fáze 1 jen skaláry)

### Co bylo odstraněno oproti původnímu návrhu

| Odstraněno | Důvod |
|------------|-------|
| `PropertyRule` + `EntityContract.PropertyRules` | Constrainty patří typům. Duplikovalo by `StrongType.ValidationRules`. |
| `ParameterExpectation` + `MethodContract.InputContract` | Parametry dědí validaci z typů. |
| `ContractConstraint` (bridge) | Zbytečný prostředník — přímý `InvariantDefinitionId`. |
| `VerificationKind` enum | Odvoditelné z dat (má scénáře → BehaviorFull). |
| `CanonicalExample` + `VerificationScenario` (2 typy) | Sloučeno do `ContractScenario` (stejná struktura). |
| `GeneratorVersion` + `VerificationSchemaVersion` (2 pole) | Nahrazeno existujícím `PipelineVersion` (PROP-039). |

---

## 5. Návrh řešení

### 5.1 ContractValue — typovaný nosič hodnot

```csharp
// Src/MetaForge.Core/Contracts/ContractValue.cs

/// <summary>
/// Typovaný nosič konkrétní hodnoty pro kontrakty a verifikační scénáře.
/// Sealed record potomci — nelze omylem vytvořit hodnotu s duálním typem.
/// Fáze 1: pouze skalární C# hodnoty.
/// </summary>
public abstract record ContractValue
{
    public required TypeModel Type { get; init; }

    public sealed record Null(TypeModel Type) : ContractValue;
    public sealed record String(string Value) : ContractValue { public String() : this("") { } }
    public sealed record Int32(int Value) : ContractValue;
    public sealed record Decimal(decimal Value) : ContractValue;
    public sealed record Boolean(bool Value) : ContractValue;
    public sealed record Guid(Guid Value) : ContractValue;
    public sealed record DateTimeOffset(DateTimeOffset Value) : ContractValue;
    public sealed record Enum(string TypeName, string Value) : ContractValue;
    public sealed record StrongType(string TypeReferenceId, string SerializedValue) : ContractValue;
}
```

### 5.2 ElementContract — sémantický kontrakt elementu

```csharp
// Src/MetaForge.Core/Contracts/ElementContract.cs

/// <summary>
/// Sémantický kontrakt elementu.
/// Zobecňuje StrongType pattern: element nese svůj význam, invarianty a scénáře.
/// ⚠️ NEDuplikuje constrainty typů/property. Ty patří StrongType / PropertyElement.
/// </summary>
public abstract record ElementContract
{
    public string ElementId { get; init; } = string.Empty;

    /// <summary>Cross-property / cross-parameter invarianty.</summary>
    public IReadOnlyList<ContractInvariant> Invariants { get; init; } = [];

    /// <summary>Unifikované scénáře — validní i nevalidní příklady.</summary>
    public IReadOnlyList<ContractScenario> Scenarios { get; init; } = [];

    public MetadataBag Metadata { get; init; } = new();
}

public record ContractInvariant
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    /// <summary>Odkaz na InvariantDefinition (PROP-036).</summary>
    public string? InvariantDefinitionId { get; init; }
    public ValidationSeverity Severity { get; init; } = ValidationSeverity.Error;
}
```

### 5.3 EntityContract — pouze cross-property invarianty

```csharp
// Src/MetaForge.Core/Contracts/EntityContract.cs

/// <summary>
/// Kontrakt pro entitu (ClassElement).
/// ⚠️ NEObsahuje PropertyRule! Constrainty jednotlivých property patří StrongType / PropertyElement.
/// EntityContract nese jen:
/// - Cross-property invarianty ("Elektromobil nesmí mít objem nádrže")
/// - Vztahové constrainty
/// - Scénáře s hodnotami podle PropertyId
/// </summary>
public sealed record EntityContract : ElementContract
{
    public IReadOnlyList<RelationConstraint> RelationConstraints { get; init; } = [];
}

public record RelationConstraint
{
    public string RelationId { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string Description { get; init; } = string.Empty;
}
```

### 5.4 MethodContract — pouze cross-parameter invarianty

```csharp
// Src/MetaForge.Core/Contracts/MethodContract.cs

/// <summary>
/// Kontrakt pro metodu (MethodElement).
/// ⚠️ NEObsahuje validace parametrů! Parametry dědí validaci ze svých typů (StrongType).
/// MethodContract nese jen:
/// - Cross-parameter invarianty ("startDate < endDate")
/// - Output/postcondition očekávání
/// - Side effects
/// - Scénářové hinty — složené ze vstupů podle ParameterId
/// </summary>
public sealed record MethodContract : ElementContract
{
    public OutputExpectation? OutputContract { get; init; }
    public IReadOnlyList<SideEffectHint> SideEffects { get; init; } = [];
    public IReadOnlyList<ScenarioHint> ScenarioHints { get; init; } = [];
}

public record OutputExpectation
{
    public string? Description { get; init; }
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

### 5.5 ContractScenario — unifikovaný scénář

```csharp
// Src/MetaForge.Core/Contracts/ContractScenario.cs

/// <summary>
/// Unifikovaný scénář — slouží pro entity (CanonicalExample) i metody (VerificationScenario).
/// Stejná struktura, jiný sémantický kontext.
/// </summary>
public sealed record ContractScenario
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }

    /// <summary>Hodnoty podle stabilního ElementId (PropertyId / ParameterId).
    /// ⚠️ Klíč = ID, ne jméno. Jméno je v DisplayNameSnapshots.</summary>
    public IReadOnlyDictionary<string, ContractValue> InputsByElementId { get; init; }
        = new Dictionary<string, ContractValue>();

    /// <summary>Debug-only snapshot jmen v čase vytvoření scénáře. Neautoritativní.</summary>
    public IReadOnlyDictionary<string, string> DisplayNameSnapshots { get; init; }
        = new Dictionary<string, string>();

    public ScenarioExpectation Expectation { get; init; } = new();
}

public sealed record ScenarioExpectation
{
    public bool ShouldSucceed { get; init; } = true;
    public string? ExpectedException { get; init; }
    public ContractValue? ExpectedReturnValue { get; init; }
    public IReadOnlyList<string> ExpectedDiagnostics { get; init; } = [];
}
```

### 5.6 Příklad: Auto entita

```csharp
// === Hodnotové objekty — vlastní svá validační pravidla ===
var spz = new StrongType("Spz", TypeModel.String, [
    new ValueObjectValidationRule("Required"),
    new ValueObjectValidationRule("Regex", "^[0-9A-Z ]{5,8}$")
]);

// === EntityContract — POUZE cross-property invarianty ===
var autoContract = new EntityContract
{
    ElementId = "entity:auto:8d7f",
    Invariants = [
        new ContractInvariant
        {
            Name = "ElektromobilNemaNadrz",
            Description = "Je-li pohon elektromobil, objem nádrže musí být null.",
            InvariantDefinitionId = "invariant:auto:elektromobil-bez-nadrze",
            Severity = ValidationSeverity.Error
        }
    ]
    // ⚠️ Žádné PropertyRules! Required/Regex patří StrongType.
};

// === Scénáře — hodnoty přes ContractValue, klíče přes PropertyId ===
var validniAuto = new ContractScenario
{
    Name = "Běžné platné auto",
    Expectation = new() { ShouldSucceed = true },
    InputsByElementId = new()
    {
        ["property:auto:znacka:01"] = new ContractValue.String("Škoda"),
        ["property:auto:spz:02"] = new ContractValue.String("1AB 2345"),
        ["property:auto:rok-vyroby:03"] = new ContractValue.Int32(2024)
    },
    DisplayNameSnapshots = new()
    {
        ["property:auto:znacka:01"] = "Značka",
        ["property:auto:spz:02"] = "SPZ"
    }
};

var autoBezSpz = new ContractScenario
{
    Name = "Auto bez SPZ — NEVALIDNÍ",
    Expectation = new() { ShouldSucceed = false },
    InputsByElementId = new()
    {
        ["property:auto:spz:02"] = new ContractValue.Null(TypeModel.String)
    }
};
```

### 5.7 VerificationStateStore (Infrastructure)

```csharp
// Src/MetaForge.Infrastructure/Verification/

public enum VerificationState { Unknown, Running, Passed, Failed, Stale }

public sealed record VerificationRecord
{
    public string ElementId { get; init; } = string.Empty;
    public string Fingerprint { get; init; } = string.Empty;
    public VerificationState State { get; init; }
    public DateTimeOffset LastVerified { get; init; }
    public string? FailureDiagnostics { get; init; }
    public string? DisplayNameSnapshot { get; init; }  // debug-only
}

public interface IVerificationStateStore
{
    Task<VerificationRecord?> GetAsync(string elementId, CancellationToken ct = default);
    Task SetAsync(VerificationRecord record, CancellationToken ct = default);
    Task InvalidateAsync(string elementId, string reason, CancellationToken ct = default);
}
```

### 5.8 Fingerprint — znovupoužití PROP-039

```csharp
// Rozšíření existujícího ElementFingerprint (PROP-039)
// Původní: StructuralHash + PipelineVersion
// Nově přidáno: ContractHash (null = element nemá kontrakt)

public sealed record ElementFingerprint
{
    public string StructuralHash { get; init; }     // PROP-039
    public string PipelineVersion { get; init; }    // PROP-039 — zahrnuje generator i verification schema
    public string? ContractHash { get; init; }      // NOVÉ — hash ElementContractu
}
```

### 5.9 Napojení na ClassElement a MethodElement

```csharp
// ClassElement — volitelná property
public EntityContract? Contract { get; init; }

// MethodElement — volitelná property
public MethodContract? Contract { get; init; }
```

---

## 6. Implementační dopad

### Nové soubory (7 typů v Core + Infrastructure)

| Soubor | Obsah |
|--------|-------|
| `Src/MetaForge.Core/Contracts/ContractValue.cs` | `ContractValue` + 9 sealed potomků |
| `Src/MetaForge.Core/Contracts/ElementContract.cs` | `ElementContract`, `ContractInvariant` |
| `Src/MetaForge.Core/Contracts/EntityContract.cs` | `EntityContract`, `RelationConstraint` |
| `Src/MetaForge.Core/Contracts/MethodContract.cs` | `MethodContract`, `OutputExpectation`, `SideEffectHint`, `ScenarioHint` |
| `Src/MetaForge.Core/Contracts/ContractScenario.cs` | `ContractScenario`, `ScenarioExpectation` |
| `Src/MetaForge.Infrastructure/Verification/VerificationState.cs` | `VerificationState`, `VerificationRecord` |
| `Src/MetaForge.Infrastructure/Verification/IVerificationStateStore.cs` | Interface |

### Změněné soubory

| Soubor | Změna |
|--------|-------|
| `Src/MetaForge.Core/Elements/ClassElement.cs` | `EntityContract? Contract` |
| `Src/MetaForge.Core/Elements/MethodElement.cs` | `MethodContract? Contract` |
| `Src/MetaForge.Core/Composability/ElementFingerprint.cs` | `ContractHash` |

### Testy (6 unit testů)

| Test | Ověřuje |
|------|---------|
| `ContractValue_String_Roundtrips` | JSON serializace/deserializace |
| `ContractValue_SameValue_SameHash` | Deterministický hash |
| `ContractScenario_Entity_Auto` | Scénář pro entitu s ContractValue |
| `ContractScenario_Method_Inputs` | Scénář pro metodu |
| `VerificationStateStore_CRUD` | Get/Set/Invalidate |
| `Fingerprint_IncludesContractHash` | Změna kontraktu → změna fingerprintu |

---

## 7. Implementační fáze

### Fáze 1: ContractValue + ElementContract (1–2 dny)
- `ContractValue` se všemi skalárními potomky
- `ElementContract`, `EntityContract`, `MethodContract`
- `ContractScenario`, `ScenarioExpectation`
- Napojení na `ClassElement.Contract` a `MethodElement.Contract` (volitelné, default `null`)

### Fáze 2: Fingerprint + VerificationStateStore (1 den)
- Rozšíření `ElementFingerprint` o `ContractHash`
- `VerificationState` + `VerificationRecord` + `IVerificationStateStore`
- File-based implementace

---

## 8. Otevřené otázky

- **OQ-057-01**: Mají všechny method/property/parameter elementy stabilní ID napříč Business → Translator → Core?
  - *Pokud ne → nutný enabling PROP pro identity před ID-first referencemi.*
- **OQ-057-02**: Má být `ElementContract` povinný?
  - *Doporučení: volitelný. Bez kontraktu = `ContractHash = null`, verifikace vždy `Unknown`.*

---

## 9. Rizika

- **Riziko: dvojí zdroj pravdy** — kontrakt se nesmí měnit přímo v Core. Musí vznikat z Business modelu přes Translator.
- **Riziko: `object?`** — eliminováno zavedením `ContractValue`.
- **Riziko: duplicitní constrainty** — eliminováno: constrainty patří typům, kontrakt jen odkazuje přes `InvariantDefinitionId`.

---

## 10. Validace

- **Build**: `dotnet build` projde
- **Testy**: 6 unit testů
- **Smoke**: Vytvoř `EntityContract` pro `Auto`, `ContractScenario` s `ContractValue`, ověř fingerprint