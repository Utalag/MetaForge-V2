# PROPOSALS_NEXT — Zásobník kandidátních návrhů

> Návrhy, které jsou identifikované, ale zatím neschválené k implementaci.
> Nikdy neimplementovat přímo z tohoto souboru — vždy přesunout do PROPOSALS.md.
> Plány jsou uloženy v `Docs/Plans/PROP-XXX-*.md` a jsou součástí návrhu.

## Kandidátní návrhy

| ID | Název | Vrstva | Priorita | Odhad | Poznámka |
|----|-------|--------|----------|-------|----------|
| **PROP-047** | Translator — Strong Type Mapping (čte `CoreDetail.IsStrongType`, vytváří `ValueObjectElement` + `PropertyElement`, translation source anotace, fallback na primitiva) | Translator | 🔴 Vysoká | 1-2 dny | Navazuje na hotovou infrastrukturu Core+Generator+Vogen architekturu. Pipeline: AI→BusinessModel→Translator→Core→Generator. |
| **PROP-045** | Generator E2E Completeness — 7 nových scénářů (Scénáře 7-12) pokrývajících async/await, foreach/while, try-catch, event/operator/delegate, lambda, struct | Generators, Tests | 🟡 Vysoká | 2-3 dny | Follow-up k PROP-043. 5/12 scénářů hotovo. |
| **PROP-044** | Translator & BusinessModel — Workflow opravy, SyncState konsolidace, Facade thread safety | Translator, BusinessModel | 🟡 Vysoká | 2-3 dny | Fixes known from PROP-020 implementation. |
| **PROP-018** | Translator — ExpertProjection a ProjectionOptions | Translator | 🟡 Střední | 2-3 dny | Potřebuje PROP-044 jako základ. |
| **PROP-046** | AI Model Benchmarking — Referenční vs lokální modely, strukturální komparátor, prompt varianty, matice pass/fail | AI, Tests | 🟡 Střední | 4 dny | Cíl: najít nejslabší lokální model srovnatelný s GPT-4o/Claude. |
| **PROP-017** | Generators — ForgeBlock Packaging (BlueprintBuilder, catalog entries) | Generators, ForgeBlocks | 🟢 Nízká | — | Až budou existovat první ForgeBlock balíky. |
| **PROP-023** | DX a architektonická vylepšení (na zvážení) — Typový SyncState, Layer stack, YAML DSL, Undo/redo | Průřezové | ⚪ Na zvážení | 5-9 dní | Neimplementovat bez schválení. |

## Odložené návrhy

| ID | Název | Důvod odložení | Datum |
|----|-------|-----------------|-------|
| PROP-018 | Translator — ExpertProjection a ProjectionOptions | nejsem si jist správnou funkcionalitou pro host surfaces | 4.7.2026 |
| PROP-020-F5 | BusinessModel — Fáze 5: BusinessBehaviorInputNode, PendingQuestion rozšíření | nízká priorita, neblokuje core flow; PROP-020 Fáze 1–4 dokončeny | 4.7.2026 |

## Odložené návrhy

| ID | Název | Důvod odložení | Datum |
|----|-------|----------------|-------|
| PROP-018 | Translator — ExpertProjection a ProjectionOptions | nejsem si jist správnou funkcionalitou pro host surfaces | 4.7.2026 |
| PROP-020-F5 | BusinessModel — Fáze 5: BusinessBehaviorInputNode, PendingQuestion rozšíření | nízká priorita, neblokuje core flow; PROP-020 Fáze 1–4 dokončeny | 4.7.2026 |

## Issues — Známé problémy k opravě

> Problémy zjištěné při Code Review po implementaci. Každý issue má vlastní detailní soubor v `Docs/Issues/ISS-xxx_nazev.md`.
> Při opravě přesunout do `PROPOSALS.md` jako nový PROP nebo task.

| # | Datum | PROP | Soubor | Závažnost | Popis | Doporučené řešení | Issue soubor |
|---|-------|------|--------|-----------|-------|-------------------|--------------|
| 1 | 4.7.2026 | PROP-028 | `InfrastructureServiceRegistration.cs` | ✅ Vyřešeno | `AddSingleton` → `TryAddSingleton`. | ISS-001 — fixed 2026-07-08 |
| 2 | 4.7.2026 | PROP-028 | `JsonCommandLogRepository.cs` | ✅ Vyřešeno | `AppendAsync` používá `Task.Run` pro offload sync I/O. | ISS-002 — fixed 2026-07-08 |
| 3 | 4.7.2026 | PROP-027 | `AiServiceRegistration.cs` | ✅ Vyřešeno | `AddMetaForgeAi()` nyní registruje `PromptRegistry` a `PromptEvaluationService`. | ISS-003 — fixed 2026-07-08 |
| 4 | 4.7.2026 | PROP-024 | `Expression.cs` | 💡 Návrh | Redundantní `Kind` string a `ExpressionKind` enum. | Při major verzi odstranit `string Kind`. | [`ISS-004`](Docs/Issues/ISS-004_PROP-024_Kind-ExpressionKind-redundancy.md) |
| 5 | 4.7.2026 | PROP-025 | `IncrementalCodeGenerator.cs` | ✅ Vyřešeno | `GetMaxEntities()` nyní čte `_license.MaxEntities`. | ISS-005 — fixed 2026-07-08 |
| 6 | 4.7.2026 | PROP-025 | `CodeGenerator.cs` | ⚠️ Nízká | `sealed` → `class` kvůli dědičnosti, možnost nechtěného přepsání. | Zvážit kompozici místo dědičnosti. | [`ISS-006`](Docs/Issues/ISS-006_PROP-025_CodeGenerator-sealed-vs-composition.md) |
| 7 | 4.7.2026 | PROP-019 | `OllamaAiTranslator.cs` | ⚠️ Nízká | Duplikuje logiku Ollama HTTP API volání z `OllamaAdapter`. | Po stabilizaci MetaForge.Ai sjednotit. | [`ISS-007`](Docs/Issues/ISS-007_PROP-019_OllamaAiTranslator-duplicate-logic.md) |
| 8 | 4.7.2026 | PROP-019 | `DefaultBusinessTranslator.cs` | ⚠️ Nízká | `TryEnrichAsync` není v `IBusinessTranslator`, obchází rozhraní. | Přidat do rozhraní nebo vytvořit `IAsyncBusinessTranslator`. | [`ISS-008`](Docs/Issues/ISS-008_PROP-019_TryEnrichAsync-missing-interface.md) |
| 9 | 4.7.2026 | PROP-026 | `Program.cs` (CLI) | ⚠️ Nízká | CLI používá root `IServiceProvider` pro scoped služby. | Scope per command nebo singleton Facade. | [`ISS-009`](Docs/Issues/ISS-009_PROP-026_CLI-scoped-services.md) |
| 10 | 4.7.2026 | PROP-029 | `EfCoreForgeBlock.cs` | ⚠️ Nízká | `RequiredTier` není v `IForgeBlockCapabilityPackage`, nevynuceno. | Přidat do rozhraní nebo použít atribut. | [`ISS-010`](Docs/Issues/ISS-010_PROP-029_RequiredTier-not-enforced.md) |
| 11 | 4.7.2026 | PROP-029 | ForgeBlock projekty | ⚠️ Nízká | ForgeBlocky bez Scriban šablon — jen metadata. | Implementovat šablony v další iteraci. | [`ISS-011`](Docs/Issues/ISS-011_PROP-029_ForgeBlock-missing-templates.md) |
| 12 | 4.7.2026 | PROP-030 | `ReplayEngine.cs` | ✅ Vyřešeno | `CommandMigrationEngine` integrován — automatická migrace před replayem. | ISS-012 — fixed 2026-07-08 |
| 13 | 4.7.2026 | PROP-022 | `BusinessDocumentDiffer.cs` | ⚠️ Nízká | Diff nezachycuje Modify, relace, workflow. | Rozšířit o detekci Modified a dalších uzlů. | [`ISS-013`](Docs/Issues/ISS-013_PROP-022_Diff-Modify-not-detected.md) |

---

## Detaily návrhů

### PROP-017: Generators — ForgeBlock packaging a katalog

Více: [`Docs/Plans/PROP-017-Generators-ForgeBlock-Packaging.md`](Docs/Plans/PROP-017-Generators-ForgeBlock-Packaging.md)

### PROP-018: Translator — ExpertProjection a ProjectionOptions

Více: [`Docs/Plans/PROP-018-Translator-ExpertProjection.md`](Docs/Plans/PROP-018-Translator-ExpertProjection.md)

### PROP-019: Translator — IAiTranslator a AI-assisted překlad

Více: [`Docs/Plans/PROP-019-Translator-AiTranslator.md`](Docs/Plans/PROP-019-Translator-AiTranslator.md)

### PROP-020: BusinessModel — Architektonický upgrade dle původního konceptu

Více: [`Docs/Plans/PROP-020-BusinessModel-Architecture-Upgrade.md`](Docs/Plans/PROP-020-BusinessModel-Architecture-Upgrade.md)

> **Stav:** 🟢 Schváleno — přesunuto do PROPOSALS.md

### PROP-021: Testování — Property-based (FsCheck) a Snapshot (Verify)

Více: [`Docs/Plans/PROP-021-Tests-PropertyBased-Snapshot.md`](Docs/Plans/PROP-021-Tests-PropertyBased-Snapshot.md)

### PROP-022: Observabilita — OpenTelemetry tracing a BusinessModel diff

Více: [`Docs/Plans/PROP-022-Observability-Tracing-Diff.md`](Docs/Plans/PROP-022-Observability-Tracing-Diff.md)

### PROP-023: DX a architektonická vylepšení na zvážení

Více: [`Docs/Plans/PROP-023-DX-Architecture-Improvements-Future.md`](Docs/Plans/PROP-023-DX-Architecture-Improvements-Future.md)

### PROP-024: Core — StrongType/ValueObject, Expression System, Record elementy

Více: [`Docs/Plans/PROP-024-Core-StrongType-Expression-Record.md`](Docs/Plans/PROP-024-Core-StrongType-Expression-Record.md)

### PROP-025: Generators — Incremental, Partial Class, Scaffolding + Monetization

Více: [`Docs/Plans/PROP-025-Generators-Incremental-Monetization.md`](Docs/Plans/PROP-025-Generators-Incremental-Monetization.md)

### PROP-026: Host Surfaces — CLI, MCP, WebApi, REPL upgrade

Více: [`Docs/Plans/PROP-026-Host-Surfaces-CLI-MCP-WebApi-REPL.md`](Docs/Plans/PROP-026-Host-Surfaces-CLI-MCP-WebApi-REPL.md)

### PROP-027: AI Layer — MetaForge.Ai projekt, OllamaAdapter, PromptRegistry

Více: [`Docs/Plans/PROP-027-AI-Layer-MetaForge-Ai.md`](Docs/Plans/PROP-027-AI-Layer-MetaForge-Ai.md)

### PROP-028: Infrastructure — Persistence, Konfigurace, Caching

Více: [`Docs/Plans/PROP-028-Infrastructure-Persistence-Config-Cache.md`](Docs/Plans/PROP-028-Infrastructure-Persistence-Config-Cache.md)

### PROP-029: ForgeBlocks — Rozšíření a marketplace

Více: [`Docs/Plans/PROP-029-ForgeBlocks-Expansion-Marketplace.md`](Docs/Plans/PROP-029-ForgeBlocks-Expansion-Marketplace.md)

### PROP-030: Bezpečnost a stabilita — Schema Migration, Validation, Health

Více: [`Docs/Plans/PROP-030-Security-Stability-Schema-Validation-Health.md`](Docs/Plans/PROP-030-Security-Stability-Schema-Validation-Health.md)

### PROP-031: Core — Statement System a upgrade Expression pro těla metod

Více: [`Docs/Plans/PROP-031-Core-Statement-System.md`](Docs/Plans/PROP-031-Core-Statement-System.md)

### PROP-032: Integrační testy — Core + Generators (Snapshot-based)

Více: [`Docs/Plans/PROP-032-Integration-Tests-Core-Generators.md`](Docs/Plans/PROP-032-Integration-Tests-Core-Generators.md)

> Testovací matice: [`Docs/Integration/01-Integration-Test-Matrix.md`](Docs/Integration/01-Integration-Test-Matrix.md)

### PROP-034: Core Reference Documentation + Support Matrix

Více: [`Docs/Plans/PROP-034-Core-Reference-Documentation.md`](Docs/Plans/PROP-034-Core-Reference-Documentation.md)

> **Vychází z:** IDEA-001, IDEA-002, IDEA-003 (Perplexity analýza d773bf6a)
> **Obsahuje:** Docs/Core/ sada (Overview, Support Matrix, Type System, Type Kinds, Value Objects, Methods, Expressions, Roundtrip, Examples)
> **OQ-034-01:** Jak udržovat support matici živou? (ručně vs generovaně)
> **OQ-034-02:** Je roundtrip cílový stav nebo jen užitečná vlastnost?
> **OQ-034-03:** Má se matice verzovat s Core?

### IDEA-004 Follow-up: Method Expression Boundary

> **Follow-up k:** PROP-031 (Core Statement System)
> **Popis:** Doplnit `MethodBodyKind` (None/Structured/Text/AiBody) do `MethodElement` a vymezit hranici "co je AST a co už ne".
> **OQ:** Jaký je default `MethodBodyKind` pro nově parsované metody?
> **Zdroj:** IDEA-004 z `Docs/ideas/old_ideas/`

### PROP-035: C#-First Core Migration

Více: [`Docs/Plans/PROP-035-CSharp-First-Core-Migration.md`](Docs/Plans/PROP-035-CSharp-First-Core-Migration.md)

> **Vychází z:** Perplexity konverzace 05663298 (5 dotazů)
> **Obsahuje:** 7 commitů migrace (RootElement → ClassElement → MethodElement → Expressions → Translator → Tests → Docs)
> **Princip:** C#-first sémantický Core, ne syntaktický. Red lines: nikdy raw kód, formatting, prompt texty, pricing flagy.
> **OQ-035-01:** Kdy udělat breaking release?
> **OQ-035-02:** Převést BaseClassName na TypeModel??
> **OQ-035-03:** Přesný tvar LanguageCapabilityProfile?

### PROP-036: Core Specification Layer

Více: [`Docs/Plans/PROP-036-Core-Specification-Layer.md`](Docs/Plans/PROP-036-Core-Specification-Layer.md)

> **Vychází z:** Perplexity konverzace 05663298 (5 dotazů)
> **Obsahuje:** InvariantDefinition, InvariantExpression boolean AST, IInvariantEvaluator se Local/Scoped/Relational/Global scope, test generation z invariantů.
> **Princip:** Jeden zdroj pravdy pro invarianty → runtime validace + test generation + AI guardraily.
> **OQ-036-01:** GeneratorIntent na StrongType nebo odvodit z invariantů?
> **OQ-036-02:** Granularita AI-generated invariantů?
> **OQ-036-03:** IInvariantEvaluator v Core nebo Infrastructure?

### PROP-037: C# Completeness — Chybějící konstrukty + Projektová metadata + Roslyn Importer

Více: [`Docs/Plans/PROP-037-CSharp-Completeness-Importer.md`](Docs/Plans/PROP-037-CSharp-Completeness-Importer.md)

> **Vychází z:** GitHub task "Implement the plan" kroky 4, 5, 6
> **Obsahuje:** DelegateElement, EventElement, OperatorElement; rozšířený ProjectElement (TargetFramework, PackageReference); MetaForge.Core.Framework (DI, ASP.NET, EF); MetaForge.Importer (Roslyn-based, 2 režimy).
> **Celkem:** 47-69h, 6-9 dní
> **OQ-037-01:** MetaForge.Core.Framework jako samostatný projekt?
> **OQ-037-02:** Granularita framework metadata?
> **OQ-037-03:** Importer s přímou Roslyn závislostí nebo abstrakcí?

### PROP-038: Core DX & Diagnostics — Fluent Builder, DiagnosticBag, AttributeModel, XmlDocModel

Více: [`Docs/Plans/PROP-038-Core-DX-Diagnostics.md`](Docs/Plans/PROP-038-Core-DX-Diagnostics.md)

> **Vychází z:** Perplexity konverzace e0609fe1
> **Obsahuje:** Fluent Builder API (Class("X").WithProperty(...).Build()); DiagnosticBag s Console/JSON/InMemory reportéry; AttributeElement jako first-class; XmlDocElement strukturovaně.
> **Celkem:** 8-13h, 1-2 dny. Additivní změny.
> **OQ-038-01:** Attributes jako breaking change, nebo vedle starých?
> **OQ-038-02:** Extension methods nebo vnořené třídy pro buildery?

### PROP-039: Core Composability — TransformPipeline, Mixin/Trait, ConventionRegistry

Více: [`Docs/Plans/PROP-039-Core-Composability.md`](Docs/Plans/PROP-039-Core-Composability.md)

> **Vychází z:** Perplexity konverzace e0609fe1
> **Obsahuje:** TransformPipeline (middleware nad immutable modelem); Mixin/Trait (build-time expanze); ConventionRegistry (PascalCase, I-prefix, Async suffix).
> **Celkem:** 9-15h, 1-2 dny.
> **OQ-039-01:** TransformPipeline v Core nebo samostatný projekt?
> **OQ-039-02:** Mixiny + atributy + XML doc?
> **OQ-039-03:** Per-element override konvencí?

---

## Legenda priorit

- 🔴 Kritická — musí se implementovat co nejdříve
- 🟡 Vysoká — důležité pro další vývoj
- 🟢 Nízká — nice to have
- ⚪ Odloženo — zatím se neimplementuje
