# PROP-061: Authoring Feedback Service, Active Cache a Learning Archive

> **Stav:** 📝 Candidate
> **Datum:** 2026-07-18
> **Autor:** Copilot (mf-planner)
> **Oblast:** Průřezové (Feedback, Translator, Core, Generators, AI, Infrastructure, CLI, MCP)
> **Odhad:** 15–25 dní (6 fází)
> **Zdroj:** Konverzace nad CODE-004 → rozšíření na cross-cutting feedback capability
> **Motivace:** Dnes diagnostiky vznikají v různých vrstvách (`Diagnostic` v Core, `DiagnosticInfo` v Generators, `ValidationIssue` v Core, `BusinessValidationIssue` v BusinessModel), ale chybí jednotný kanál k uživateli, dočasná cache pro rychlé zobrazení a anonymizovaný learning archive pro zlepšování Translatoru a AI.
> **Závislosti:** PROP-056 (DocumentProjection — unifikovaná projekce), PROP-060 (Element Identity — `Guid Id`, `IMemberElement.Id`, `ElementIdMapping`)
> **Absorbuje:** CODE-004 (MapType TODO Cleanup + Generator Diagnostics) — původní soubor přesunut do `Docs/Plans/Implemented/`.

## Cíl

1. **Sjednotit diagnostiku** z Generatoru, Translatoru a TransformPipeline do jednoho kanonického kanálu k uživateli
2. **Zavést `AuthoringFeedbackService`** v novém projektu `MetaForge.Feedback` jako cross-cutting agregační službu
3. **Přidat `ActiveFeedbackCache`** — dočasnou per-project cache pro zobrazení otevřených warningů bez nutnosti full pipeline runu
4. **Přidat volitelnou AI repair vrstvu** — z téhož feedbacku AI navrhne až 3 opravy ke schválení (nikdy auto-apply)
5. **Přidat `FeedbackLearningArchive`** — anonymizovaný, consent-based export uzavřených hlášení pro zlepšování Translatoru a AI
6. **Všechny změny do BusinessAuthoringDocument jdou přes Facade/PatchEngine/CommandLog** — feedback je read-only, oprava je perzistentní

## Motivace

### Dnešní stav — fragmentovaná diagnostika

V ekosystému MetaForge dnes existují **4 různé diagnostické typy**, z nichž ani jeden není sjednocený kanál k uživateli:

| Typ | Vrstva | Kolekce | Klíčové pole | Co umí |
|-----|--------|---------|-------------|--------|
| `Diagnostic` | Core | `DiagnosticBag` | `Code`, `ElementPath`, `SuggestedFix` | Nejbohatší — target pro kanonický typ |
| `DiagnosticInfo` | Generators | `GeneratedCodeArtifact.Diagnostics` | `ElementId`, `ElementName` | Chudší — chybí `Code`, `ElementPath` |
| `ValidationIssue` | Core | `CoreValidator.Validate()` | `Code`, `Category` | Specifický pro Core validaci |
| `BusinessValidationIssue` | BusinessModel | `BusinessDocumentValidator` | `Code`, `Path`, `Suggestion` | Specifický pro Business validaci |

**Důsledky:**
- CLI dnes ignoruje `artifact.Diagnostics` — uživatel nevidí varování z generátoru
- Translator enrichment selhání (nerozlišený typ) se nepropisuje do žádného user-facing kanálu
- `DiagnosticBag` z Core validace se ztrácí po skončení pipeline
- AI enrichment pracuje bez strukturované diagnostické zpětné vazby — neví, co přesně selhalo
- Chybí mechanismus "otevři projekt a hned vidím, co je špatně" — diagnostika se počítá znovu při každé akci

### Cílový stav — sjednocený feedback

```
Translator ──→ DiagnosticBag ──┐
Core/Pipeline ──→ DiagnosticBag ──┤
Generator ──→ DiagnosticInfo ──┤
                               ↓
                   AuthoringFeedbackService
                   (agregace, deduplikace)
                        │
          ┌─────────────┼─────────────┐
          ↓             ↓             ↓
   ActiveFeedbackCache  CLI/MCP/Chat  AI (volitelné)
   (per-project JSON)   (host výstup)  Repair Suggestions
          │                            (max 3, ke schválení)
          ↓
   FeedbackLearningArchive
   (anonymizovaný, consent export)
```

## Architektonická rozhodnutí

### 1. Kanonický typ: `Diagnostic` (Core)

**Rozhodnutí:** `AuthoringFeedbackRecord` **obaluje** `Diagnostic` (Core), nevytváří pátý diagnostický typ. `DiagnosticInfo` (Generators) bude rozšířen o `Code` a `ElementPath`, aby dosáhl parity s `Diagnostic`.

**Důvod:** V ekosystému už existují 4 diagnostické typy. Přidání pátého by prohloubilo fragmentaci. `Diagnostic` (Core) má `Code`, `ElementPath`, `SuggestedFix` — je nejbohatší a nejsprávnější volba pro kanonický typ.

### 2. Layer diagram — kdo zná koho

```
MetaForge.Feedback  (zná Core, Translator, Generators — konzumuje jejich výstupy)
       ↑
       │ čte
       │
MetaForge.Translator    (zapisuje do DiagnosticBag, NEinjekuje Feedback)
MetaForge.Core          (zapisuje do DiagnosticBag)
MetaForge.Generators    (zapisuje do DiagnosticBag přes AsyncLocal)
```

**Rozhodnutí:** Translator/Core/Generators **zapisují diagnostiku do `DiagnosticBag`** (ambient/event pattern), `AuthoringFeedbackService` ji **čte**. Tím se vyhne porušení vrstvení — nižší vrstvy neznají vyšší.

### 3. Write-back: existující `SetCoreDetailOp`

**Rozhodnutí:** Repair suggestions používají pro write-back existující `SetCoreDetailOp` (Translator), ne nový `PatchOperationDraft`. Návrh opravy je `RepairRecommendation` s polem `SetCoreDetailOp[]`.

**Důvod:** `IPatchOperation` a `SetCoreDetailOp` už existují jako součást Translator write-back kanálu. Není důvod vytvářet paralelní operaci.

### 4. Projektní expozice: `DocumentProjection` (PROP-056)

**Rozhodnutí:** Fáze 3 cílí `DocumentProjection` (PROP-056), ne `ProjectionView` (deprecated).

**Důvod:** PROP-056 zavádí unifikovanou projekci s `CoreId` traceabilitou. `ProjectionView` je deprecated.

### 5. Infrastruktura: cache v `MetaForge.Infrastructure`

**Rozhodnutí:** `IFeedbackCacheRepository` a `IFeedbackLearningRepository` (kontrakty i implementace) jsou v `MetaForge.Infrastructure/Caching/`, používají `IOptions<StorageOptions>` a `FileSystemProvider`.

**Důvod:** Infrastructure už má `CheckpointProjectionCache`, `IVerificationStateStore`, `FileSystemProvider` — stejný pattern. Feedback cache je technická podpůrná persistence, ne doménový stav.

### 6. AI: nové rozhraní v Translator vrstvě

**Rozhodnutí:** `IRepairSuggestionService` je definováno v Translator vrstvě (vedle `ITranslationService`), implementace v `MetaForge.Ai`.

**Důvod:** AI repair je enrichment workflow, ne feedback workflow. Patří do Translator/AI vrstvy. `AuthoringFeedbackService` poskytuje **kontext** pro AI, ale samotné generování návrhů je AI odpovědnost.

## Fáze

### Fáze 0 — Generator diagnostics (absorbed CODE-004)

**Cíl:** Odstranit TODO komentáře z `MapDataType`, přidat správné mapování pro Array/Nullable, přidat diagnostiku pro nerozpoznané typy.

**Změny:** Vše v `Src/MetaForge.Generators/CodeGenerator.cs` — 5 editačních míst:
1. `AsyncLocal<List<Diagnostic>>` ambientní kolektor + `BeginTypeDiagnostics()` / `EndTypeDiagnostics()`
2. `Generate()` — wire-up kolektoru (begin/end)
3. `MapType()` — nové cesty pro `DataType.Array` (`T[]`), `DataType.Nullable` (`T?`), diagnostika
4. `MapDataType()` — odstranění 5 TODO/komentářů, čisté `"object"` fallbacky
5. `IsKnownPrimitive()` — pomocná metoda pro detekci známých typů (23 hodnot `DataType`)

**Testy:** `Tests/MetaForge.Generators.Tests/CSharp/MapTypeDiagnosticsTests.cs` (10 unit testů)

**Výstup:** Generátor emituje `Diagnostic` (ne `DiagnosticInfo` — rozšířeno) do kolektoru, který `Generate()` připojí k `GeneratedCodeArtifact.Diagnostics`.

**Odhad:** 1 den

---

### Fáze 1 — Kontrakty + AuthoringFeedbackService + ActiveFeedbackCache

**Cíl:** Zavést nový projekt `MetaForge.Feedback`, definovat kontrakty, implementovat agregační službu a dočasnou cache.

**Nový projekt:** `Src/MetaForge.Feedback/MetaForge.Feedback.csproj`
- Závislosti: `MetaForge.Core`, `MetaForge.Translator`, `MetaForge.Generators`, `MetaForge.Infrastructure`, `Microsoft.Extensions.DependencyInjection.Abstractions`
- Target: `net9.0`

**Kontrakty:**

```csharp
// === MetaForge.Feedback/Models/AuthoringFeedbackRecord.cs ===
// Agregát nad Diagnostic (Core) — přidává per-project metadata

public sealed record AuthoringFeedbackRecord(
    Guid FeedbackId,
    string ProjectId,
    string? ElementId,          // IMemberElement.Id z PROP-060
    string ElementKind,         // Class, Property, Method, Enum, ...
    Diagnostic CoreDiagnostic,  // ← kanonický typ (Core)
    string Stage,               // translator | pipeline | generator | ai
    IReadOnlyList<RepairRecommendation> Suggestions,
    string Status,              // Open | Suggested | Resolved | Dismissed
    string Fingerprint,         // hash/fingerprint elementu
    DateTimeOffset FirstSeenUtc,
    DateTimeOffset LastSeenUtc,
    int OccurrenceCount
);

// === MetaForge.Feedback/Models/RepairRecommendation.cs ===
// Návrh opravy — read-only výstup pro uživatele/AI

public sealed record RepairRecommendation(
    int Rank,
    string Kind,                // deterministic | ai
    decimal Confidence,
    string Explanation,
    string Rationale,
    IReadOnlyList<FeedbackEvidence> Evidence
);

// === MetaForge.Feedback/Models/FeedbackEvidence.cs ===

public sealed record FeedbackEvidence(
    string Type,                // invariant | validator | generator | ai
    string Key,
    string Value
);

// === MetaForge.Feedback/IAuthoringFeedbackService.cs ===

public interface IAuthoringFeedbackService
{
    Task<AuthoringFeedbackSnapshot> GetCurrentAsync(string projectId, CancellationToken ct);
    Task MarkDismissedAsync(Guid feedbackId, CancellationToken ct);
    Task MarkResolvedAsync(Guid feedbackId, ResolutionInfo resolution, CancellationToken ct);
}

public sealed record AuthoringFeedbackSnapshot(
    IReadOnlyList<AuthoringFeedbackRecord> OpenItems,
    int TotalCount,
    int ErrorCount,
    int WarningCount
);

// === MetaForge.Infrastructure/Caching/IFeedbackCacheRepository.cs ===

public interface IFeedbackCacheRepository
{
    Task<IReadOnlyList<AuthoringFeedbackRecord>> LoadOpenAsync(string projectId, CancellationToken ct);
    Task UpsertAsync(AuthoringFeedbackRecord record, CancellationToken ct);
    Task RemoveAsync(Guid feedbackId, CancellationToken ct);
    Task InvalidateByElementAsync(string elementId, CancellationToken ct);
}

// === MetaForge.Infrastructure/Learning/IFeedbackLearningRepository.cs ===

public interface IFeedbackLearningRepository
{
    Task AppendAsync(FeedbackLearningRecord record, CancellationToken ct);
    Task<IReadOnlyList<FeedbackLearningRecord>> GetPendingExportAsync(CancellationToken ct);
    Task MarkExportedAsync(Guid learningId, CancellationToken ct);
}
```

**Implementace:**
- `AuthoringFeedbackService` — scoped, agreguje `Diagnostic` z:
  - `GeneratedCodeArtifact.Diagnostics` (Generator)
  - `DiagnosticBag` z Translator enrichment (`DiagnosticBag` se předává jako výstup `TranslateDocument`)
  - `DiagnosticBag` z TransformPipeline
- `JsonFeedbackCacheRepository` — JSON soubor per project, `IOptions<StorageOptions>.FeedbackCachePath`
- `ActiveFeedbackCache` — deduplikace přes `ElementId + Diagnostic.Code + Fingerprint`, invalidace při změně fingerprintu

**DI registrace:** `services.AddMetaForgeFeedback(builder.Configuration)` v `MetaForge.Feedback/DiRegistration.cs`

**Testy:** `Tests/MetaForge.Feedback.Tests/` (nový projekt) — unit testy `AuthoringFeedbackService`, integrační testy `JsonFeedbackCacheRepository`

**Odhad:** 4–5 dní

---

### Fáze 2 — Translator & Pipeline diagnostics

**Cíl:** Translator a TransformPipeline začnou emitovat strukturovanou diagnostiku, kterou `AuthoringFeedbackService` konzumuje.

**Změny:**

1. **Translator enrichment diagnostics** (`Src/MetaForge.Translator/DefaultBusinessTranslator.cs`):
   - Při `TranslateDocument()`: `DiagnosticBag` se naplní warningem, pokud `DataType.Entity` / `Struct` / `Record` nemá `CustomTypeName` a Translator neumí typ rozlišit
   - Nový `Diagnostic.Code`: `TL-001` (unresolved entity type), `TL-002` (unresolved struct/record type)
   - Výstup: `TranslateDocument()` vrací `(IReadOnlyList<RootElement>, DiagnosticBag)`

2. **Core/Pipeline diagnostics** (`Src/MetaForge.Core/TransformPipeline/`):
   - `BuildResult<T>` už podporuje `DiagnosticBag` — pipeline stage přidávají diagnostiku na `DiagnosticBag`
   - Nové `Diagnostic.Code`: `PL-001` (transform failed), `PL-002` (invariant violated)
   - `CoreValidator.Validate()` → `DiagnosticBag` (místo `IReadOnlyList<ValidationIssue>`)

3. **Unifikace `ValidationIssue` → `Diagnostic`:**
   - `CoreValidator` přestane vracet `ValidationIssue`, začne vracet `Diagnostic`
   - `BusinessDocumentValidator` se nemění — zůstává na `BusinessValidationIssue` (není součástí feedback pipeline)

4. **`DiagnosticInfo` (Generators) rozšíření:**
   - Přidat `Code` property (string) — pro paritu s `Diagnostic`
   - Přidat `ElementPath` (volitelné) — pro přesnější lokalizaci

**Výstup:** Všechny tři stage (Translator, Pipeline, Generator) emitují `Diagnostic` se strukturovaným `Code`, `Severity`, `ElementPath`.

**Odhad:** 3–4 dny

---

### Fáze 3 — Projektní expozice + host surfaces

**Cíl:** Feedback je viditelný v CLI, MCP a (volitelně) Chat.

**Změny:**

1. **`DocumentProjection` (PROP-056) rozšíření:**
   - Nová volitelná sekce `FeedbackSection` v `DocumentProjection`
   - Obsahuje `IReadOnlyList<AuthoringFeedbackRecord>`, filtrovatelné přes `ProjectionFilter`

2. **CLI** (`Src/MetaForge.Cli/Commands/FeedbackCommands.cs`):
   - `mf list feedback` — zobrazí otevřené warningy (tabulka s Code, Severity, Stage, ElementKind, Message)
   - `mf feedback dismiss <id>` — označí hlášení jako Dismissed
   - `mf feedback suggest <id>` — spustí AI repair suggestion (pokud je AI aktivní)
   - `mf feedback consent [on|off|status]` — správa learning export consentu

3. **MCP** (`Src/MetaForge.Mcp/Tools/FeedbackTools.cs`):
   - `get_feedback` — vrátí strukturovaný JSON se všemi otevřenými hlášeními
   - `dismiss_feedback` — označí hlášení jako Dismissed
   - `suggest_repair` — spustí AI repair suggestion a vrátí `RepairRecommendation[]`
   - `set_consent` — nastaví consent pro learning export

4. **Konfigurace:**
   ```json
   // appsettings.json — nové sekce
   {
     "Feedback": {
       "CacheEnabled": true,
       "CachePath": ".metaforge/feedback-cache/",
       "Consent": "NotAsked",  // NotAsked | Granted | Denied
       "LearningExportEndpoint": "https://api.metaforge.dev/v1/feedback"
     }
   }
   ```

**Odhad:** 3–4 dny

---

### Fáze 4 — AI suggestion layer

**Cíl:** AI může z feedbacku vytvořit 1–3 návrhy oprav s vysvětlením. Bez AI funguje systém beze změny.

**Nové rozhraní v Translator vrstvě:**

```csharp
// === MetaForge.Translator/IRepairSuggestionService.cs ===

public interface IRepairSuggestionService
{
    Task<IReadOnlyList<RepairRecommendation>> SuggestRepairsAsync(
        AuthoringFeedbackRecord feedback,
        BusinessAuthoringDocument document,
        DocumentProjection projection,
        CancellationToken ct);
}
```

**Implementace:**
- `AiRepairSuggestionService` v `MetaForge.Ai` — používá `IAiBackendAdapter`, `PromptRegistry`
- Nový `.prompt.md`: `Templates/repair-suggestion.prompt.md`
- Prompt obsahuje: `Diagnostic` (Code, Message, ElementPath), `BusinessAuthoringDocument` excerpt, relevantní `DocumentProjection`
- AI vrací JSON s polem `RepairRecommendation`
- `PromptEvaluator` pro testování kvality návrhů

**Graceful fallback:**
- Když AI není dostupná: `IRepairSuggestionService` vrací `Array.Empty<RepairRecommendation>()`
- Uživatel dostane systémové hlášení bez návrhů

**Bezpečnost:**
- AI **nikdy** neaplikuje změny automaticky
- Návrhy jsou read-only výstup pro uživatele ke schválení
- Schválený návrh jde přes `Facade.ApplyPatch()` s `SetCoreDetailOp` (existující operace)

**Odhad:** 3–4 dny

---

### Fáze 5 — LearningArchive + HTTP export + consent

**Cíl:** Uzavřené feedback záznamy jsou anonymizovány a připraveny k exportu. Export je opt-in se souhlasem uživatele.

**Kontrakt:**

```csharp
// === MetaForge.Feedback/Models/FeedbackLearningRecord.cs ===

public sealed record FeedbackLearningRecord(
    Guid LearningId,
    string DiagnosticCode,
    string Severity,
    string Stage,
    string ElementKind,
    string FingerprintHash,          // anonymizovaný hash
    bool AiUsed,
    bool AiSuggestionAccepted,
    int SuggestionCount,
    int? AcceptedSuggestionRank,
    string ResolutionKind,           // manual | ai-approved | dismissed | auto-resolved
    int IterationCount,
    long TimeToResolutionMs,
    string? PromptTemplateVersion,
    string? AiProvider,
    DateTimeOffset ClosedAtUtc,
    string ConsentState              // NotAsked | Granted | Denied | Exported
);
```

**Lifecycle:**
- `Open` → `Suggested` → `Resolved`/`Dismissed` → `Archived` (vznikne `FeedbackLearningRecord`) → `ReadyForExport` (anonymizace) → `Exported` (HTTP odeslání)
- `ActiveFeedbackCache` maže `Resolved`/`Dismissed` záznamy při příštím pipeline runu
- `FeedbackLearningArchive` záznamy perzistují do exportu

**Anonymizace (co se NExportuje):**
- ❌ Názvy entit, atributů, metod
- ❌ Raw notes, popisy
- ❌ Plný `BusinessAuthoringDocument` / `CommandLog` / snapshot
- ❌ `ElementId` v plain textu (pouze `FingerprintHash`)

**Co se exportuje:**
- ✅ `DiagnosticCode`, `Severity`, `Stage`
- ✅ `ElementKind` (ne `ElementName`)
- ✅ `FingerprintHash` (anonymizovaný)
- ✅ AI usage stats: `AiUsed`, `AiSuggestionAccepted`, `SuggestionCount`, `AcceptedSuggestionRank`
- ✅ `ResolutionKind`, `IterationCount`, `TimeToResolutionMs`
- ✅ `PromptTemplateVersion`, `AiProvider`

**HTTP export:**
- Endpoint: `POST {LearningExportEndpoint}` (konfigurovatelný)
- Payload: JSON pole `FeedbackLearningRecord[]`
- Retry: exponential backoff, max 3 pokusy
- Batch: odesílá se dávka uzavřených záznamů, ne jednotlivě

**Consent UX:**
- CLI: `mf feedback consent on` / `mf feedback consent off` / `mf feedback consent status`
- MCP: `set_consent { "consent": "Granted" | "Denied" }`
- Consent se ukládá do `appsettings.json` → `Feedback.Consent`
- Výchozí: `NotAsked` — dokud uživatel explicitně nepovolí, nic se neodesílá

**Testy:** `Tests/MetaForge.Feedback.Tests/` — anonymizace, consent flow, HTTP mock

**Odhad:** 4–5 dní

---

## Souhrn fází

| Fáze | Název | Odhad | Závislosti | Výstup |
|------|-------|-------|-----------|--------|
| **0** | Generator diagnostics (CODE-004) | 1 den | — | ✅ Hotovo (2026-07-18) |
| **1** | Kontrakty + Service + Cache | 4–5 dní | Fáze 0 | ✅ Hotovo (2026-07-18) |
| **2** | Translator & Pipeline diagnostics | 3–4 dny | Fáze 1 | ✅ Hotovo (2026-07-18) |
| **3** | Projektní expozice + hosty | 3–4 dny | Fáze 2 | ✅ Hotovo (2026-07-18) |
| **4** | AI suggestion layer | 3–4 dny | Fáze 3 | ✅ Hotovo (2026-07-18) |
| **5** | LearningArchive + export + consent | 4–5 dní | Fáze 3 | ✅ Hotovo (2026-07-18) |
| **Celkem** | | **18–23 dní** | | ✅ Dokončeno |

Fáze 0–3 tvoří **MVP** (~11–14 dní) — uživatel vidí feedback ve všech hostech.
Fáze 4–5 jsou **nástavba** (~7–9 dní) — AI návrhy a learning export.

## Soubory

### Nové projekty

| Projekt | Cesta | Obsah |
|---------|-------|-------|
| `MetaForge.Feedback` | `Src/MetaForge.Feedback/` | `AuthoringFeedbackService`, modely, `DiRegistration` |
| `MetaForge.Feedback.Tests` | `Tests/MetaForge.Feedback.Tests/` | Unit + integrační testy |

### Nové soubory v existujících projektech

| Projekt | Soubor | Fáze |
|---------|--------|------|
| Generators | `Tests/.../MapTypeDiagnosticsTests.cs` | 0 |
| Infrastructure | `Caching/IFeedbackCacheRepository.cs` | 1 |
| Infrastructure | `Caching/JsonFeedbackCacheRepository.cs` | 1 |
| Infrastructure | `Learning/IFeedbackLearningRepository.cs` | 1 |
| Infrastructure | `Learning/JsonFeedbackLearningRepository.cs` | 5 |
| Translator | `IRepairSuggestionService.cs` | 4 |
| AI | `AiRepairSuggestionService.cs` | 4 |
| AI | `Templates/repair-suggestion.prompt.md` | 4 |
| CLI | `Commands/FeedbackCommands.cs` | 3 |
| MCP | `Tools/FeedbackTools.cs` | 3 |

### Modifikované soubory

| Projekt | Soubor | Fáze |
|---------|--------|------|
| Generators | `CodeGenerator.cs` | 0 |
| Translator | `DefaultBusinessTranslator.cs` | 2 |
| Translator | `DocumentProjection.cs` (PROP-056) | 3 |
| Core | `TransformPipeline/*.cs` | 2 |
| Core | `CoreValidator.cs` | 2 |
| Core | `DiagnosticBag.cs` | 2 |
| Infrastructure | `StorageOptions.cs` | 1 |
| CLI | `appsettings.json` | 3,5 |
| CLI | `DiRegistration.cs` | 1 |
| MCP | `DiRegistration.cs` | 1 |

### Přesuny

| Z | Do | Důvod |
|---|----|-------|
| `Docs/Plans/CODE-004-MapType-TODO-Cleanup-Diagnostics.md` | `Docs/Plans/Implemented/CODE-004-MapType-TODO-Cleanup-Diagnostics.md` | Absorbován do PROP-061 |

### Meta soubory

| Soubor | Akce |
|--------|------|
| `PROPOSALS.md` | Přidat PROP-061 do Aktivní návrhy |
| `MetaForge.slnx` | Přidat `MetaForge.Feedback` a `MetaForge.Feedback.Tests` |

## Verifikace

| # | Příkaz / Kritérium | Očekávaný výsledek |
|---|--------------------|-------------------|
| 1 | `dotnet build` | 0 errors |
| 2 | `dotnet test --filter "MapType"` | 10+ testů projde |
| 3 | `dotnet test` | všech ~650+ testů projde, žádná regrese |
| 4 | CLI `mf list feedback` | Zobrazí otevřené warningy (tabulka) |
| 5 | MCP `get_feedback` | Vrátí strukturovaný JSON |
| 6 | CLI `mf feedback suggest <id>` (bez AI) | "AI není dostupná — zobrazuji pouze systémové hlášení" |
| 7 | CLI `mf feedback suggest <id>` (s AI) | Vypíše 1–3 návrhy s confidence a rationale |
| 8 | Schválený návrh → `mf save` → `mf generate` | Feedback pro daný element zmizí (Resolved) |
| 9 | Learning archive | Pouze anonymizované záznamy, žádná PII |
| 10 | `mf feedback consent off` → změna modelu | Export se neprovede |

## Rizika

| # | Riziko | Mitigace |
|---|--------|----------|
| 1 | **4→5 diagnostických typů** — se zavedením `AuthoringFeedbackRecord` vzniká riziko, že místo unifikace přidáme další fragmentaci | `AuthoringFeedbackRecord` obaluje `Diagnostic` (Core) jako kanonický typ. `DiagnosticInfo` (Generators) rozšířeno o `Code`. `ValidationIssue` migrováno na `Diagnostic` |
| 2 | **Vrstvení** — Translator by neměl injikovat `IAuthoringFeedbackService` | Ambient/event pattern: Translator zapisuje do `DiagnosticBag`, Feedback čte. Žádná injekce Feedback do Translatoru |
| 3 | **`ProjectionView` deprecated** — Fáze 3 by omylem cílila starý typ | Cílit `DocumentProjection` (PROP-056) |
| 4 | **AI repair kontrakt** — paralelní k `ITranslationService` | `IRepairSuggestionService` je nové rozhraní v Translator vrstvě, implementace v AI — jasná separace |
| 5 | **Cache staleness** — po editaci modelu cache ukazuje staré warningy | Invalidace přes `ElementId + Fingerprint`. `Fingerprint` se přepočítá při každé změně elementu |
| 6 | **Learning export leak** — neúmyslný export PII | Anonymizace na úrovni `FeedbackLearningRecord` konstrukce — plaintext názvy nikdy nevstupují do learning recordu |
| 7 | **Nový projekt `MetaForge.Feedback`** — zvýšení složitosti solution | Projekt je tenký — jen modely, `AuthoringFeedbackService`, `DiRegistration`. Infrastruktura zůstává v Infrastructure |
| 8 | **Fáze 2 backward compat** — `CoreValidator` změna z `ValidationIssue` na `Diagnostic` | Postupná migrace: nová metoda `ValidateToDiagnostic()`, stará `Validate()` deprecated s `[Obsolete]` |

## Scope

### Zahrnuto
- Generator diagnostics (CODE-004 — absorbováno)
- Translator enrichment diagnostics
- Core/Pipeline diagnostics
- `AuthoringFeedbackService` v `MetaForge.Feedback`
- `ActiveFeedbackCache` (Infrastructure, JSON, per-project)
- CLI feedback commands (`list`, `dismiss`, `suggest`, `consent`)
- MCP feedback tools (`get_feedback`, `dismiss_feedback`, `suggest_repair`, `set_consent`)
- AI repair suggestions (volitelné, max 3, čtení pouze)
- `FeedbackLearningArchive` (anonymizace, consent, HTTP export)
- `FeedbackOptions` v konfiguraci

### Nezahrnuto
- Auto-apply AI oprav (AI jen navrhuje, nikdy nemění)
- WebApi host surface (není v aktuálním MVP)
- Blazor frontend (PROP-053)
- Auto-apply consent (vždy explicitní uživatelská akce)
- Batch remediation (více oprav najednou)
- `ProjectionView` integrace (deprecated v PROP-056)

## Otevřené otázky

- **OQ-061-01**: Má `CoreValidator` zůstat na `ValidationIssue` a mít paralelní `ValidateToDiagnostic()`, nebo kompletně migrovat? → **Preferováno**: `[Obsolete]` starou metodu, přidat `ValidateDiagnostics()`
- **OQ-061-02**: Má `FeedbackLearningExport` používat retry policy z `21-Telemetry.md` (exponential backoff), nebo vlastní? → **Preferováno**: Použít existující retry policy
- **OQ-061-03**: Má `ActiveFeedbackCache` používat checkpoint model (jako `CheckpointProjectionCache`), nebo flat JSON? → **Preferováno**: Flat JSON per-project, jednodušší pro MVP

## Akceptační kritéria

1. ✅ `dotnet build` — 0 errors, `MetaForge.Feedback` projekt existuje
2. ✅ `dotnet test` — všechny existující testy procházejí, nové testy procházejí
3. ✅ CLI `mf list feedback` zobrazí otevřené warningy po `mf generate`
4. ✅ MCP `get_feedback` vrací strukturovaný JSON
5. ✅ `mf feedback suggest <id>` (bez AI) → "AI není dostupná"
6. ✅ `mf feedback suggest <id>` (s AI) → 1–3 návrhy s confidence
7. ✅ Schválený návrh jde přes `Facade.ApplyPatch()` s `SetCoreDetailOp`
8. ✅ `mf feedback consent on` → learning záznamy se exportují
9. ✅ `mf feedback consent off` → žádný export
10. ✅ Learning export neobsahuje PII (názvy entit, atributů, raw text)
11. ✅ `ActiveFeedbackCache` se invaliduje při změně modelu
12. ✅ `BusinessAuthoringDocument` a `CommandLog` neobsahují feedback data
