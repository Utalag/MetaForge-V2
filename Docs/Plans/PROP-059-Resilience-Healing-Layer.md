# PROP-059: Resilience & Healing Layer

Typ výsledku: Candidate Proposal
Zdroj podnětu: Perplexity konverzace e2801d78 (2026-07-16) — návrhy #5, #6, #7, #8
Stav životního cyklu: Candidate — ⚠️ **ODLOŽENO**: aktivovat až po PROP-058 v produkci, po ověření reálné frekvence chyb
Rozhodovací owner:
Poslední revize: 2026-07-16 (hloubková analýza — odložení)

Priorita: Low (původně Medium) — PROP-043/045 generátory jsou stabilní; frekvence chyb neznámá
Oblast: Infrastructure, Generators, AI
Owner:
Datum vytvoření: 2026-07-16
Aktualizováno: 2026-07-16

Navazuje na:
- **PROP-057** (ElementContract + VerificationModel) — potřebuje `ElementContract` pro určení, co je validní oprava, a `VerificationStateStore` pro evidenci výsledků
- **PROP-058** (Sandbox Preview Runner) — potřebuje sandbox pro spouštění opravných pokusů (compile gate + execution)
- **IDEA-014** (AiSegment Registry Tier2 — Healing segment) — koncept AiSegment.Healing
- **PROP-036** (Core Specification Layer — hotovo) — `InvariantDefinition` definuje hranice "co je správně"

Nahrazuje / rozšiřuje:
- **PROP-050** (Self-Healing Pipeline — ❌ zamítnut 2026-07-11) — PROP-059 je **zásadně odlišný, bezpečnější přístup** ke stejnému uživatelskému problému

## ⚠️ AKTIVAČNÍ PODMÍNKA (2026-07-16)

Tento návrh je **odložen** na základě hloubkové analýzy.

**Důvod**: PROP-043 (Generator Completeness) a PROP-045 (E2E Completeness) jsou stabilní a důkladně otestované (91+ testů). Reálná frekvence selhání generování není známá — může být tak nízká (< 5 %), že healing by řešil neexistující problém.

**Kritérium pro aktivaci**: PROP-058 (Sandbox Preview Runner) je v produkci a data ukazují, že > 10 % metod selhává při generování/kompilaci.

**Pokud frekvence chyb < 5 %**: PROP-059 bude **DROPPED** definitivně (stejně jako PROP-050). Healing nedává smysl, pokud generátor funguje spolehlivě.

**Hlavní riziko**: Uživatelé se naučí spoléhat na healing místo psaní správných modelů. To je anti-vzorec — platforma by měla učit správné modelování, ne maskovat chyby.

Blokuje:
- *(žádné — toto je koncový návrh)*

Související IDEAs:
- **IDEA-014** (AiSegment Registry — Healing + Prehealing segmenty) — 🔴 prerekvizita pro Fázi 3 (AI-assisted repair). Healing segment potřebuje vlastní model, teplotu, prompt šablony.
- **IDEA-016** (Output Readiness Model) — konzumuje výsledky healingu; `OutputReadiness` může být podmíněno tím, že healing proběhl úspěšně (nebo nebyl potřeba).
- **IDEA-018** (Execution Trace Recorder) — 🔴 trasování přes `ILogger`: každý `HealingAttempt` jako strukturovaný event. Ne samostatný recorder.
- **IDEA-020** (NodeAssist OperationValidator) — 🟡 whitelist pattern (`IOperationPolicy`) lze sdílet s `HealingPolicyProvider`.
- **IDEA-029** (ProductionHub) — akceptační testovací sada: `ProductionHub_Healing` scénář.
- **IDEA-031** (Agent Playbook) — po aktivaci PROP-059 přidat do playbooku krok "Heal if broken".

Související soubory:
- `Docs/Plans/Dropped/PROP-050-Self-Healing-Pipeline.md` — **PŮVODNÍ ZAMÍTNUTÝ NÁVRH**
- `Docs/Ideas/old_ideas/IDEA-012-Self-Healing-Pipeline.md` — **PŮVODNÍ ZAMÍTNUTÝ KONCEPT**
- `Src/MetaForge.Core/Elements/MethodElement.cs`
- `Src/MetaForge.Generators/ExpressionRenderer.cs`
- `Src/MetaForge.Infrastructure/`

---

## ⚠️ KLÍČOVÉ: Proč PROP-059 uspěje tam, kde PROP-050 selhal

PROP-050 byl zamítnut 2026-07-11 jako *"příliš experimentální — složité AST manipulace s rizikem sémantických změn, obtížně testovatelné"*.  
Důvod zamítnutí byl **správný**: AST-patching bez znalosti sémantiky je křehký a nebezpečný.

**PROP-059 jde zásadně jinudy:**

| PROP-050 (❌ zamítnut) | PROP-059 (🆕 navržen) |
|------------------------|----------------------|
| AST-patching v **Core** | Healing engine **MIMO Core** (Infrastructure) |
| `RoslynHealingDetector` — technická detekce | Začíná od **ElementContract** (PROP-057) — sémantický kontrakt |
| "Oprav AST, doufej, že to nezmění sémantiku" | **Řízené politiky**: Blocking / RecoverableSilent / RecoverableVisible / NeedsApproval |
| Bez audit trailu | **HealingAttemptLedger** — každý pokus zalogován |
| Cíl: technicky opravit kód | Cíl: **nepustit uživatele k zemi** kvůli internímu detailu |
| Healery v Core/AI vrstvě | Healery v Infrastructure, AI jen jako volitelný suggester |

---

## 1. Kontext

Perplexity konverzace identifikovala klíčovou uživatelskou bolest: **Core je pro uživatele black-box a mohlo by je to locknout.** Když render/generování selže na jedné metodě, uživatel je zablokovaný — nemá jak zjistit co je špatně, nemá jak pokračovat. Čisté "chyba, konec, vrať se zítra" je pro black-box systém nepřijatelné.

Konverzace dále identifikovala, že MetaForge už má skvělé předpoklady pro **graceful recovery**: `DiagnosticBag`, `CoreValidator`, `ElementFingerprint` (PROP-039), `InvariantDefinition` (PROP-036), a nově navržený `ElementContract` (PROP-057). Chybí ale vrstva, která tyto signály využije pro **user-facing resilience**.

Klíčový citát z konverzace:

> *"Nechceš, aby self-healing uměl jen technické opravy render/AST chyb, nebo aby sahal i do business logiky metody? První je dobře ohraničené; druhé už je skoro autonomní code authoring."*

PROP-059 jde cestou **první varianty**: opravujeme jen technické chyby (render, pořadí statementů, chybějící return) — ne business logiku.

---

## 2. Problém dnes

- Když render/generování **selže na jedné metodě**, uživatel je **zablokovaný** — nemůže pokračovat v exportu.
- Core je pro uživatele **black-box** — nemá jak zjistit, co je špatně, jak to opravit.
- Není mechanisms pro **"toto je nekritická chyba, pokračuj s náhradním řešením"**.
- Chybí **audit trail** pro automatické opravy — není jasné co se změnilo, proč, a kdo to schválil.
- PROP-050 byl zamítnut, ale **původní bolest** (uživatel zablokován) zůstala **nevyřešena**.

### Konkrétní scénáře selhání (z Perplexity konverzace)

1. **Špatné pořadí statementů** — deklarace proměnné až po jejím použití → CS0103
2. **Chybějící return** — ne všechny cesty vrací hodnotu → CS0161
3. **Příliš složitý výraz** — ExpressionRenderer vygeneruje nekompilovatelný kód → CS1525
4. **Duplicitní guard clause** — AI vygeneruje stejnou kontrolu dvakrát

---

## 3. Cíl

### User-facing resilience vrstva

- **Nekritická chyba** → automatická oprava, uživatel pokračuje (ideálně si ani nevšimne).
- **Střední chyba** → oprava s upozorněním ("metoda X byla automaticky stabilizována").
- **Kritická chyba** → jasná stopka s diagnostikou, návrh řešení.

### MethodVerificationAndHealingEngine (MIMO Core)

- Vezme `MethodElement` + diagnostics → pokusí se o opravu → znovu ověří.
- **Deterministické repair strategie** jako první volba (bez AI).
- **AI-assisted repair** jako volitelná druhá vrstva (např. Bonsai model).
- **Max N pokusů** (default 3), pak finální diagnostika.
- **Každý pokus zalogován** do `HealingAttemptLedger`.

### Čtyři politiky

| Politika | Chování | Použití |
|----------|---------|---------|
| `Blocking` | STOP — bez recovery | Kritické chyby: narušení source of truth, nekonzistentní model |
| `RecoverableSilent` | Auto-oprava, bez rušení | Drobné syntaktické chyby: chybějící `;`, špatné pořadí `AddVar` |
| `RecoverableVisible` | Auto-oprava + varování | Střední chyby: metoda stabilizována, doplněn return |
| `NeedsApproval` | Návrh opravy, čeká | Sémantické změny: změna signatury, změna typu |

---

## 4. Architektonické invarianty

- **Healing engine je MIMO Core** — je to provozní vrstva (Infrastructure), ne doménový model.
- **BusinessAuthoringDocument zůstává source of truth** — healing ho NEMĚNÍ.
- **Ephemeral repair**: oprava platí jen pro daný export/preview běh (dočasný soubor).
- **Persistent repair**: návrh změny jde standardní cestou přes Facade → PatchEngine → CommandLog (Fáze 4).
- **AI je volitelná** — deterministické repair strategie musí fungovat bez AI.
- **Každý healing pokus je auditovatelný** (`HealingAttemptLedger`).
- **Core zůstává read-only derivace** — healing pracuje s generovanými artefakty, ne s Core elementy.

---

## 5. Scope

### In scope (Fáze 1)

- `IMethodHealingEngine` — hlavní kontrakt
- `HealingPolicy` enum + `HealingPolicyProvider` — mapování diagnostik na politiky
- **3 deterministické repair strategie:**
  - `ReorderStatementsStrategy` — oprava pořadí statementů (CS0103)
  - `AddMissingReturnStrategy` — doplnění chybějícího return (CS0161)
  - `SimplifyExpressionStrategy` — náhrada složitého výrazu jednodušším (CS1525)
- `HealingAttemptLedger` — audit trail (in-memory, později file-based)
- Integrace se **Sandbox Preview Runnerem** (PROP-058) — compile gate + execution
- **Preview / Export režimy** s různou přísností (z PROP-058)

### Out of scope

- AI-assisted repair (Fáze 2 — Bonsai model jako repair suggester)
- Healing pro celé projekty (jen jednotlivé metody)
- Automatické generování patchů do BusinessModelu (Fáze 3 — `SuggestedPatchGenerator`)
- Self-evolution / učení se z historie úspěšných oprav (Fáze 4 — Governed self-evolution)

---

## 6. Návrh řešení

### 6.1 Architektura toku

```
Export/Preview Pipeline
  │
  ├─ 1. Vygeneruj C# z MethodElement (ExpressionRenderer)
  ├─ 2. Zkompiluj (Roslyn in-process)
  │     ├─ OK → pokračuj v pipeline ✅
  │     └─ FAIL → 3. Klasifikuj chyby
  │           │
  │           ├─ 4. Urči HealingPolicy pro každou chybu
  │           │     ├─ Blocking → STOP, vrať diagnostiku ❌
  │           │     ├─ RecoverableSilent → oprav potichu
  │           │     ├─ RecoverableVisible → oprav + varování
  │           │     └─ NeedsApproval → návrh, čekej (future)
  │           │
  │           ├─ 5. Aplikuj repair strategii
  │           ├─ 6. Znovu vygeneruj + zkompiluj
  │           │     ├─ OK → zaloguj, pokračuj ✅
  │           │     └─ FAIL → další pokus (max 3)
  │           │           └─ Po vyčerpání → finální diagnostika ❌
  │           │
  │           └─ 7. Zaloguj každý pokus do HealingAttemptLedger
  │
  └─ Pokračuj v pipeline (s opraveným kódem nebo chybou)
```

### 6.2 Kontrakty

```csharp
// Src/MetaForge.Infrastructure/Healing/IMethodHealingEngine.cs

public interface IMethodHealingEngine
{
    Task<HealingResult> TryHealAsync(
        HealingContext context,
        CancellationToken ct = default);
}
```

```csharp
// Src/MetaForge.Infrastructure/Healing/HealingContext.cs

public sealed record HealingContext
{
    /// <summary>MethodElement, který selhal.</summary>
    public MethodElement Method { get; init; } = null!;

    /// <summary>Volitelný kontrakt metody (PROP-057).</summary>
    public MethodContract? Contract { get; init; }

    /// <summary>Vygenerovaný zdrojový kód, který neprošel kompilací.</summary>
    public string GeneratedSourceCode { get; init; } = string.Empty;

    /// <summary>Diagnostika z Roslyn kompilace.</summary>
    public IReadOnlyList<DiagnosticInfo> Diagnostics { get; init; } = [];

    /// <summary>Preview nebo Export.</summary>
    public SandboxMode Mode { get; init; }

    /// <summary>Číslo aktuálního pokusu (1-based).</summary>
    public int AttemptNumber { get; init; }
}
```

```csharp
// Src/MetaForge.Infrastructure/Healing/HealingResult.cs

public sealed record HealingResult
{
    /// <summary>True pokud se podařilo opravit.</summary>
    public bool Success { get; init; }

    /// <summary>Opravený zdrojový kód (null pokud neúspěch).</summary>
    public string? PatchedSourceCode { get; init; }

    /// <summary>Která strategie byla použita.</summary>
    public string? StrategyUsed { get; init; }

    /// <summary>Popis co se změnilo (pro diagnostiku).</summary>
    public string? Description { get; init; }

    /// <summary>Použitá politika.</summary>
    public HealingPolicy AppliedPolicy { get; init; }

    /// <summary>Zbývající diagnostika po opravě.</summary>
    public IReadOnlyList<DiagnosticInfo> RemainingDiagnostics { get; init; } = [];
}
```

### 6.3 Politiky

```csharp
// Src/MetaForge.Infrastructure/Healing/HealingPolicy.cs

public enum HealingPolicy
{
    /// <summary>STOP — kritická chyba, bez recovery.</summary>
    Blocking,

    /// <summary>Auto-oprava, uživatel není rušen.</summary>
    RecoverableSilent,

    /// <summary>Auto-oprava, uživatel vidí varování.</summary>
    RecoverableVisible,

    /// <summary>Návrh opravy, čeká na potvrzení (Fáze 3+).</summary>
    NeedsApproval
}

public interface IHealingPolicyProvider
{
    HealingPolicy GetPolicy(IReadOnlyList<DiagnosticInfo> diagnostics, SandboxMode mode);
}
```

### 6.4 Deterministické repair strategie

```csharp
// Src/MetaForge.Infrastructure/Healing/Strategies/IRepairStrategy.cs

public interface IRepairStrategy
{
    /// <summary>Lidsky čitelný název strategie.</summary>
    string Name { get; }

    /// <summary>Priorita — nižší číslo = vyšší priorita.</summary>
    int Priority { get; }

    /// <summary>Dokáže tato strategie opravit danou sadu chyb?</summary>
    bool CanHandle(IReadOnlyList<DiagnosticInfo> diagnostics);

    /// <summary>Provede opravu. Vrací null pokud se nepodařilo.</summary>
    Task<RepairAttempt?> TryRepairAsync(HealingContext context, CancellationToken ct);
}

public sealed record RepairAttempt
{
    public string PatchedSourceCode { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
```

**3 vestavěné strategie:**

| # | Strategie | DiagnosticId | Akce | Priorita |
|---|-----------|-------------|------|----------|
| 1 | `ReorderStatementsStrategy` | CS0103, CS0841 | Přesune deklaraci proměnné před její první použití | 10 |
| 2 | `AddMissingReturnStrategy` | CS0161 | Přidá `return default;` na konec metody | 20 |
| 3 | `SimplifyExpressionStrategy` | CS1525, CS1002 | Zjednoduší komplexní výraz na lokální proměnné | 30 |

### 6.5 HealingAttemptLedger

```csharp
// Src/MetaForge.Infrastructure/Healing/IHealingAttemptLedger.cs

public interface IHealingAttemptLedger
{
    Task RecordAsync(HealingAttemptRecord record, CancellationToken ct = default);
    Task<IReadOnlyList<HealingAttemptRecord>> GetHistoryAsync(
        string methodElementId, CancellationToken ct = default);
}

public sealed record HealingAttemptRecord
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string MethodElementId { get; init; } = string.Empty;
    public string InputFingerprint { get; init; } = string.Empty;
    public string? StrategyUsed { get; init; }
    public HealingPolicy Policy { get; init; }
    public bool Success { get; init; }
    public string? OutputFingerprint { get; init; }
    public string? Description { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public string? CorrelationId { get; init; }
}
```

### 6.6 Orchestrátor

```csharp
// Src/MetaForge.Infrastructure/Healing/MethodHealingEngine.cs

public sealed class MethodHealingEngine : IMethodHealingEngine
{
    private readonly IReadOnlyList<IRepairStrategy> _strategies;
    private readonly IHealingPolicyProvider _policyProvider;
    private readonly IHealingAttemptLedger _ledger;
    private const int MaxAttempts = 3;

    public async Task<HealingResult> TryHealAsync(
        HealingContext context, CancellationToken ct = default)
    {
        var currentSource = context.GeneratedSourceCode;
        var currentDiagnostics = context.Diagnostics;

        for (int attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            var policy = _policyProvider.GetPolicy(currentDiagnostics, context.Mode);
            if (policy == HealingPolicy.Blocking)
                return Fail(context, "Kritická chyba, healing není povolen.");

            var strategy = _strategies
                .OrderBy(s => s.Priority)
                .FirstOrDefault(s => s.CanHandle(currentDiagnostics));

            if (strategy == null)
                return Fail(context, "Žádná strategie neumí opravit tuto chybu.");

            var repairContext = context with
            {
                AttemptNumber = attempt,
                GeneratedSourceCode = currentSource,
                Diagnostics = currentDiagnostics
            };

            var repair = await strategy.TryRepairAsync(repairContext, ct);
            if (repair == null)
                continue; // strategie selhala, zkus další

            currentSource = repair.PatchedSourceCode;

            await _ledger.RecordAsync(new HealingAttemptRecord
            {
                MethodElementId = context.Method.Id,
                InputFingerprint = context.Method.Fingerprint?.ToString() ?? "",
                StrategyUsed = strategy.Name,
                Policy = policy,
                Success = true,
                Description = repair.Description,
                Timestamp = DateTimeOffset.UtcNow
            }, ct);

            return new HealingResult
            {
                Success = true,
                PatchedSourceCode = currentSource,
                StrategyUsed = strategy.Name,
                Description = repair.Description,
                AppliedPolicy = policy
            };
        }

        return Fail(context, $"Healing selhal po {MaxAttempts} pokusech.");
    }

    private static HealingResult Fail(HealingContext ctx, string reason) => new()
    {
        Success = false,
        Description = reason,
        AppliedPolicy = HealingPolicy.Blocking,
        RemainingDiagnostics = ctx.Diagnostics
    };
}
```

---

## 7. Implementační dopad

### Nové soubory

| Soubor | Obsah |
|--------|-------|
| `Src/MetaForge.Infrastructure/Healing/IMethodHealingEngine.cs` | Interface |
| `Src/MetaForge.Infrastructure/Healing/MethodHealingEngine.cs` | Implementace + orchestrátor |
| `Src/MetaForge.Infrastructure/Healing/HealingContext.cs` | Context DTO |
| `Src/MetaForge.Infrastructure/Healing/HealingResult.cs` | Result DTO |
| `Src/MetaForge.Infrastructure/Healing/HealingPolicy.cs` | Enum + `IHealingPolicyProvider` |
| `Src/MetaForge.Infrastructure/Healing/IHealingAttemptLedger.cs` | Interface ledgeru |
| `Src/MetaForge.Infrastructure/Healing/HealingAttemptLedger.cs` | In-memory implementace |
| `Src/MetaForge.Infrastructure/Healing/HealingAttemptRecord.cs` | Záznam pokusu |
| `Src/MetaForge.Infrastructure/Healing/Strategies/IRepairStrategy.cs` | Interface strategie |
| `Src/MetaForge.Infrastructure/Healing/Strategies/ReorderStatementsStrategy.cs` | Strategie #1 |
| `Src/MetaForge.Infrastructure/Healing/Strategies/AddMissingReturnStrategy.cs` | Strategie #2 |
| `Src/MetaForge.Infrastructure/Healing/Strategies/SimplifyExpressionStrategy.cs` | Strategie #3 |
| `Tests/MetaForge.Infrastructure.Tests/Healing/` | Testy |

### Testy

| Test | Typ | Ověřuje |
|------|-----|---------|
| `ReorderStatements_MovesDeclarationBeforeUse` | Unit | CS0103 → deklarace přesunuta |
| `AddMissingReturn_AddsDefaultReturn` | Unit | CS0161 → přidán `return default;` |
| `SimplifyExpression_SplitsComplexExpression` | Unit | CS1525 → výraz rozdělen na proměnné |
| `HealingEngine_Success_FirstAttempt` | Integrační | Jeden pokus → OK |
| `HealingEngine_Fail_AfterMaxAttempts` | Integrační | 3 pokusy → fail |
| `HealingEngine_BlockingPolicy_Stops` | Integrační | Blocking → STOP |
| `HealingAttemptLedger_RecordsAllAttempts` | Unit | Všechny pokusy zalogovány |
| `PolicyProvider_Preview_AllowsRecoverable` | Unit | Preview → RecoverableSilent |
| `PolicyProvider_Export_BlockingOnCritical` | Unit | Export → Blocking na kritické |

---

## 8. Implementační fáze

### Fáze 1: Základní healing engine (2 dny)

- `IMethodHealingEngine` + `MethodHealingEngine` (orchestrátor)
- 3 deterministické repair strategie
- `HealingAttemptLedger` (in-memory)
- `HealingPolicyProvider` (základní mapování DiagnosticId → politika)
- Max 3 pokusy, pak diagnostika

### Fáze 2: Politiky a integrace s PROP-058 (1–2 dny)

- Rozlišení Preview vs Export režimu
- Preview: `RecoverableSilent` pro CS0103/CS0161
- Export: `RecoverableVisible` max, `Blocking` pro kritické
- Integrace do Sandbox Preview Runner pipeline

### Fáze 3: AI-assisted repair (2–3 dny, volitelné)

- `AiRepairStrategy` — využívá lokální model (např. Bonsai 500M)
- AI navrhuje opravu → deterministická validace → buď OK nebo fallback
- Fallback: když AI není dostupná → deterministické strategie

### Fáze 4: SuggestedPatchGenerator (budoucí)

- Z úspěšného healingu vygenerovat návrh patche
- Patch jde standardní cestou přes Facade → PatchEngine → CommandLog
- Uživatel schválí/odmítne

---

## 9. Otevřené otázky

- **OQ-059-01**: Jak poznat "kritickou" vs "nekritickou" chybu?
  - *Doporučení: Klasifikace podle DiagnosticId. CS0161 (missing return) = kritická v Export módu. CS0103 (name not in scope) = Recoverable. Mapování v `HealingPolicyProvider`.*
- **OQ-059-02**: Má healing engine měnit source of truth, nebo jen generovaný kód?
  - *Doporučení: Fáze 1–3 jen generovaný kód (ephemeral). Fáze 4: návrh patche do BusinessModelu.*
- **OQ-059-03**: Jak velký model pro AI-assisted repair a jak ho integrovat?
  - *Doporučení: Bonsai 500M (deepgrove/Bonsai). Lokálně přes OllamaAdapter. Není instruction-tuned → fine-tuning pro downstream použití.*

---

## 10. Rizika a trade-offy

- **Riziko falešné opravy**: Healing opraví chybu, ale změní sémantiku.
  - *Mitigace: Fáze 1–2 jen deterministické strategie (bez AI). Každá oprava ověřena přes kompilaci. AI až ve Fázi 3 s fallbackem.*
- **Riziko závislosti na healingu**: Uživatel se spolehne na healing a přestane kontrolovat výstup.
  - *Mitigace: `RecoverableVisible` vždy ukáže varování. `HealingAttemptLedger` je vždy k dispozici pro audit.*
- **Riziko pomalého healingu**: Opakovaná kompilace může být pomalá.
  - *Mitigace: Max 3 pokusy. Cachování fingerprintů.*
- **Vědomý kompromis**: Fáze 1 neřeší persistentní opravu do source of truth — jen ephemeral pro export/preview. Trvalá oprava až ve Fázi 4.
- **Nekoliduje s PROP-050**: PROP-050 byl zamítnut pro odlišný přístup (AST-patching v Core). PROP-059 je záměrně navržen tak, aby se vyhnul stejným pastem.

---

## 11. Validace

- **Build**: `dotnet build` projde (Infrastructure projekt)
- **Testy**: 9 testovacích scénářů (viz sekce Testy)
- **Smoke scénář**: Metoda s chybějícím return → `AddMissingReturnStrategy` → metoda projde kompilací → `HealingAttemptLedger` obsahuje záznam
- **Ruční kontrola**: `HealingAttemptLedger` musí být čitelný. Výstup `RecoverableVisible` musí obsahovat srozumitelné varování.

---

## 12. Výsledek po dokončení

*(Vyplní se při uzavření návrhu.)*
