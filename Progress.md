# Progress — Chronologický log změn

> Každá dokončená změna se zapisuje ve formátu:
> `[YYYY-MM-DD] {Epic}/{Slice} — {Popis změny} ({Autor})`

## Log

| Datum | Epic/Slice | Popis | Autor |
|-------|-----------|-------|-------|
| 2026-07-08 | PROP-043 — Generator Completeness | Plné pokrytí Core→C# generování: 15/15 expression typů (New, Await, Conversion, Default, IsPattern, Lambda, NullCoalescing, Switch expression), 13/13 statement typů (ForEach, Switch, TryCatch, Using, UsingDeclaration, LocalFunction), Event+Operator generování, Constructor/Field šablony napojeny, Delegate generování, MapType varování. Build: 0 chyb. | Copilot |
| 2026-07-08 | PROP-040 — Core Member Consistency | IMemberElement interface, PropertyElement +Attributes+XmlSummary+IMemberElement, MethodElement +XmlSummary+IMemberElement, EventElement/OperatorElement +IMemberElement. 11 testů (MemberConsistencyTests). | Copilot |
| 2026-07-08 | PROP-041 — ConstructorElement + FieldElement | ConstructorElement (factory: Basic/Private/Static, fluent: WithAccess/WithParameter/WithInitializer/WithBody), FieldElement (factory: Basic/ReadOnly/StaticReadOnly/Const, fluent: WithAccess/WithDefault), integrace do ClassElement (+Constructors/+Fields/+WithConstructor/+WithField). 16 testů (ConstructorFieldTests). | Copilot |
| 2026-07-08 | PROP-042 — Core Test Expansion | 8 nových guard kódů v CoreValidator (C13, C14, M13, M14, P9, P10, G11, G12), ValidateStruct metoda, 12 GuardValidationTests, celkem 45 nových testů (405 celkem). Build: 0 chyb. | Copilot |
| 2026-07-08 | PROP-037 — C# Completeness | 3 nové element typy: DelegateElement, EventElement, OperatorElement (OperatorKind enum s 26 hodnotami). ProjectElement rozšířen o TargetFramework, RootNamespace, NullableEnabled, ImplicitUsings, PackageReference, AnalyzerReference, ProjectReference. 21 testů (360 celkem). | Copilot |
| 2026-07-08 | PROP-039 — Core Composability | 3 soubory v Core/Composability/: ElementMixin (ConflictStrategy: Skip/Throw/Replace) + BuiltInMixins (Auditable, SoftDelete), ConventionRegistry (IConvention, 3 vestavěné konvence) + ConventionContext, ElementFingerprint (SHA256 dirty-tracking, IEquatable). 26 nových testů (339 celkem). Build: 0 chyb. | Copilot |
| 2026-07-08 | PROP-034 — Core Reference Documentation | 8 dokumentů v Docs/Core/: 00-Overview, 00-Support-Matrix (81 položek), 01-Type-System, 02-Type-Kinds, 03-Value-Objects, 04-Methods, 05-Expressions-and-AST, 06-Roundtrip-Boundary, 07-Examples. Každý dokument jednotný formát: C# → Core → popis, stav podpory, omezení. Matice slouží jako živý backlog. | Copilot |
| 2026-07-08 | PROP-036 — Core Specification Layer | 7 souborů v Core/Specifications/: InvariantSeverity, InvariantScope, InvariantExpression (7 typů boolean AST + builder), InvariantDefinition (s GeneratorIntent + InvariantProvenance), EvaluationResult+InvariantViolation, IInvariantEvaluator+InvariantEvaluationContext+IModelLookup, ReflectionBasedInvariantEvaluator (reflection-based property path resolver), BuiltInInvariants (12 standardních invariantů: 6 Method, 4 Class, 2 Property). 30 nových testů (313 celkem). Build: 0 chyb. | Copilot |
| 2026-07-08 | PROP-031 — Core Statement System (rozšíření) | 6 nových statement typů: SwitchStatement+SwitchCase, ForEachStatement, TryCatchStatement+CatchClause, UsingStatement, UsingDeclarationStatement, LocalFunctionStatement. StatementKind rozšířen o 6 hodnot. 6 testů (283 celkem). Generátory nevyžadují změnu. | Copilot |
| 2026-07-08 | PROP-038 — Core DX, Diagnostics & Pipeline | 3 fáze: (1) Fluent Builder API — 8 builderů + ModelDefinition + MetadataBag (MetadataScope, MergeStrategy, Keys) + integrace do RootElement/PropertyElement/MethodElement; (2) DiagnosticBag — DiagnosticSeverity, ElementPath, Diagnostic, IDiagnosticCollector, DiagnosticBag, BuildResult\<T\> s monadickým .Then()/.Map(), 3 reportéry; (3) TransformPipeline — IModelTransform, TransformContext, PipelineOptions, TransformPipeline, AttributeReflectionTransform. 15 nových testů (277 celkem). | Copilot |
| 2026-07-08 | PROP-035 — C#-First Core Migration | 7 commitů: (1) RootElement +Namespace +XmlSummary + fluent settery na všech 4 element typech, (2) ClassElement+InterfaceElement+StructElement +TypeParameters +TypeConstraints +PrimaryConstructorParameters, nový GenericConstraint s ConstraintKind enum (7 druhů), PrimaryRecord/Generic factory, (3) MethodElement +ExpressionBody +IsExtension +TypeParameters +TypeConstraints +Generic factory, (4) 8 nových expression typů (Lambda, New+MemberBinding, Default, Conversion, Await, Switch+SwitchArm, IsPattern+PatternKind, NullCoalescing) + NamedArgument do MethodCallExpression + rozšíření ExpressionKind, (5) LanguageCapabilityProfile zjednodušeno na C#-first licensing gate (Supported/Unsupported s wildcard "*", profily Default/Basic/Professional), (6) 13 nových validačních invariantních testů (262 celkem), (7) aktualizace New_Architecture 03/04/05. Code review: 3 BLOCKERY opraveny, 8 doporučení na follow-up. Build: 0 chyb, 262 testů prošlo. | Copilot |
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
