# Závislosti

## Externí projektové reference

```mermaid
flowchart LR
    subgraph MetaForge.Translator
        TRANSLATOR
    end

    subgraph Dependencies
        AI["MetaForge.Ai\nAI runtime adaptery\n(IAiRuntimeAdapter, AiPlatformConfiguration)"]
        BM["MetaForge.BusinessModel\nAuthoring dokumenty, patche,\nvalidace, serializace"]
        CORE["MetaForge.Core\nKatalog, DataTypes, Discovery,\nForgeBlock registry"]
        DTO["MetaForge.Dto\nTransport DTO (MetaForgeTransportDto)\na Core→Dto→Core konverze"]
        GEN["MetaForge.Generators\nForgeBlock package bootstap,\nbuilt-in provider registrace"]
    end

    TRANSLATOR --> AI
    TRANSLATOR --> BM
    TRANSLATOR --> CORE
    TRANSLATOR --> DTO
    TRANSLATOR --> GEN
```

### MetaForge.Ai
- `AiPlatformConfiguration` – config pro AI segmenty (Conversation, AuthoringTranslation)
- `IAiRuntimeAdapter` / `HttpAiRuntimeAdapter` – HTTP klient pro AI inference
- `IAiRuntimeAdapterFactory` – továrna na runtime adaptéry

### MetaForge.BusinessModel
- `BusinessAuthoringDocument` – vstupní model pro překlad
- `BusinessEntityNode`, `BusinessAttributeNode`, `BusinessBehaviorNode`, `BusinessRelationNode` – elementy dokumentu
- `BusinessPatchOperation`, `BusinessPatchEngine` – patch aplikace
- `BusinessDocumentStore` – persistence dokumentu
- `BusinessTreeRenderer` – textová reprezentace stromu
- `BusinessDocumentJsonSerializer` – JSON serializace
- `BusinessDocumentValidator` – validace
- `BusinessProjectionView`, `IProjectionQueryService`, `IShadowCommandStore` – projection/shadow log
- `BusinessPatchToCommandMapper` – mapování patchů na command log
- `BusinessAuthoringDocument` factory, `BusinessNoteNode`, `PendingQuestionNode`, `WorkflowBindingSyncState`

### MetaForge.Core
- `CatalogManager` – type resolution, preset lookup, catalog queries
- `ProgramLanguage` – výstupní jazyk (CSharp, Python, ...)
- `DataType` – datové typy (Guid, String, Int, Custom, ...)
- `EntityKind` – Primitive, Class, Enum
- `SemanticCollection` – None, List, Set
- `TypeResolution` / `TypeResolutionExtensions` – výsledek resolve typu
- `IDiscoverySession`, `DiscoveryQuery`, `DiscoveryQueryResult` – discovery API
- `ForgeBlockPackageRegistry` – registr ForgeBlock balíčků
- `IForgeBlockPackage`, `ForgeBlockAttribute` – ForgeBlock metadata
- `BuiltInCatalogProvider`, `ForgeBlockRegistryCatalogProvider` – catalog providery
- `ValueObjectPreset`, `ValidationRulePreset` – preset definice

### MetaForge.Dto
- `MetaForgeTransportDto` – výstupní DTO překladu
- `TransportClassDto`, `TransportPropertyDto`, `TransportMethodDto` – elementy DTO
- `TransportStrongTypeDto`, `TransportTypeModelDto` – typový systém DTO
- `MetaProject`, `Class`, `Property`, `Method` – Core model pro generátory
- `ToCore()` / `ToDto()` – konverze mezi DTO a Core

### MetaForge.Generators
- `BuiltInForgeBlockPackageBootstrap` – registrace built-in ForgeBlock balíčků

## Testovací projekty

```mermaid
flowchart LR
    SUBJ["MetaForge.Translator"]
    TRANSTESTS["MetaForge.Translator.Tests\n(DefaultBusinessTranslator, AiEnvelopeExtraction)"]
    BMTESTS["MetaForge.BusinessModel.Tests\n(AuthoringConversationService, BusinessAuthoringHostFacade,\nNodeAssistFacade, FacadeSlices, ...)"]
    CORETESTS["MetaForge.Core.Tests\n(StandardLibraryTranslatorRegistry)"]

    TRANSTESTS --> SUBJ
    BMTESTS --> SUBJ
    BMTESTS --> BM
    CORETESTS --> CORE
```

## Modely (DTO) napříč vrstvami

| Vstupní model | → | Výstupní model | Kde |
|---------------|---|----------------|-----|
| `BusinessAuthoringDocument` | → | `MetaForgeTransportDto` | `DefaultBusinessTranslator` |
| `ConversationPromptRequest` | → | `ConversationAiResult` | `IAuthoringConversationAiClient` |
| `AuthoringPromptRequest` | → | `AuthoringResponseEnvelope` | `IAuthoringAiClient` |
| `BusinessAuthoringDocument` | → | `ProjectionView` | `ProjectionReadService` |
| `NodeAssistRequest` | → | `NodeAssistProposal` | `BusinessAuthoringHostFacade` |
| User message string | → | `ConversationTurnResult` | `AuthoringConversationService` |
| `BusinessPatchOperation[]` | → | `ConversationTurnResult` | `CommandWriteService` |

## Příklad: Úplný call stack pro "pridej entitu Order" s AI

```mermaid
sequenceDiagram
    box "Tenký host" 
        participant CLI as MetaForge.Cli
    end
    box "Translator - Facade"
        participant BAHF as BusinessAuthoringHostFacade
        participant DBT as DefaultBusinessTranslator
        participant PRS as ProjectionReadService
    end
    box "Translator - Conversation"
        participant ACS as AuthoringConversationService
        participant CWS as CommandWriteService
        participant ACPB as AuthoringCommandProvenanceBuilder
    end
    box "Translator - AI Adapters"
        participant ACA as AuthoringConversationAiClientAdapter
        participant ATA as AuthoringAiClientAdapter
    end
    box "MetaForge.Ai"
        participant ATS as AiTranslationService
        participant RTA as HttpAiRuntimeAdapter
    end
    box "MetaForge.BusinessModel"
        participant BM as BusinessPatchEngine
        participant DS as BusinessDocumentStore
    end

    CLI->>BAHF: ProcessMessageAsync("pridej entitu Order")
    BAHF->>ACS: ProcessMessageAsync(msg)
    
    Note over ACS: Conversation AI path
    
    ACS->>ACA: CompleteConversationAsync(request)
    ACA->>ATS: CompletePromptAsync(system, user)
    ATS->>RTA: CompleteAsync(request)
    RTA-->>ATS: AI response
    ATS-->>ACA: raw text
    ACA->>ACA: AiJsonEnvelopeExtractor.ExtractJsonPayload()
    ACA->>ACA: JsonSerializer.Deserialize<ConversationAiResult>()
    ACA-->>ACS: ConversationAiResult (with Brief)

    Note over ACS: Auto-translate

    ACS->>ATA: CompleteAuthoringAsync(request with brief)
    ATA->>ATS: CompletePromptAsync(system, user)
    ATS->>RTA: CompleteAsync(request)
    RTA-->>ATS: AI response
    ATS-->>ATA: raw text
    ATA->>ATA: AiJsonEnvelopeExtractor.ExtractJsonPayload()
    ATA->>ATA: AuthoringAiEnvelopeRepair.TryDeserialize()
    ATA-->>ACS: AuthoringResponseEnvelope (Mode=Apply, patches)

    Note over ACS: Apply patches

    ACS->>CWS: ApplyPatches(patches)
    CWS->>BM: Apply(document, patches)
    BM-->>CWS: ApplyResult (Success)
    CWS->>ACPB: CreateAiTranslatedCommandContext()
    CWS->>CWS: TryAppendShadowCommands()
    CWS->>DS: Save(document)
    DS-->>CWS: saved document
    CWS-->>ACS: WriteApplyResult
    
    ACS-->>BAHF: ConversationTurnResult (Success)
    BAHF-->>CLI: result
```
