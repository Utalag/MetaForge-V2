# Progress — Chronologický log změn

> Každá dokončená změna se zapisuje ve formátu:
> `[YYYY-MM-DD] {Epic}/{Slice} — {Popis změny} ({Autor})`

## Log

| Datum | Epic/Slice | Popis | Autor |
|-------|-----------|-------|-------|
| 2026-07-04 | Epic 1 — Governance | Vytvoření solution, PROPOSALS.md, Progress.md, Memories.md, PROPOSALS_NEXT.md, README.md | Copilot |
| 2026-07-04 | Epic 2 — Core | Kompletní Core vrstva: typový model, elementy, katalog, ForgeBlock, ValueObjects, inference, standard libraries | Copilot |
| 2026-07-04 | Epic 3 — BusinessModel | BusinessAuthoringDocument, CommandLog, ReplayEngine, PatchEngine a operace | Copilot |
| 2026-07-04 | Epic 4 — Translator | Facade, projekce, překlad, write-back, UpdateAttributeOp | Copilot |
| 2026-07-04 | Epic 5 — Host Surfaces | CLI a MCP host surfaces, Composition Root, appsettings | Copilot |
| 2026-07-04 | Epic 6 — AI | IAiBackendAdapter, OllamaAdapter, AI inference, enrichment, DI registration | Copilot |
| 2026-07-04 | Epic 7 — Generators | CSharpGenerator, BaseCodeGenerator, LanguageMapping | Copilot |
| 2026-07-04 | Epic 8 — ForgeBlocks | Math, String, Validation ForgeBlock balíky | Copilot |
| 2026-07-04 | Epic 9 — Testy | 4 test projekty, 63 unit testů, TypeModel, Catalog, CommandLog, Replay, Patch, Translator, Generators | Copilot |
| 2026-07-04 | PROP-020 — BusinessModel Upgrade | Architektonický upgrade: immutabilita (sealed record), CoreDetail, SyncState, CommandLog provenance, MutationId idempotence, Workflow modely, validace, BusinessIdAllocator, enum typy (BehaviorKind, RelationKind), ProjectInfo. 67 testů prošlo. | Copilot |
| 2026-07-04 | PROP-020 — Fáze 1 (Additive) | CoreInfoSource, AttributeSyncState, BusinessAttributeCoreDetail, CoreDetail na BusinessAttributeNode, BusinessIdAllocator | Copilot |
| 2026-07-04 | PROP-020 — Fáze 2 (CommandLog) | CommandSource, CommandIssuedBy, CommandProvenance, rozšíření CommandEnvelope (StreamId, Provenance, MutationId), idempotence CommandLogStore.TryAppend | Copilot |
| 2026-07-04 | PROP-020 — Fáze 3 (Immutabilita) | Všechny modely → sealed record s init-only + IReadOnlyList; PatchEngine, ReplayEngine, IPatchOperation, všechny operace přepsány pro immutable pattern; SetCoreDetailOp, UpdateSyncStateOp; WriteBackService → SetCoreDetailOp; BusinessAuthoringHostFacade opraven | Copilot |
| 2026-07-04 | PROP-020 — Fáze 4 (Workflow+Validace) | BusinessBehaviorKind, BusinessRelationKind, BusinessProjectInfo, Workflow modely (6 typů), BusinessDocumentValidator, BusinessValidationIssue | Copilot |
| 2026-07-04 | PROP-020 — Dokumentace | Aktualizace New_Architecture dle PROP-020: 07-BusinessModel.md (kompletní přepis), 08-Translator.md (CoreDetail/SyncState projekce, SetCoreDetailOp), 00-Index.md (status), 18-Ready-to-Run-Prompts.md (CommandEnvelope, CommandLogStore prompty) | Copilot |
| 2026-07-04 | PROP-028 — Infrastructure | MetaForge.Infrastructure projekt: JSONL persistence (JsonCommandLogRepository), JsonDocumentRepository, InMemoryCommandLogRepository, IOptions konfigurace (MetaForgeOptions, StorageOptions, AiOptions), CheckpointProjectionCache, InfrastructureServiceRegistration, FileSystemProvider | Copilot |
| 2026-07-04 | PROP-024 — Core (StrongType+Expression+Record) | StrongType/ValueObject/ConversionOptions/ValueObjectValidationRule, CatalogManager StrongType registry, Expression hierarchie (11 druhů, ExpressionKind, BinaryOperator/UnaryOperator), IConstraintInferencer + RuleBasedConstraintInferencer, ClassElement.IsRecord | Copilot |
| 2026-07-04 | PROP-027 — AI Layer | MetaForge.Ai projekt: IAiBackendAdapter, OllamaAdapter (Ollama /api/generate), AiConstraintInferencer, AiTranslationService, PromptRegistry (YAML frontmatter .prompt.md), PromptEvaluationService/PromptEvaluator, 3 prompt templates, AiServiceRegistration | Copilot |
| 2026-07-04 | PROP-028/024/027 — Dokumentace | Aktualizace New_Architecture dle PROP-028, PROP-024, PROP-027: 11-Infrastructure.md (Caching, Configuration), 06-Core-Services.md (StrongType, ICatalogProvider, IConstraintInferencer), 05-Core-Behaviors.md (Expression hierarchie), 09-AI-Layer.md (OllamaAdapter, PromptRegistry, PromptEvaluator), 00-Index.md (status) | Copilot |
| 2026-07-04 | PROP-026 — Host Surfaces | CLI migrováno na System.CommandLine + Spectre.Console (CliOutputFormatter s tabulkami/panely), MCP s JSON-RPC (stdin/stdout loop + McpToolDiscovery s dynamickými tools podle dokumentu), WebApi zatím neimplementováno | Copilot |
| 2026-07-04 | PROP-029 — ForgeBlocks | 3 nové ForgeBlock balíky: AutoMapper (Domain tier, 3 capability), EF Core (Infrastructure tier, 5 capability), FluentValidation (Infrastructure tier, 3 capability). Všechny implementují IForgeBlockCapabilityPackage s tier omezením. Core: ForgeBlockRegistry, IForgeBlockPackage, IForgeBlockCapabilityPackage, ForgeBlockPackageDescriptor, DiscoveryMetadata | Copilot |
| 2026-07-04 | PROP-026/029 — Dokumentace | Aktualizace New_Architecture dle PROP-026, PROP-029: 12-Host-Surfaces.md (System.CommandLine, Spectre.Console, McpToolDiscovery, JSON-RPC), 27-ForgeBlock-External-Libraries.md (stav implementace 6 ForgeBlocků, tier model), 00-Index.md (status) | Copilot |

---

## Checkpointy

| Datum | Epic | Tag |
|-------|------|-----|
| 2026-07-04 | Epic 2 | checkpoint/epic-2-done |
| 2026-07-04 | Epic 3 | checkpoint/epic-3-done |
| 2026-07-04 | Epic 4 | checkpoint/epic-4-done |
| 2026-07-04 | Epic 5 | checkpoint/epic-5-done |
| 2026-07-04 | Epic 6 | checkpoint/epic-6-done |
| 2026-07-04 | Epic 7 | checkpoint/epic-7-done |
| 2026-07-04 | Epic 8 | checkpoint/epic-8-done |
| 2026-07-04 | Epic 9 | checkpoint/epic-9-done |
