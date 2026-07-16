# PROP-050 Self-Healing Pipeline — Detekce a oprava AI-generovaného kódu

Typ výsledku: Candidate Proposal → ❌ Dropped
Zdroj podnětu: IDEA-012 (For_Inspiration/Architecture-Define/08-Methods-and-SelfHealing.md)
Stav životního cyklu: Dropped — odloženo na neurčito jako příliš experimentální (2026-07-11)
Rozhodovací owner:
Poslední revize: 2026-07-11

Priorita: High
Oblast: Core, AI, Generators
Owner:
Datum vytvoření: 2026-07-11
Aktualizováno: 2026-07-11

Navazuje na:
- PROP-031 (Core Statement System — hotovo)
- PROP-035 (C#-First Core Migration — hotovo)
- PROP-043 (Generator Completeness — hotovo)
- PROP-036 (Core Specification Layer — hotovo; invarianty definují validitu)

Blokuje:
- —

Související soubory:
- `Src/MetaForge.Core/Elements/Statements/`
- `Src/MetaForge.Generators/ExpressionRenderer.cs`
- `Src/MetaForge.Generators/CodeGenerator.cs`
- `For_Inspiration/Architecture-Define/08-Methods-and-SelfHealing.md`

## 1. Kontext

Původní projekt MetaForge obsahoval propracovaný koncept Self-Healing — opravné smyčky pro AI-generovaný kód. Při analýze gapů mezi původním konceptem a současnou implementací byl Self-Healing identifikován jako největší chybějící koncept — není pokryt žádným existujícím PROPem.

Statement AST (PROP-031) a C#-first Core (PROP-035) jsou nyní stabilní a dokončené — vytvářejí základ, na kterém lze postavit healing pipeline. AI generátor (PROP-027, PROP-043) umí produkovat AST, ale chybí smyčka "vygeneruj → zvaliduj → oprav".

## 2. Problém dnes

- `MethodElement.Body` je nyní Statement AST, ale chybí nástroje pro opravu chybně vygenerovaného AST.
- AI generátor může produkovat strukturálně vadný kód — chybí smyčka "vygeneruj → zvaliduj → oprav".
- Roslyn kompilace selže, ale uživatel neví, co opravit.
- Fail-fast bez self-healing = špatná UX pro AI-assisted code generation.
- Současné PROPy (031, 035, 043) řeší AST a C#-first model, ale neřeší "co když AI vygeneruje blbost".

Konkrétní známé AI chyby:
- Špatné pořadí `AddVar` (deklarace proměnné až po jejím použití)
- Chybějící return větve (CS0161 — ne všechny cesty vrací hodnotu)
- Halucinace konstruktorů (volání neexistujícího konstruktoru)
- Duplicitní guard clauses
- Chybějící středníky nebo závorky v renderovaném textu

## 3. Cíl

- AI generátor produkuje AST → validation → pokud chyba, healing oprava → opakovaná validace.
- Uživatel dostane validní kód i z neperfektního AI generování.
- Max N iterací (default 3), poté fallback na deterministický stub.
- Healing je volitelný (opt-in) — nezdržuje E2E bez AI.

## 4. Architektonické invarianty

- AI je volitelná vrstva, ne podmínka základní funkčnosti — healing pipeline musí být komponovatelná i bez AI.
- Core nesmí nést logiku, která patří do vyšší vrstvy — healing orchestrace patří do AI vrstvy, AST opravy do Core.
- Healing nesmí změnit sémantiku kódu — pouze opravit strukturální chyby.

## 5. Scope

### In scope
- **Detection stage**: Roslyn kompilace AST → seznam chyb (CS0161, CS0103, CS0117, CS1002, CS1525)
- **Healing stage**: mapování známých chyb na strukturované opravy AST
- **Validation stage**: opakovaná kompilace po healingu — max N iterací
- **Fallback**: pokud healing selže → deterministický fallback (throw `NotImplementedException` nebo generický stub)
- **Prehealing** (volitelné): validace AST před exportem

### Out of scope
- Healing sémantických chyb (špatná business logika)
- Healing pro negenerovaný kód (uživatelský kód)
- Plná integrace do MCP/CLI (pouze pipeline vrstva)
- Self-healing pro jiné jazyky než C#

## 6. Návrh řešení

### Architektura

```
AI Generator → AST → [Prehealing] → Export → C# text → [Healing Pipeline]
                                                              ↓
                                              Roslyn Compilation
                                                              ↓
                                              Error Detection → map → AST Patch
                                                              ↓
                                              Re-export → Re-compile (max N×)
                                                              ↓
                                              Fallback if failed
```

### Detection stage

`IHealingDetector` interface v Core:

```csharp
public interface IHealingDetector
{
    IReadOnlyList<HealingIssue> Detect(string sourceCode);
}

public record HealingIssue(
    string DiagnosticId,    // "CS0161", "CS0103"
    string Message,
    int Line,
    int Column,
    HealingSeverity Severity
);
```

První implementace: `RoslynHealingDetector` — kompiluje C# kód, parsuje diagnostiky.

### Healing stage

`IHealingProvider` interface v Core:

```csharp
public interface IHealingProvider
{
    bool CanHandle(string diagnosticId);
    HealingResult? Heal(HealingIssue issue, MethodElement method);
}

public record HealingResult(
    MethodElement PatchedMethod,
    string Description
);
```

Vestavěné healery (v MetaForge.Ai nebo MetaForge.Generators):

| Diagnostika | Healer | Akce |
|------------|--------|------|
| CS0161 (not all paths return) | `MissingReturnHealer` | Přidá `return default;` na konec metody |
| CS0103 (name not in scope) | `ReorderDeclarationHealer` | Přesune deklaraci před použití |
| CS1002 (missing semicolon) | `MissingSemicolonHealer` | Přidá `;` na konec statementu |
| CS0117 (does not contain definition) | `MemberAccessHealer` | Opraví member access path |
| CS1525 (invalid expression) | `ExpressionFallbackHealer` | Nahradí výraz `default` placeholderem |

### Pipeline orchestrátor

`HealingPipeline` (v MetaForge.Ai nebo MetaForge.Generators):

```csharp
public sealed class HealingPipeline
{
    private readonly IHealingDetector _detector;
    private readonly IReadOnlyList<IHealingProvider> _healers;
    private const int MaxIterations = 3;

    public CodeGenerationResult Heal(CodeGenerationResult initial)
    {
        var current = initial;
        for (int i = 0; i < MaxIterations; i++)
        {
            var issues = _detector.Detect(current.SourceCode);
            if (!issues.Any()) return current;  // ✅ clean

            foreach (var issue in issues)
            {
                var healer = _healers.FirstOrDefault(h => h.CanHandle(issue.DiagnosticId));
                if (healer == null) continue;

                var result = healer.Heal(issue, /* method */);
                if (result != null) current = ApplyPatch(current, result);
            }
        }
        return Fallback(current);  // ⚠️ max iterations reached
    }
}
```

### Fallback

- Pokud healing po N iteracích stále selže → nahradit tělo metody `throw new NotImplementedException();`
- Fallback je deterministický — nikdy negeneruje nový AI prompt

### Rozdělení odpovědností

- **Core**: `IHealingDetector`, `IHealingProvider`, `HealingIssue`, `HealingResult` — interface a value objekty
- **Generators**: `RoslynHealingDetector` — Roslyn-based detekce (mimo Core, závislost na Roslyn)
- **AI nebo Generators**: jednotlivé healery
- **AI**: `HealingPipeline` orchestrátor
## 9. Nástupnický návrh (2026-07-16)

Tento návrh byl **zamítnut** 2026-07-11 jako příliš experimentální.  
Stejnou uživatelskou bolest (nezablokovat uživatele kvůli interní chybě) řeší nová sada návrhů — **zásadně odlišným, bezpečnějším přístupem**:

- **PROP-057: ElementContract + VerificationModel** — sémantické kontrakty jako základ
- **PROP-058: Sandbox Preview Runner** — izolované spouštění metod
- **PROP-059: Resilience & Healing Layer** — user-facing resilience s audit trailem

### Klíčové rozdíly oproti PROP-050

| PROP-050 (❌ zamítnut) | PROP-059 (🆕 navržen) |
|------------------------|----------------------|
| AST-patching v **Core** | Healing engine **MIMO Core** (Infrastructure) |
| `RoslynHealingDetector` — technická detekce | Začíná od **ElementContract** — sémantický kontrakt |
| "Oprav AST, doufej" | **Řízené politiky**: Blocking / RecoverableSilent / RecoverableVisible / NeedsApproval |
| Bez audit trailu | **HealingAttemptLedger** — každý pokus zalogován |
| Cíl: technicky opravit kód | Cíl: **nepustit uživatele k zemi** kvůli internímu detailu |
| Healery v Core/AI vrstvě | Healery v Infrastructure, AI jen jako volitelný suggester |

Zdroj: Perplexity konverzace e2801d78 (2026-07-16)
## 7. Implementační dopad

### Změněné projekty nebo soubory
- `Src/MetaForge.Core/Healing/IHealingDetector.cs` — nový
- `Src/MetaForge.Core/Healing/IHealingProvider.cs` — nový
- `Src/MetaForge.Core/Healing/HealingIssue.cs` — nový
- `Src/MetaForge.Core/Healing/HealingResult.cs` — nový
- `Src/MetaForge.Generators/Healing/RoslynHealingDetector.cs` — nový
- `Src/MetaForge.Generators/Healing/MissingReturnHealer.cs` — nový
- `Src/MetaForge.Generators/Healing/ReorderDeclarationHealer.cs` — nový
- `Src/MetaForge.Ai/HealingPipeline.cs` — nový (orchestrátor)
- `Tests/MetaForge.Generators.Tests/Healing/` — testy

### API a kontrakty
- Nová veřejná rozhraní `IHealingDetector` a `IHealingProvider` v Core.

### Testy
- Unit testy pro každý healer (2-3 testy na healer)
- Integration test: AI vygeneruje chybný AST → healing opraví → validní C#

### Dokumentace
- Aktualizace `New_Architecture/09-AI-Layer.md` — Self-Healing sekce

## 8. Implementační fáze

### Fáze 1: Rozhraní a detekce (~1 den)
- `IHealingDetector`, `IHealingProvider` v Core
- `RoslynHealingDetector` v Generators
- Detekce CS0161, CS0103, CS1002

### Fáze 2: Základní healery (~1.5 dne)
- `MissingReturnHealer`
- `ReorderDeclarationHealer`
- `MissingSemicolonHealer`
- Unit testy

### Fáze 3: Pipeline orchestrátor (~1 den)
- `HealingPipeline` v MetaForge.Ai
- Fallback strategie
- Max N iterací

### Fáze 4: Integrace a testy (~1 den)
- Napojení na AI generátor
- E2E test: chybný AST → validní C#
- Dokumentace

## 9. Otevřené otázky

- **OQ-050-01**: Kam patří healing rozhraní? Do Core (neutrální, bez závislostí), nebo do samostatného projektu `MetaForge.Healing`? Navrhuji Core — rozhraní jsou malá a stabilní.
- **OQ-050-02**: Má healing běžet automaticky (součást `GenerateCodeAsync`) nebo explicitně (tool `heal code`)? Navrhuji volitelně automaticky s možností vypnutí.
- **OQ-050-03**: Jak předat `MethodElement` healeru, když healing pracuje nad textem (SourceCode), ne nad AST? Vyžaduje zpětné mapování (SourceMap z IDEA-009) — nebo healovat až nad AST před exportem?

## 10. Rizika a trade-offy

- **Riziko sémantické změny**: Healing může opravit chybu způsobem, který změní chování kódu. Mitigace: healery mění pouze strukturální chyby (chybějící return, pořadí deklarací), nikdy business logiku.
- **Riziko nekonečné smyčky**: Max N iterací (3) + fallback.
- **Riziko false positives**: Ne každá CS0161 je chyba AI — někdy uživatel skutečně zapomněl return. Mitigace: healing je volitelný, uživatel ho může vypnout.
- **Vědomý kompromis**: První verze pokrývá jen 3-5 nejčastějších diagnostik — další se přidávají postupně.

## 11. Validace

- Build: `dotnet build` bez chyb
- Testy: unit testy pro každý healer + E2E healing test
- Smoke: AI vygeneruje metodu bez return → healing přidá `return default;` → kompilace projde
- Jak poznáme, že je návrh hotový: Pipeline zpracuje známé AI chyby (CS0161, CS0103, CS1002) a vrátí validní C#

## 12. Výsledek po dokončení

*— vyplnit při uzavření —*
