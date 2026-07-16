# PROP-058: Sandbox Preview Runner

Typ výsledku: Candidate Proposal
Zdroj podnětu: Perplexity konverzace e2801d78 (2026-07-16) — návrhy #3, #4
Stav životního cyklu: Candidate
Rozhodovací owner:
Poslední revize: 2026-07-16

Priorita: High
Oblast: Generators, Host Surfaces, Infrastructure
Owner:
Datum vytvoření: 2026-07-16
Aktualizováno: 2026-07-16

Navazuje na:
- **PROP-057** (ElementContract + VerificationModel) — potřebuje `ElementContract` pro definici scénářů a `VerificationStateStore` pro skipování
- **PROP-030** (SandboxGuard — hotovo) — PROP-058 **rozšiřuje** koncept sandboxu: PROP-030 = TIER0 limity, PROP-058 = method execution
- **PROP-043** (Generator Completeness — hotovo) — generátor umí emitovat C# z MethodElement
- **PROP-048** (Generator Render Core Tests — hotovo) — `ExpressionRenderer` je otestovaný

Blokuje:
- **PROP-059** (Resilience & Healing Layer) — healing engine potřebuje sandbox pro spouštění opravných pokusů

Související soubory:
- `Src/MetaForge.Generators/ExpressionRenderer.cs`
- `Src/MetaForge.Generators/CodeGenerator.cs`
- `Src/MetaForge.Cli/`
- `Src/MetaForge.Infrastructure/Sandbox/`

---

## 1. Kontext

MetaForge dnes umí generovat C# kód (PROP-043), validovat syntaxi přes Roslyn (PROP-032, PROP-048), a má sandbox limity pro TIER 0 (PROP-030). Ale **neumí spustit** vygenerovanou metodu s testovacími vstupy a vrátit výsledek. Uživatel, který modeluje doménu, nemá jak si rychle ověřit, že metoda dělá to, co má — musí čekat na plný export, ručně napsat test, nebo spustit celou aplikaci.

Perplexity konverzace identifikovala potřebu **"Sandbox Preview Runneru"** — dočasné konzolové aplikace, která vezme vygenerovaný kód metody, obalí ho harness kódem, zkompiluje a spustí s poskytnutými JSON vstupy.

### Dva režimy — klíčový koncept z konverzace

> *Preview je pískoviště, Export je podpis pod výsledkem.*

| Aspekt | Preview režim | Export režim |
|--------|--------------|-------------|
| Účel | Rychlé ověření, experimentování | Produkční artifact |
| Tolerance chyb | Vysoká — dovolí stuby, degradaci | Nízká — fail-fast |
| Healing | Povolen (`RecoverableSilent`) | Omezený (`RecoverableVisible` max) |
| Zápis do source of truth | NE | NE (sandbox je vždy ephemeral) |
| CLI command | `metaforge preview run-method` | `metaforge export --verify` |

---

## 2. Problém dnes

- Není způsob, jak **rychle** vyzkoušet jednu metodu bez plného exportu.
- Uživatel nevidí, jestli jeho business logika funguje, dokud nespustí celou aplikaci.
- Chybí **compile gate** pro jednotlivé metody — dnes jen pro celé projekty.
- Není mechanismus pro **Preview vs Export** režim s různou přísností.
- PROP-030 SandboxGuard řeší jen **limity** (počet entit), ne execution.

---

## 3. Cíl

### Sandbox Preview Runner

- Vezme `MethodElement` → vygeneruje C# → obalí harness kódem → vytvoří dočasný projekt → zkompiluje → spustí s JSON vstupy → vrátí výsledek.
- Dva režimy: **Preview** (tolerantní, povolí stuby) a **Export** (přísný, fail-fast).
- CLI command: `metaforge preview run-method --method MethodName --inputs '{"param": value}'`.
- **Vše ephemeral** — nic se nezapisuje do source of truth.

### Compile gate

- Před spuštěním ověří, že generovaný kód jde zkompilovat.
- Oddělí "kód se nedá sestavit" od "metoda se nechová očekávaně".

### Integrace s PROP-057

- Využívá `MethodContract.ScenarioHints` pro automatické generování testovacích vstupů.
- Využívá `VerificationStateStore` pro skipování již ověřených metod.

---

## 4. Architektonické invarianty

- **Sandbox runner je MIMO Core** — je to provozní vrstva (Infrastructure), ne doménový model.
- **Nic se nemutuje** v BusinessAuthoringDocument — sandbox pracuje s ephemeral artefakty.
- **AI je volitelná** — sandbox musí fungovat i bez AI. AI-assisted vstupy jsou optional.
- **Host surface (CLI) zůstává tenká** — pouze předává příkazy Facade → SandboxExecutionService.
- **Preview režim smí degradovat výstup** (stub místo problémové metody), Export ne.

---

## 5. Scope

### In scope (Fáze 1)

- `ISandboxExecutionService` — hlavní kontrakt
- `SandboxExecutionRequest` / `SandboxExecutionResult` — datové kontrakty
- Vytvoření dočasného `.csproj` + `Program.cs` s harness kódem
- Kompilace **in-process** přes Roslyn (`CSharpCompilation`)
- Spuštění metody s JSON vstupy (deserializace parametrů, serializace výsledku)
- CLI command: `metaforge preview run-method`
- **Compile gate**: syntax + kompilace před spuštěním
- **Timeout**: výchozí 10s, konfigurovatelný


### Out of scope (Fáze 1)
- Metody s externími závislostmi (DbContext, HttpClient, IMediator, ...) → Fáze 2
- Stubování závislostí → Fáze 2
- Docker kontejnerová izolace → Fáze 2

### Out of scope (celkově)
- Sandbox pro celé projekty (jen jednotlivé metody)
- Interaktivní REPL (→ budoucí PROP, již naznačeno v PROP-026)
- AI-assisted healing v sandboxu (→ PROP-059, odloženo)

### Související IDEAs
- **IDEA-009** (SourceMap) — mapování chyb kompilace → Core elementy; kandidát na Fázi 2
- **IDEA-010** (EmitPhase hooks) — sandbox pipeline fáze (generate → compile → execute)
- **IDEA-016** (Output Readiness) — konzumuje sandbox výsledky; Fáze 4
- **IDEA-018** (Execution Trace) — trasování sandbox execution přes `ILogger`, ne samostatný recorder
- **IDEA-029** (ProductionHub) — akceptační test: `ProductionHub_Sandbox` (jen pro čisté metody)


### 6.1 Architektura toku

```
CLI: metaforge preview run-method --method CalculatePrice --inputs '{"quantity":5}'
  │
  ▼
BusinessAuthoringHostFacade (tenká vrstva)
  │
  ▼
ISandboxExecutionService
  │
  ├─ 1. Získej MethodElement z Core (přes Translator/Projection)
  ├─ 2. Získej MethodContract (volitelný, z PROP-057)
  ├─ 3. Vygeneruj C# kód metody (ExpressionRenderer)
  ├─ 4. Vygeneruj harness kód:
  │     - Třída SandboxHost s Main()
  │     - Deserializace parametrů z JSON
  │     - Volání cílové metody
  │     - Serializace výsledku / zachycení výjimky
  ├─ 5. Vytvoř dočasný .csproj (net9.0, console, minimal references)
  ├─ 6. Zkompiluj in-process (Roslyn CSharpCompilation)
  │     ├─ OK → pokračuj
  │     └─ FAIL → vrať CompilationFailedDiagnostics
  ├─ 7. Spusť sestavené assembly s JSON vstupy (Process.Start s timeoutem)
  │     ├─ OK → vrať SandboxExecutionResult s OutputJson
  │     ├─ Exception → vrať ExecutionFailedDiagnostics
  │     └─ Timeout → vrať TimeoutDiagnostics
  └─ 8. Ukliď dočasné soubory
```

### 6.2 Kontrakty

```csharp
// Src/MetaForge.Infrastructure/Sandbox/ISandboxExecutionService.cs

public interface ISandboxExecutionService
{
    Task<SandboxExecutionResult> ExecuteAsync(
        SandboxExecutionRequest request,
        CancellationToken ct = default);
}
```

```csharp
// Src/MetaForge.Infrastructure/Sandbox/SandboxExecutionRequest.cs

public sealed record SandboxExecutionRequest
{
    /// <summary>ID MethodElementu ke spuštění.</summary>
    public string MethodElementId { get; init; } = string.Empty;

    /// <summary>MethodElement z Core.</summary>
    public MethodElement Method { get; init; } = null!;

    /// <summary>Volitelný kontrakt metody (PROP-057).</summary>
    public MethodContract? Contract { get; init; }

    /// <summary>JSON vstupní parametry.</summary>
    public string InputJson { get; init; } = "{}";

    /// <summary>Režim: Preview (tolerantní) nebo Export (přísný).</summary>
    public SandboxMode Mode { get; init; } = SandboxMode.Preview;

    /// <summary>Timeout pro spuštění.</summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(10);
}

public enum SandboxMode { Preview, Export }
```

```csharp
// Src/MetaForge.Infrastructure/Sandbox/SandboxExecutionResult.cs

public sealed record SandboxExecutionResult
{
    /// <summary>True pokud metoda doběhla bez výjimky.</summary>
    public bool Success { get; init; }

    /// <summary>Serializovaná návratová hodnota (JSON).</summary>
    public string? OutputJson { get; init; }

    /// <summary>Chybová zpráva (pokud výjimka).</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Standardní výstup (Console.WriteLine).</summary>
    public string? StdOut { get; init; }

    /// <summary>Diagnostika z kompilace.</summary>
    public IReadOnlyList<DiagnosticInfo> CompilationDiagnostics { get; init; } = [];

    /// <summary>Celkový čas vykonávání.</summary>
    public TimeSpan ExecutionTime { get; init; }

    /// <summary>Použitý režim.</summary>
    public SandboxMode Mode { get; init; }
}
```

### 6.3 Harness kód (generovaný)

```csharp
// Vygenerovaný dočasný Program.cs — Sandbox Harness
// ⚠️ AUTO-GENERATED — DO NOT EDIT

using System;
using System.Text.Json;

try
{
    // === DESERIALIZUJ VSTUPY ===
    var inputJson = @"{""quantity"":5}";  // doplněno z InputJson
    // ... deserializace podle signatury metody ...

    // === VOLEJ CÍLOVOU METODU ===
    var result = MyNamespace.OrderService.CalculatePrice(quantity: 5);

    // === SERIALIZUJ VÝSTUP ===
    var outputJson = JsonSerializer.Serialize(result);
    Console.WriteLine("__SANDBOX_RESULT__:" + outputJson);
    return 0;
}
catch (Exception ex)
{
    Console.WriteLine("__SANDBOX_ERROR__:" + ex.Message);
    Console.WriteLine("__SANDBOX_STACKTRACE__:" + ex.StackTrace);
    return 1;
}
```

### 6.4 CLI integrace

```bash
# Spuštění metody s JSON vstupy (Preview režim)
metaforge preview run-method --method CalculatePrice --inputs '{"quantity":5,"discount":0.1}'

# Spuštění s timeoutem a vynuceným Export režimem
metaforge preview run-method --method CalculatePrice --inputs '{"quantity":5}' --mode export --timeout 30

# Spuštění s využitím scénářů z MethodContract (PROP-057)
metaforge preview run-method --method CalculatePrice --scenario HappyPath
```

---

## 7. Implementační dopad

### Nové soubory

| Soubor | Obsah |
|--------|-------|
| `Src/MetaForge.Infrastructure/Sandbox/ISandboxExecutionService.cs` | Interface |
| `Src/MetaForge.Infrastructure/Sandbox/SandboxExecutionService.cs` | Implementace |
| `Src/MetaForge.Infrastructure/Sandbox/SandboxExecutionRequest.cs` | Request DTO |
| `Src/MetaForge.Infrastructure/Sandbox/SandboxExecutionResult.cs` | Result DTO |
| `Src/MetaForge.Infrastructure/Sandbox/SandboxHarnessGenerator.cs` | Generátor harness kódu |
| `Src/MetaForge.Cli/Commands/PreviewCommand.cs` | CLI command |

### Změněné soubory

| Soubor | Změna |
|--------|-------|
| `Src/MetaForge.Cli/Program.cs` | Registrace `PreviewCommand` |
| `Src/MetaForge.Infrastructure/ServiceRegistration.cs` | DI registrace `ISandboxExecutionService` |

### Testy

| Test | Typ | Ověřuje |
|------|-----|---------|
| `Sandbox_Execute_SimpleMethod_ReturnsOutput` | Integrační | Metoda vracející hodnotu → správný JSON |
| `Sandbox_Execute_ThrowingMethod_ReturnsError` | Integrační | Metoda házející výjimku → ErrorMessage |
| `Sandbox_Execute_InvalidCode_CompilationFailed` | Integrační | Nevalidní C# → diagnostika |
| `Sandbox_Execute_Timeout_ReturnsTimeout` | Integrační | Dlouhá metoda → timeout |
| `Sandbox_Mode_Preview_AllowsDegradation` | Unit | Preview režim dovolí stub |
| `Sandbox_Mode_Export_FailFast` | Unit | Export režim stopne při chybě |
| `Sandbox_Harness_GeneratesCompilableCode` | Unit | Vygenerovaný harness je validní C# |

---

## 8. Implementační fáze

### Fáze 1: Základní sandbox execution (2 dny)

- `ISandboxExecutionService` + implementace
- `SandboxHarnessGenerator` — generování harness kódu
- In-process Roslyn kompilace (`CSharpCompilation`)
- Spuštění přes `Process.Start` s timeoutem
- JSON serializace/deserializace parametrů a výsledků
- Úklid dočasných souborů

### Fáze 2: CLI integrace (1 den)

- `metaforge preview run-method` command
- Výstup do konzole (formátovaný JSON)
- Podpora `--mode preview|export`
- Podpora `--scenario` (využití scénářů z PROP-057)

### Fáze 3: Compile gate + politiky (1 den)

- Rozlišení Preview/Export režimu
- Preview: povolí stub metody
- Export: fail-fast při jakékoliv chybě kompilace
- Integrace s `VerificationStateStore` (PROP-057) — skipování

---

## 9. Otevřené otázky

- **OQ-058-01**: Kompilovat in-process nebo v externím procesu?
  - *Doporučení: in-process pro Fázi 1 (rychlost, jednoduchost). Docker izolace pro Fázi 2 (bezpečnost).*
- **OQ-058-02**: Jak řešit závislosti metody na jiných typech?
  - *Doporučení: generovat minimální stub typy pro závislosti. Např. pokud metoda používá `Auto` třídu, vygenerovat `class Auto { public string Spz {get;set;} }` s vlastnostmi použitými v metodě.*
- **OQ-058-03**: Má sandbox cachovat zkompilovaná assembly?
  - *Doporučení: ano, s fingerprint-based invalidací z PROP-057. Pokud se MethodElement nezměnil, přeskočit kompilaci.*

---

## 10. Rizika a trade-offy

- **Bezpečnostní riziko**: Sandbox runner spustí uživatelský/vygenerovaný kód.
  - *Mitigace Fáze 1: timeout + Process.Start bez admin práv. Fáze 2: Docker kontejner.*
- **Riziko pomalé kompilace**: In-process Roslyn kompilace může být pomalá při velkých modelech.
  - *Mitigace: cachovat assembly s fingerprint-based invalidací.*
- **Vědomý kompromis**: Jen jednotlivé metody, ne celé projekty (Fáze 1). Sandbox pro celé projekty je výrazně složitější (závislosti, NuGet).
- **Nekoliduje s PROP-030**: PROP-030 řeší limity (max entit v TIER0), PROP-058 řeší execution. Doplňují se.

---

## 11. Validace

- **Build**: `dotnet build` projde
- **Testy**: 7 scénářů (viz sekce Testy)
- **Smoke scénář**: `metaforge preview run-method --method CalculatePrice --inputs '{"quantity":5}'` → validní JSON výstup
- **Ruční kontrola**: Výstup musí být čitelný pro vývojáře. Chyby musí obsahovat diagnostiku.

---

## 12. Výsledek po dokončení

*(Vyplní se při uzavření návrhu.)*
