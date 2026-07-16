# PROPOSALS — Master Checklist

> Aktivní návrhy a jejich stav.
> Každý návrh musí mít odkaz na detailní markdown v `Docs/Plans/`.

## Aktivní návrhy — Zásobník dle priority implementace

> Poslední aktualizace: 2026-07-16 (hloubková analýza — přehodnocení priorit)

| ID | Název | Vrstva | Priorita | Odhad | Závislosti | Odkaz |
|----|-------|--------|----------|-------|------------|-------|
| **PROP-055** | ReferenceGraph — typový graf závislostí, detekce cyklů, správné pořadí generování | Core, Generators | 🔴 Vysoká | ~2 dny | Není | [Detail](Docs/Plans/PROP-055-ReferenceGraph.md) |
| **PROP-060** | Element Identity Stabilization — přidání `Guid Id` do `IMemberElement`, oprava Business→Core ID mapping, ID-first foundation | Core, BusinessModel, Translator | 🔴 Kritická | 1–2 dny | PROP-040 (hotovo) | [Detail](Docs/Plans/PROP-060-Element-Identity-Stabilization.md) |
| **PROP-057** | ElementContract + VerificationModel — sémantické kontrakty pro elementy, fingerprint-based verifikační stavy | Core, Infrastructure | 🔴 Vysoká | 3–4 dny | **PROP-060**, PROP-024, PROP-036, PROP-039 (hotovo) | [Detail](Docs/Plans/PROP-057-ElementContract-VerificationModel.md) |
| **PROP-054** | ForgeBlock DI Extension Methods — `IForgeBlockDiProvider`, `AddEfCore()`, `AddAutoMapper()`, `AddFluentValidation()` | ForgeBlocks, Infrastructure | 🟡 Střední | ~2 dny | ISS-011, PROP-017 (hotovo) | [Detail](Docs/Plans/PROP-054-ForgeBlock-DI-Extension-Methods.md) |
| **PROP-058** | Sandbox Preview Runner — izolované spouštění metod s JSON vstupy, MVP: jen čisté funkce bez závislostí | Generators, CLI, Infrastructure | 🟡 Střední | 3–5 dní | **PROP-057** | [Detail](Docs/Plans/PROP-058-Sandbox-Preview-Runner.md) |
| **PROP-056** | Projection Unification + JSON Snapshot — sjednocení `ProjectionView` a `ExpertProjectionView` do `DocumentProjection`, `ToJson(filter)` | Translator, BusinessModel | 🟡 Střední | ~3 dny | PROP-018 (hotovo) | [Detail](Docs/Plans/PROP-056-Projection-Unification-JsonSnapshot.md) |
| **PROP-053** | Web Frontend — Blazor Server s MudBlazor | Frontend | ⚪ Na zvážení | ~5 dní | PROP-044, PROP-045 (hotovo) | [Detail](Docs/Plans/PROP-053-Web-Frontend-Blazor.md) |
| **PROP-023** | DX vylepšení — Typový SyncState, Layer stack, YAML DSL, Undo/redo | Průřezové | ⚪ Na zvážení | 5–9 dní | ∞ | [Detail](Docs/Plans/PROP-023-DX-Architecture-Improvements-Future.md) |

## Odložené návrhy

| ID | Název | Důvod odložení |
|----|-------|-----------------|
| **PROP-059** | Resilience & Healing Layer — user-facing resilience, method-level healing s audit trailem | 🔴 **Odloženo 2026-07-16**: PROP-043/045 generátory jsou stabilní. Aktivovat až po PROP-058 v produkci, pokud frekvence chyb > 10 %. Pokud < 5 % → DROPPED. |
| **CODE-003** | Monetizace — IGenerationCostPolicy, tier licence, billing gate | Platforma může běžet bez monetizace. Plánováno po ověření trakce. |
| **B5** | WebApi — REST API host surface | CLI + MCP pro MVP stačí. Až bude poptávka. |

## Zamítnuté návrhy

| ID | Název | Důvod zamítnutí | Datum |
|----|-------|-----------------|-------|
| **PROP-050** | Self-Healing Pipeline | Příliš experimentální — složité AST manipulace s rizikem sémantických změn, obtížná testovatelnost. PROP-059 (nástupce) je odložen — čeká se na data o reálné frekvenci chyb generování. | 2026-07-11 |

## Dokončené návrhy

| ID | Název | Datum dokončení | Odkaz |
|----|-------|-----------------|-------|
| **PROP-052** | Operator/Event/Delegate — 4 Delegate snapshot testy (D1-D4) | 2026-07-11 | [Detail](Docs/Plans/Implemented/PROP-052-OperatorEventDelegate-FollowUp.md) |
| **PROP-048** | Generator Render Core Tests — 72 ExpressionRenderer + 13 StatementRenderer + 6 TemplateManager = 91 unit testů, 3 bugy v ExpressionRenderer opraveny (MapType, RenderSwitch, decimal) | 2026-07-11 | [Detail](Docs/Plans/PROP-048-Generator-Render-Core-Tests.md) |
| **PROP-049** | Test Framework Consolidation — UPDATE_SNAPSHOTS env var v SnapshotComparer | 2026-07-11 | [Detail](Docs/Plans/PROP-049-Test-Framework-Consolidation.md) |
| **PROP-051** | Support Matrix — Strojově čitelný YAML contract map (73 položek, 5 kategorií, 4 contract statusy) | 2026-07-11 | [Detail](Docs/Plans/PROP-051-Support-Matrix-Contract-Map.md) |
| **CODE-001** | CLI generate/export command — BusinessModel→Core→C# pipeline, `save` command | 2026-07-12 | CLI `generate --output` + `save` commands |
| **CODE-002** | Perzistence v CLI — JSONL CommandLog + JSON Document, load na startupu | 2026-07-12 | `InfrastructureServiceRegistration` v CLI DI |
| **PROP-018** | ExpertProjection — 6 modelů, ProjectionOptions, diagnostika, relace | 2026-07-12 | `ExpertProjectionView`, `ProjectionReadService.GetExpertProjection()` |
| **PROP-017** | ForgeBlock Packaging — BlueprintBuilder, CodePackageDependency, integrator | 2026-07-12 | `ForgeBlockBlueprint`, `ForgeBlockPackageIntegrator` |
| **CODE-004** | ISS-004 — Kind jako computed property (14 Expression potomků převedeno) | 2026-07-12 | [Detail](Docs/Issues/Solved-Issues/ISS-004_PROP-024_Kind-ExpressionKind-redundancy.md) |
| **CODE-006** | ISS-006 — CodeGenerator sealed + TieredCodeGenerator/IncrementalCodeGenerator kompozice | 2026-07-12 | [Detail](Docs/Issues/Solved-Issues/ISS-006_PROP-025_CodeGenerator-sealed-vs-composition.md) |
| **CODE-011** | ISS-011 — ForgeBlock plugin šablony: IForgeBlockTemplateProvider, 4 Scriban šablony | 2026-07-12 | [Detail](Docs/Issues/Solved-Issues/ISS-011_PROP-029_ForgeBlock-missing-templates.md) |
| **CODE-CLN** | Issues cleanup — 15 issues vyřešeno a přesunuto do Solved-Issues/ | 2026-07-12 | [Detail](Docs/Issues/Solved-Issues/) |
| **CODE-CI** | CI/CD — build.yml: GitHub Actions, build + testy 6 projektů | 2026-07-13 | `.github/workflows/build.yml` |
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

---

## Legenda stavů

- 🟡 Draft — návrh se píše
- 🟢 Schváleno — připraveno k implementaci
- 🔵 V implementaci — právě se implementuje
- ✅ Dokončeno — implementováno a otestováno
- ❌ Zamítnuto — nebude se implementovat
