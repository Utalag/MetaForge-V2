# PROPOSALS — Master Checklist

> Aktivní návrhy a jejich stav.
> Každý návrh musí mít odkaz na detailní markdown v `Docs/Plans/`.

## Aktivní návrhy

> ⚠️ **Pořadí implementace řídí** [`Docs/Plans/Implementation-Roadmap.md`](Docs/Plans/Implementation-Roadmap.md).
> Fáze určují, co se implementuje teď a co až později. Paralelní streamy mohou běžet současně.

| ID | Název | Priorita | Odhad | Závislosti | Odkaz |
|----|-------|----------|-------|------------|-------|
| PROP-036 | **Core Specification Layer** — InvariantDefinition, booleovský AST, IInvariantEvaluator | 🟡 Vysoká | 5–8 dní | PROP-035 ✅, PROP-038 ✅, PROP-031 ✅ | [Detail](Docs/Plans/PROP-036-Core-Specification-Layer.md) |
| PROP-037 | **C# Completeness** — Delegate, Event, Operator, Roslyn Importer | 🟢 Střední | 6–9 dní | PROP-035 ✅, PROP-038 ✅, PROP-031 ✅ | [Detail](Docs/Plans/PROP-037-CSharp-Completeness-Importer.md) |
| **OSTATNÍ** | | | | | |
| PROP-034 | Core Reference Documentation + Support Matrix | 🟡 Vysoká | 4 dny | — | [Detail](Docs/Plans/PROP-034-Core-Reference-Documentation.md) |
| PROP-039 | Core Composability — Mixin/Trait, ConventionRegistry | 🟢 Střední | 1,5–2 dny | PROP-038 | [Detail](Docs/Plans/PROP-039-Core-Composability.md) |
| PROP-032 | Integrační testy Core + Generators | 🟡 Vysoká | 7 dní | PROP-031 ✅ | [Detail](Docs/Plans/PROP-032-Integration-Tests-Core-Generators.md) |
| PROP-023 | DX vylepšení — Typový SyncState, Layer stack, YAML DSL, Undo/redo | ⚪ Na zvážení | 5–9 dní | ∞ | [Detail](Docs/Plans/PROP-023-DX-Architecture-Improvements-Future.md) |
| PROP-033 | Core Element Factory Methods & CoreValidator | 🟢 Schváleno | — | PROP-031 ✅, PROP-032 ✅ | [Detail](Docs/Plans/PROP-033-Core-Element-Factories-Validation.md) |


## Dokončené návrhy

| ID | Název | Datum dokončení | Odkaz |
|----|-------|-----------------|-------|
| PROP-031 | Core Statement System — rozšíření: Switch, ForEach, TryCatch, Using, UsingDeclaration, LocalFunction; StatementKind rozšířen o 6 hodnot | 2026-07-08 | [Docs/Plans/PROP-031-Core-Statement-System.md](Docs/Plans/PROP-031-Core-Statement-System.md) |
| PROP-038 | Core DX & Diagnostics — Fluent Builder API (8 builderů), MetadataBag, DiagnosticBag+BuildResult, TransformPipeline+AttributeReflection | 2026-07-08 | [Docs/Plans/PROP-038-Core-DX-Diagnostics.md](Docs/Plans/PROP-038-Core-DX-Diagnostics.md) |
| PROP-035 | C#-First Core Migration — RootElement, ClassElement, MethodElement, 8 expression typů, GenericConstraint, LanguageCapabilityProfile | 2026-07-08 | [Docs/Plans/PROP-035-CSharp-First-Core-Migration.md](Docs/Plans/PROP-035-CSharp-First-Core-Migration.md) |
| PROP-020 | BusinessModel — architektonický upgrade | 2026-07-04 | [Docs/Plans/PROP-020-BusinessModel-Architecture-Upgrade.md](Docs/Plans/PROP-020-BusinessModel-Architecture-Upgrade.md) |
| PROP-001 | Governance a Project Scaffold | 2026-07-04 | [Docs/Plans/PROP-001-Governance.md](Docs/Plans/PROP-001-Governance.md) |
| PROP-002 | Core vrstva — typový model, elementy, katalog | 2026-07-04 | [Docs/Plans/PROP-002-Core.md](Docs/Plans/PROP-002-Core.md) |
| PROP-003 | BusinessModel — dokument, CommandLog, replay | 2026-07-04 | [Docs/Plans/PROP-003-BusinessModel.md](Docs/Plans/PROP-003-BusinessModel.md) |
| PROP-004 | Translator — Facade, projekce, překlad | 2026-07-04 | [Docs/Plans/PROP-004-Translator.md](Docs/Plans/PROP-004-Translator.md) |
| PROP-005 | Host Surfaces — CLI a MCP | 2026-07-04 | [Docs/Plans/PROP-005-Host-Surfaces.md](Docs/Plans/PROP-005-Host-Surfaces.md) |
| PROP-006 | AI Layer — volitelná AI s graceful fallback | 2026-07-04 | [Docs/Plans/PROP-006-AI-Layer.md](Docs/Plans/PROP-006-AI-Layer.md) |
| PROP-007 | Generators — CSharpGenerator (C#-first) | 2026-07-04 | [Docs/Plans/PROP-007-Generators.md](Docs/Plans/PROP-007-Generators.md) |
| PROP-008 | ForgeBlock balíky — Math, String, Validation | 2026-07-04 | [Docs/Plans/PROP-008-ForgeBlocks.md](Docs/Plans/PROP-008-ForgeBlocks.md) |
| PROP-009 | Testovací infrastruktura — 63 unit testů | 2026-07-04 | [Docs/Plans/PROP-009-Tests.md](Docs/Plans/PROP-009-Tests.md) |
| PROP-024 | Core — StrongType, Expression, Record, Source Gen | 2026-07-04 | [Docs/Plans/PROP-024-Core-StrongType-Expression-Record.md](Docs/Plans/PROP-024-Core-StrongType-Expression-Record.md) |
| PROP-027 | AI Layer — MetaForge.Ai, Ollama, PromptRegistry | 2026-07-04 | [Docs/Plans/PROP-027-AI-Layer-MetaForge-Ai.md](Docs/Plans/PROP-027-AI-Layer-MetaForge-Ai.md) |
| PROP-028 | Infrastructure — persistence, config, caching | 2026-07-04 | [Docs/Plans/PROP-028-Infrastructure-Persistence-Config-Cache.md](Docs/Plans/PROP-028-Infrastructure-Persistence-Config-Cache.md) |
| PROP-019 | Translator — AI enrichment (IAiTranslator) | 2026-07-04 | [Docs/Plans/PROP-019-Translator-AiTranslator.md](Docs/Plans/PROP-019-Translator-AiTranslator.md) |
| PROP-025 | Generators — Incremental, monetizace, scaffold | 2026-07-04 | [Docs/Plans/PROP-025-Generators-Incremental-Monetization.md](Docs/Plans/PROP-025-Generators-Incremental-Monetization.md) |
| PROP-026 | Host Surfaces — CLI upgrade, MCP discovery | 2026-07-04 | [Docs/Plans/PROP-026-Host-Surfaces-CLI-MCP-WebApi-REPL.md](Docs/Plans/PROP-026-Host-Surfaces-CLI-MCP-WebApi-REPL.md) |
| PROP-029 | ForgeBlocks — EF Core, AutoMapper, FluentValidation | 2026-07-04 | [Docs/Plans/PROP-029-ForgeBlocks-Expansion-Marketplace.md](Docs/Plans/PROP-029-ForgeBlocks-Expansion-Marketplace.md) |
| PROP-030 | Bezpečnost — schema migrace, validace, health | 2026-07-04 | [Docs/Plans/PROP-030-Security-Stability-Schema-Validation-Health.md](Docs/Plans/PROP-030-Security-Stability-Schema-Validation-Health.md) |
| PROP-021 | Testování — FsCheck property + Verify snapshot | 2026-07-04 | [Docs/Plans/PROP-021-Tests-PropertyBased-Snapshot.md](Docs/Plans/PROP-021-Tests-PropertyBased-Snapshot.md) |
| PROP-022 | Observabilita — OpenTelemetry, BusinessModel diff | 2026-07-04 | [Docs/Plans/PROP-022-Observability-Tracing-Diff.md](Docs/Plans/PROP-022-Observability-Tracing-Diff.md) |

## Zamítnuté návrhy

| ID | Název | Důvod zamítnutí | Datum |
|----|-------|-----------------|-------|
| —  | —     | —               | —     |

---

## Legenda stavů

- 🟡 Draft — návrh se píše
- 🟢 Schváleno — připraveno k implementaci
- 🔵 V implementaci — právě se implementuje
- ✅ Dokončeno — implementováno a otestováno
- ❌ Zamítnuto — nebude se implementovat
