# PROPOSALS — Master Checklist

> Aktivní návrhy a jejich stav.
> Každý návrh musí mít odkaz na detailní markdown v `Docs/Plans/`.

## Aktivní návrhy — Zásobník dle priority implementace

| ID | Název | Vrstva | Priorita | Odhad | Závislosti | Odkaz |
|----|-------|--------|----------|-------|------------|-------|
| **PROP-018** | Translator — ExpertProjection a ProjectionOptions | Translator | 🟡 Střední | 2-3 dny | PROP-044 (hotovo) | [Detail](Docs/Plans/PROP-018-Translator-ExpertProjection.md) |
| **PROP-017** | ForgeBlock Packaging — BlueprintBuilder, katalog | Generators, ForgeBlocks | 🟢 Nízká | — | PROP-045 (hotovo) | [Detail](Docs/Plans/PROP-017-Generators-ForgeBlock-Packaging.md) |
| **PROP-023** | DX vylepšení — Typový SyncState, Layer stack, YAML DSL, Undo/redo | Průřezové | ⚪ Na zvážení | 5–9 dní | ∞ | [Detail](Docs/Plans/PROP-023-DX-Architecture-Improvements-Future.md) |

## Dokončené návrhy

| ID | Název | Datum dokončení | Odkaz |
|----|-------|-----------------|-------|
| **PROP-046** | AI Model Benchmarking — CoreElementComparer, strukturální testy | 2026-07-11 | [Detail](Docs/Plans/Implemented/PROP-046-AI-Model-Benchmarking.md) |
| **PROP-045** | Generator E2E Completeness — 13/13 scénářů (všechny renderery) | 2026-07-11 | [Detail](Docs/Plans/Implemented/PROP-045-Generator-E2E-Completeness.md) |
| **PROP-047** | Translator — Strong Type Mapping | 2026-07-10 | [Detail](Docs/Plans/Implemented/PROP-047-Translator-StrongType-Mapping.md) |
| **PROP-044** | Translator & BusinessModel — Workflow opravy, SyncState konsolidace | 2026-07-10 | [Detail](Docs/Plans/Implemented/PROP-044-Translator-BusinessModel-Fixes.md) |
| **PROP-042** | Core Test Expansion — FsCheck, snapshoty, guard testy (12+11+16 testů) | 2026-07-08 | [Detail](Docs/Plans/Implemented/PROP-042-Core-Test-Expansion.md) |
| **PROP-041** | ConstructorElement + FieldElement, integrace do ClassElement | 2026-07-08 | [Detail](Docs/Plans/Implemented/PROP-041-ConstructorElement-FieldElement.md) |
| **PROP-040** | Core Member Consistency — IMemberElement, PropertyElement Attributes+XmlSummary | 2026-07-08 | [Detail](Docs/Plans/Implemented/PROP-040-Core-Member-Consistency.md) |
| **PROP-039** | Core Composability — ElementMixin, ConventionRegistry, ElementFingerprint (26 testů) | 2026-07-08 | [Detail](Docs/Plans/Implemented/PROP-039-Core-Composability.md) |
| **PROP-038** | Core DX & Diagnostics — Fluent Builder API, MetadataBag, DiagnosticBag, TransformPipeline | 2026-07-08 | [Detail](Docs/Plans/Implemented/PROP-038-Core-DX-Diagnostics.md) |
| **PROP-037** | C# Completeness — DelegateElement, EventElement, OperatorElement, ProjectElement rozšíření | 2026-07-08 | [Detail](Docs/Plans/Implemented/PROP-037-CSharp-Completeness-Importer.md) |
| **PROP-036** | Core Specification Layer — InvariantDefinition, InvariantExpression, BuiltInInvariants (12) | 2026-07-08 | [Detail](Docs/Plans/Implemented/PROP-036-Core-Specification-Layer.md) |
| **PROP-035** | C#-First Core Migration — RootElement, ClassElement, MethodElement, 8 expression typů | 2026-07-08 | [Detail](Docs/Plans/Implemented/PROP-035-CSharp-First-Core-Migration.md) |
| **PROP-034** | Core Reference Documentation — Docs/Core/ sada (9 dokumentů) | 2026-07-08 | [Detail](Docs/Plans/Implemented/PROP-034-Core-Reference-Documentation.md) |
| **PROP-033** | Core Factory Methods & CoreValidator — 30+ factory metod, 19 validačních kódů | 2026-07-08 | [Detail](Docs/Plans/Implemented/PROP-033-Core-Element-Factories-Validation.md) |
| **PROP-032** | Integrační testy Core + Generators — SnapshotComparer, 48 testů (7 scénářů) | 2026-07-08 | [Detail](Docs/Plans/Implemented/PROP-032-Integration-Tests-Core-Generators.md) |
| **PROP-031** | Core Statement System — Switch, ForEach, TryCatch, Using, LocalFunction (6 nových) | 2026-07-08 | [Detail](Docs/Plans/Implemented/PROP-031-Core-Statement-System.md) |
| **PROP-020** | BusinessModel — architektonický upgrade (SyncState, CoreDetail, Provenance) | 2026-07-04 | [Detail](Docs/Plans/Implemented/PROP-020-BusinessModel-Architecture-Upgrade.md) |
| **PROP-030** | Bezpečnost a stabilita — Schema migrace, validace, health checks | 2026-07-04 | [Detail](Docs/Plans/Implemented/PROP-030-Security-Stability-Schema-Validation-Health.md) |
| **PROP-029** | ForgeBlocks — EF Core, AutoMapper, FluentValidation (PROP-029) | 2026-07-04 | [Detail](Docs/Plans/Implemented/PROP-029-ForgeBlocks-Expansion-Marketplace.md) |
| **PROP-028** | Infrastructure — persistence, konfigurace, caching | 2026-07-04 | [Detail](Docs/Plans/Implemented/PROP-028-Infrastructure-Persistence-Config-Cache.md) |
| **PROP-027** | AI Layer — MetaForge.Ai, OllamaAdapter, PromptRegistry | 2026-07-04 | [Detail](Docs/Plans/Implemented/PROP-027-AI-Layer-MetaForge-Ai.md) |
| **PROP-026** | Host Surfaces — CLI upgrade, MCP discovery, WebApi, REPL | 2026-07-04 | [Detail](Docs/Plans/Implemented/PROP-026-Host-Surfaces-CLI-MCP-WebApi-REPL.md) |
| **PROP-025** | Generators — Incremental, partial class, scaffolding, monetizace | 2026-07-04 | [Detail](Docs/Plans/Implemented/PROP-025-Generators-Incremental-Monetization.md) |
| **PROP-024** | Core — StrongType, Expression, Record elementy | 2026-07-04 | [Detail](Docs/Plans/Implemented/PROP-024-Core-StrongType-Expression-Record.md) |
| **PROP-022** | Observabilita — OpenTelemetry tracing, BusinessModel diff | 2026-07-04 | [Detail](Docs/Plans/Implemented/PROP-022-Observability-Tracing-Diff.md) |
| **PROP-021** | Testování — Property-based (FsCheck) a Snapshot (Verify) | 2026-07-04 | [Detail](Docs/Plans/Implemented/PROP-021-Tests-PropertyBased-Snapshot.md) |
| **PROP-019** | Translator — IAiTranslator a AI-assisted překlad | 2026-07-04 | [Detail](Docs/Plans/Implemented/PROP-019-Translator-AiTranslator.md) |
| **PROP-009** | Testovací infrastruktura — 63 unit testů | 2026-07-04 | [Detail](Docs/Plans/Implemented/PROP-009-Tests.md) |
| **PROP-008** | ForgeBlock balíky — Math, String, Validation | 2026-07-04 | [Detail](Docs/Plans/Implemented/PROP-008-ForgeBlocks.md) |
| **PROP-007** | Generators — CSharpGenerator (C#-first) | 2026-07-04 | [Detail](Docs/Plans/Implemented/PROP-007-Generators.md) |
| **PROP-006** | AI Layer — volitelná AI s graceful fallback | 2026-07-04 | [Detail](Docs/Plans/Implemented/PROP-006-AI-Layer.md) |
| **PROP-005** | Host Surfaces — CLI a MCP | 2026-07-04 | [Detail](Docs/Plans/Implemented/PROP-005-Host-Surfaces.md) |
| **PROP-004** | Translator — Facade, projekce, překlad | 2026-07-04 | [Detail](Docs/Plans/Implemented/PROP-004-Translator.md) |
| **PROP-003** | BusinessModel — dokument, CommandLog, replay | 2026-07-04 | [Detail](Docs/Plans/Implemented/PROP-003-BusinessModel.md) |
| **PROP-002** | Core vrstva — typový model, elementy, katalog | 2026-07-04 | [Detail](Docs/Plans/Implemented/PROP-002-Core.md) |
| **PROP-001** | Governance a Project Scaffold | 2026-07-04 | [Detail](Docs/Plans/Implemented/PROP-001-Governance.md) |

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
