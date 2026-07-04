# Scaffold — Projekty a složky

> Fyzická struktura nového projektu. Každá složka má jasný účel.

---

## Solution

```xml
<!-- MetaForge.slnx -->
<Solution>
  <Folder Name="/Src/">
    <Project Path="Src/MetaForge.Abstractions/MetaForge.Abstractions.csproj" />
    <Project Path="Src/MetaForge.Core/MetaForge.Core.csproj" />
    <Project Path="Src/MetaForge.BusinessModel/MetaForge.BusinessModel.csproj" />
    <Project Path="Src/MetaForge.Translator/MetaForge.Translator.csproj" />
    <Project Path="Src/MetaForge.Infrastructure/MetaForge.Infrastructure.csproj" />
    <Project Path="Src/MetaForge.Cli/MetaForge.Cli.csproj" />
    <Project Path="Src/MetaForge.Mcp/MetaForge.Mcp.csproj" />
    <Project Path="Src/MetaForge.WebApi/MetaForge.WebApi.csproj" />
    <Project Path="Src/MetaForge.Generators/MetaForge.Generators.csproj" />
    <Project Path="Src/MetaForge.Ai/MetaForge.Ai.csproj" />
  </Folder>

---

## Složková struktura — Src/MetaForge.Abstractions/

```
Src/MetaForge.Abstractions/
├── MetaForge.Abstractions.csproj
├── Enums/
│   ├── MetadataState.cs            # Draft → Valid → Invalid → Ready
│   └── ConstraintKind.cs           # Precondition, Invariant, Postcondition
├── Results/
│   └── ValidationResult.cs         # Severity, Message, RuleCode
└── Contracts/
    ├── IValidatable.cs
    └── IDiscoverySession.cs
```

---

## Složková struktura — Src/MetaForge.Infrastructure/

```
Src/MetaForge.Infrastructure/
├── MetaForge.Infrastructure.csproj
├── Persistence/
│   ├── ICommandLogRepository.cs        # Uložení/načtení CommandLog
│   ├── IDocumentRepository.cs          # Uložení/načtení BusinessAuthoringDocument
│   ├── JsonCommandLogRepository.cs     # File-based implementace
│   ├── JsonDocumentRepository.cs
│   └── InMemoryCommandLogRepository.cs # Pro testy
└── FileSystem/
    └── FileSystemProvider.cs
```
  <Folder Name="/Src/ForgeBlocks/">
    <Project Path="Src/ForgeBlocks/Math/MetaForge.ForgeBlocks.Math.csproj" />
    <Project Path="Src/ForgeBlocks/String/MetaForge.ForgeBlocks.String.csproj" />
  </Folder>
  <Folder Name="/Tests/">
    <Project Path="Tests/MetaForge.Core.Tests/MetaForge.Core.Tests.csproj" />
    <Project Path="Tests/MetaForge.BusinessModel.Tests/MetaForge.BusinessModel.Tests.csproj" />
    <Project Path="Tests/MetaForge.Translator.Tests/MetaForge.Translator.Tests.csproj" />
    <Project Path="Tests/MetaForge.Generators.Tests/MetaForge.Generators.Tests.csproj" />
    <Project Path="Tests/MetaForge.WebApi.Tests/MetaForge.WebApi.Tests.csproj" />
  </Folder>
</Solution>
```

---

## Složková struktura — Src/MetaForge.Core/

```
Src/MetaForge.Core/
├── MetaForge.Core.csproj
├── Abstractions/
│   ├── IValidatable.cs
│   ├── ICodeGenerator.cs
│   ├── AppRoot.cs
│   ├── RootElement.cs
│   └── GeneratedCodeArtifact.cs
├── DataTypes/
│   ├── DataType.cs
│   ├── EntityKind.cs
│   ├── SemanticCollection.cs
│   ├── TypeModel.cs
│   └── TypeMapper.cs                 # mapování DataType na DB typy (SQL Server, PostgreSQL, SQLite...)
├── Elements/
│   ├── ProjectElement.cs
│   ├── Types/
│   │   ├── ClassElement.cs
│   │   ├── InterfaceElement.cs
│   │   ├── EnumElement.cs
│   │   └── StructElement.cs
│   ├── Members/
│   │   ├── PropertyElement.cs
│   │   ├── MethodElement.cs
│   │   └── ParameterElement.cs
│   ├── Modifiers/
│   │   └── AccessModifier.cs
│   ├── Primitives/
│   │   ├── Field.cs
│   │   ├── Property.cs
│   │   ├── Parameter.cs
│   │   └── Variable.cs
│   └── Expressions/
│       ├── Expression.cs
│       ├── ComputedExpression.cs
│       ├── ComputedOperation.cs
│       ├── Statement.cs
│       ├── Comment.cs
│       ├── IExpressionRenderer.cs
│       ├── ExpressionRendererRegistry.cs
│       ├── SemanticMath.cs
│       └── SemanticStandardLibrary.cs
├── Inference/
│   ├── IConstraintInferencer.cs
│   ├── RuleBasedConstraintInferencer.cs
│   └── Boundary/
│       ├── BoundaryAnalysisResult.cs
│       ├── IDomainAnalyzer.cs
│       ├── MethodBoundaryAnalyzer.cs
│       ├── BoundaryRule.cs
│       └── DomainAnalyzers/
│           ├── MathBoundaryAnalyzer.cs
│           ├── StringBoundaryAnalyzer.cs
│           ├── FinanceBoundaryAnalyzer.cs
│           ├── CollectionBoundaryAnalyzer.cs
│           ├── RulesBoundaryAnalyzer.cs
│           └── GenericBoundaryAnalyzer.cs
├── StandardLibraries/
│   ├── IStandardLibraryTranslator.cs
│   ├── IStandardLibraryTranslatorRegistry.cs
│   ├── StandardLibraryTranslatorRegistry.cs
│   ├── StandardLibraryRequirements.cs
│   └── StandardLibraryRequirementResolver.cs
├── Configuration/
│   └── AIInferenceSettings.cs
├── ValueObjects/
│   ├── StrongTypeDescriptor.cs
│   ├── VogenConversions.cs
│   └── ValueObjectValidationRule.cs
├── Configuration/
│   └── AIInferenceSettings.cs
├── Common/
│   ├── MetadataState.cs            # Draft → Valid → Invalid → Ready
│   ├── LayerType.cs                # Domain, Database, Contract, Service, Api
│   └── CodePackageDependency.cs    # NuGet balíček závislostí
├── Catalog/
│   ├── CatalogItem.cs
│   ├── CatalogSearchOptions.cs
│   ├── PresetDefinition.cs
│   ├── CatalogManager.cs
│   ├── ICatalogProvider.cs
│   ├── BuiltInCatalogProvider.cs
│   ├── FileSystemCatalogProvider.cs
│   ├── MarketplaceCatalogProvider.cs
│   ├── ForgeBlockRegistryCatalogProvider.cs
│   ├── NodePresetSuggester.cs
│   ├── TypeResolution.cs
│   ├── PresetDefinition.cs
│   └── ValueObjectDefinition.cs
├── Discovery/
│   ├── IDiscoverySession.cs
│   ├── DiscoveryQuery.cs
│   ├── DiscoveryResult.cs
│   ├── DefaultDiscoverySession.cs
│   ├── DiscoveryTag.cs
│   └── DiscoveryMetadata.cs
├── ForgeBlockPackages/
│   ├── IForgeBlockPackage.cs
│   ├── IForgeBlockCapabilityPackage.cs
│   ├── IForgeBlockDiscoveryContributor.cs
│   ├── ForgeBlockRegistry.cs
│   ├── ForgeBlockCapability.cs
│   ├── ForgeBlockPackageDescriptor.cs
│   └── ForgeBlockCatalogEntryDescriptor.cs
└── Validation/
    ├── ValidationResult.cs
    └── ValidationPipeline.cs
```

---

## Složková struktura — Src/MetaForge.BusinessModel/

```
Src/MetaForge.BusinessModel/
├── MetaForge.BusinessModel.csproj
├── Models/
│   ├── BusinessAuthoringDocument.cs
│   ├── BusinessEntityNode.cs
│   ├── BusinessAttributeNode.cs
│   ├── BusinessBehaviorNode.cs
│   ├── BusinessRelationNode.cs
│   ├── BusinessNoteNode.cs
│   ├── PendingQuestionNode.cs
│   └── CustomTypeDefinition.cs
├── CommandLog/
│   ├── CommandEnvelope.cs
│   ├── CommandLogStore.cs
│   └── ReplayEngine.cs
├── Patches/
│   ├── PatchEngine.cs
│   ├── IPatchOperation.cs
│   └── Operations/
│       ├── AddEntityOp.cs
│       ├── UpdateEntityOp.cs
│       ├── DeleteEntityOp.cs
│       ├── AddAttributeOp.cs
│       ├── UpdateAttributeOp.cs
│       └── DeleteAttributeOp.cs
├── Identity/
│   └── BusinessIdAllocator.cs
├── Persistence/
│   ├── DocumentSerializer.cs
│   └── DocumentDeserializer.cs
└── Validation/
    ├── DocumentValidator.cs
    └── BusinessRules.cs
```

---

## Složková struktura — Src/MetaForge.Translator/

```
Src/MetaForge.Translator/
├── MetaForge.Translator.csproj
├── Host/
│   ├── BusinessAuthoringHostFacade.cs
│   ├── ProjectionReadService.cs
│   ├── ProjectionView.cs
│   ├── ExpertProjection.cs
│   └── NodeAssistContext.cs
├── Translation/
│   ├── IBusinessTranslator.cs
│   ├── ITranslationService.cs
│   ├── DefaultBusinessTranslator.cs
│   └── WriteBackService.cs
├── Prompting/
│   ├── PromptBuilder.cs
│   └── ContextProjection.cs
└── Telemetry/
    ├── TracingService.cs
    └── MetricsCollector.cs
```

---

## Složková struktura — Src/MetaForge.Generators/

```
Src/MetaForge.Generators/
├── MetaForge.Generators.csproj
├── BaseCodeGenerator.cs
├── TemplateManager.cs
├── CSharp/
│   ├── CSharpGenerator.cs
│   ├── CSharpPackageManifestGenerator.cs
│   └── Templates/
│       ├── class.scriban
│       ├── interface.scriban
│       └── enum.scriban
└── Packaging/
    └── PackageOutputWriter.cs
```

---

## Složková struktura — Src/MetaForge.Ai/

```
Src/MetaForge.Ai/
├── MetaForge.Ai.csproj
├── Abstractions/
│   └── IAiBackendAdapter.cs           # transport (definice i implementace zde)
├── Inference/
│   ├── AiConstraintInferencer.cs       # implementuje IConstraintInferencer z Core
│   └── PromptBuilder.cs                # prompt z MethodElement
├── Translation/
│   ├── AiTranslationService.cs         # implementuje ITranslationService z Translatoru
│   └── PromptBuilder.cs                # prompt z atributu
└── Fallback/
    └── DeterministicFallbackStrategy.cs
```

---

## Složková struktura — Host surfaces

```
Src/MetaForge.Cli/
├── MetaForge.Cli.csproj
├── Program.cs
├── Commands/
│   ├── AddEntityCommand.cs
│   ├── ProjectionCommand.cs
│   ├── TranslateCommand.cs
│   └── ExportCommand.cs
└── Formatting/
    └── CliOutputFormatter.cs

Src/MetaForge.Mcp/
├── MetaForge.Mcp.csproj
├── Program.cs
└── Tools/
    ├── AddEntityTool.cs
    ├── GetProjectionTool.cs
    ├── TranslateTool.cs
    └── ExportTool.cs

Src/MetaForge.WebApi/
├── MetaForge.WebApi.csproj
├── Program.cs
├── Controllers/
│   ├── AuthoringController.cs
│   ├── ProjectionController.cs
│   └── ExportController.cs
├── Dtos/
│   ├── AddEntityRequest.cs
│   ├── AddEntityResponse.cs
│   ├── ProjectionResponse.cs
│   └── ErrorResponse.cs
└── Middleware/
    ├── ExceptionHandlingMiddleware.cs
    └── RequestLoggingMiddleware.cs
```

---

## Složková struktura — Governance a Docs

```
Docs/
├── Architecture/
│   ├── layers.md
│   └── guardrails.md
├── Plans/
│   └── (detailní proposal markdowny)
└── workflow-markdown-first.md

PROPOSALS.md
PROPOSALS_NEXT.md
Progress.md
MeMetaForge.WebApi | MetaForge.Translator |
| mories.md
```

---

## Projekt reference (závislosti)

| Projekt | Závisí na |
|---------|-----------|
| MetaForge.Core | Žádný |
| MetaForge.BusinessModel | Žádný (Core nepotřebuje přímo) |
| MetaForge.Translator | MetaForge.Core, MetaForge.BusinessModel |
| MetaForge.Generators | MetaForge.Core |
| MetaForge.Ai | MetaForge.Translator |
| MetaForge.Cli | MetaForge.Translator |
| MetaForge.Mcp | MetaForge.Translator |
| ForgeBlocks.* | MetaForge.Core |
