# Komponentní mapa

## Adresářová struktura

```
MetaForge.Translator/
├── AiTranslationService.cs              # AI runtime adapter (IAiTranslator)
├── DefaultBusinessTranslator.cs         # Deterministický překlad (IBusinessTranslator)
├── IAiTranslator.cs                     # Rozhraní pro AI runtime
├── IBusinessTranslator.cs               # Rozhraní pro deterministický překlad
├── IForgeBlockTranslatorPackage.cs      # SIROTCÍ — nyní nepoužito
│
├── Configuration/
│   └── AuthoringConversationConfiguration.cs    # Konfigurační model (Persistence, Prompting, ShadowLog, Enrichment)
│
├── Conversation/
│   ├── AuthoringAiClientAdapter.cs              # IAiTranslator → IAuthoringAiClient + IPromptCompletionAiClient
│   ├── AuthoringAiClientFactory.cs              # Továrna na AI klienty z AiPlatformConfiguration
│   ├── AuthoringAiHealthProbe.cs                # Health check pro AI segmenty
│   ├── AuthoringCommandProvenanceBuilder.cs     # Build command provenence pro shadow log
│   ├── AuthoringConversationAiClientAdapter.cs  # IAiTranslator → IAuthoringConversationAiClient
│   ├── AuthoringConversationService.cs          # Conversation engine (two-phase AI pipeline)
│   ├── CommandWriteService.cs                   # Write path: patche → shadow log → persistence
│   ├── ConversationTurnResult.cs                # DTO: výsledek jednoho conversation turnu
│   ├── NodeAssistOperationValidation.cs         # ScopeValidator + OperationValidator pro node assist
│   ├── NodeAssistProposal.cs                    # DTO: node-level AI proposal
│   ├── NodeAssistResult.cs                      # DTO: AI node-assist výsledek
│   └── NodeAssistService.cs                     # Orchestrátor node-level AI asistence
│
├── Host/
│   ├── AuthoringContextProjection.cs            # Authoring context projekce (workflow, questions, discovery summary)
│   ├── BusinessAuthoringHostFacade.cs           # Hlavní veřejné API (CRUD, export, projection, node-assist, discovery)
│   ├── DiscoveryTextRenderer.cs                 # Formátování discovery results do textu
│   ├── ExpertProjection.cs                      # Expert projekce + renderer
│   ├── NodeAssistContext.cs                     # Kontext pro node-level AI
│   ├── NodeAssistContextBuilder.cs              # Builder kontextu z ProjectionView + NodePath
│   ├── NodeAssistRequest.cs                     # Vstupní kontrakt pro node assist
│   ├── NodePath.cs                              # Adresace node (entita + atribut/behavior)
│   ├── ProjectionOptions.cs                     # Konfigurace projekčních sekcí
│   ├── ProjectionReadService.cs                 # Orchestrátor projekce (replay + expert + workflow + context)
│   ├── ProjectionView.cs                        # Unifikovaný výsledek projekce
│   └── WorkflowProjection.cs                    # Workflow projekce + builder
│
├── Prompting/
│   ├── AiJsonEnvelopeExtractor.cs               # Extrakce JSON z AI odpovědi (odstranění think bloků, code fences)
│   ├── AuthoringAiEnvelopeRepair.cs             # Oprava JSON chyb, normalizace OP názvů, alias mapping
│   ├── AuthoringPromptRequest.cs                # Request DTO pro translation AI
│   ├── AuthoringResponseEnvelope.cs             # Response envelope (Mode, AssistantMessage, Patches)
│   ├── ConversationAiResult.cs                  # Výsledek conversation AI
│   ├── ConversationPromptRequest.cs             # Request DTO pro conversation AI
│   ├── IAuthoringAiClient.cs                    # Rozhraní pro translation AI klienta
│   ├── IAuthoringConversationAiClient.cs        # Rozhraní pro conversation AI klienta
│   ├── IPromptCompletionAiClient.cs             # Rozhraní pro generic prompt completion
│   ├── SemanticBriefJson.cs                     # Struktura SemanticBrief + converter
│   └── ModelPrompts/
│       ├── AuthoringConversationModelPrompt.cs  # Prompt pro conversation AI
│       ├── AuthoringTranslationModelPrompt.cs   # Prompt pro translation AI
│       └── NodeAssistModelPrompt.cs             # Prompt pro node-level AI
│
├── Telemetry/
│   ├── MetaForgeTelemetry.cs                    # Centralní Meter + Countery + Histogramy
│   ├── TelemetryHelper.cs                       # Pomocné metody (duration měření, result tag)
│   └── TelemetryTags.cs                         # Konstanty tag names/values
│
├── Trace/
│   ├── ExecutionTraceRecorderExtensions.cs       # Extension metody (TraceComponentScope)
│   ├── IExecutionTraceRecorder.cs               # Rozhraní pro structured execution tracing
│   └── OtelExecutionTraceRecorder.cs            # OpenTelemetry implementace
│
└── Properties/
    └── AssemblyInfo.cs                          # InternalsVisibleTo pro test projekty
```

## Třídy podle vrstvy

### Vrstva: Facade / Host API (vstupní bod pro tenké hosty)

| Třída | Viditelnost | Závislosti |
|-------|-------------|------------|
| `BusinessAuthoringHostFacade` | `public sealed` | `AuthoringConversationService`, `CatalogManager`, `IBusinessTranslator`, `ForgeBlockPackageRegistry`, `IDiscoverySession`, `NodePresetSuggester`, `IExecutionTraceRecorder` |
| `ProjectionReadService` | `public sealed` | `IProjectionQueryService`, `CatalogManager`, `IDiscoverySession` |

### Vrstva: Conversation Engine (authoring konverzace + write path)

| Třída | Viditelnost | Závislosti |
|-------|-------------|------------|
| `AuthoringConversationService` | `public sealed` | `CommandWriteService`, `IAuthoringConversationAiClient`, `IAuthoringAiClient`, `AuthoringConversationConfiguration`, `IDiscoverySession` |
| `CommandWriteService` | `internal sealed` | `BusinessPatchEngine`, `BusinessDocumentStore`, `IProjectionQueryService`, `IShadowCommandStore`, `BusinessPatchToCommandMapper` |
| `NodeAssistService` | `public sealed` | `IPromptCompletionAiClient` |
| `AuthoringCommandProvenanceBuilder` | `internal sealed` | - |

### Vrstva: Překlad (deterministic + AI)

| Třída | Rozhraní | Popis |
|-------|----------|-------|
| `DefaultBusinessTranslator` | `IBusinessTranslator` | Deterministický: BusinessModel → MetaForgeTransportDto |
| `AiTranslationService` | `IAiTranslator` | AI runtime: volá `IAiRuntimeAdapter.CompleteAsync` |
| `AuthoringAiClientAdapter` | `IAuthoringAiClient`, `IPromptCompletionAiClient` | Adapter: `IAiTranslator` → nová rozhraní |
| `AuthoringConversationAiClientAdapter` | `IAuthoringConversationAiClient` | Adapter: `IAiTranslator` → conversation rozhraní |
| `AuthoringAiClientFactory` | `internal static` | Továrna: vytváří AI klienty z `AiPlatformConfiguration` |

### Vrstva: Prompting (AI prompt management)

| Třída | Popis |
|-------|-------|
| `AiJsonEnvelopeExtractor` | Odstranění think bloků, code fences, extrakce prvního JSON bloku |
| `AuthoringAiEnvelopeRepair` | Oprava JSON chyb, normalizace OP názvů (add → add_entity, atd.) |
| `AuthoringConversationModelPrompt` | Prompt builder pro conversation AI |
| `AuthoringTranslationModelPrompt` | Prompt builder pro translation AI |
| `NodeAssistModelPrompt` | Prompt builder pro node-level AI |

### Vrstva: Projekce (read-only view na dokument)

| Třída | Popis |
|-------|-------|
| `ExpertProjectionBuilder` | Builder expert projekce (type resolution, preset suggestions, diagnostika) |
| `WorkflowProjectionBuilder` | Builder workflow projekce |
| `AuthoringContextBuilder` | Builder authoring context (workflow summary, questions, discovery) |
| `DiscoveryTextRenderer` | Formátování discovery results |

### Vrstva: Telemetrie a Tracing

| Třída | Popis |
|-------|-------|
| `MetaForgeTelemetry` | OpenTelemetry Meter, ActivitySource, Countery, Histogramy |
| `TelemetryHelper` | TelemetryTimer, ResolveResultTag |
| `OtelExecutionTraceRecorder` | Structured execution tracing přes OpenTelemetry |
| `IExecutionTraceRecorder` | Rozhraní pro trace recorder |

## DTO / Modely

| Třída | Použití |
|-------|---------|
| `AuthoringResponseEnvelope` | Výstup translation AI (mode, message, patches) |
| `ConversationAiResult` | Výstup conversation AI (message, brief, warnings) |
| `ConversationTurnResult` | Výstup `ProcessMessageAsync` (tree, JSON, applied operations) |
| `SemanticBriefJson` | Bridge mezi conversation AI a translation AI |
| `AuthoringPromptRequest` | Vstup pro translation AI |
| `ConversationPromptRequest` | Vstup pro conversation AI |
| `AuthoringConversationConfiguration` | Konfigurace z `metaforge.authoring.json` |
| `ProjectionView` | Unifikovaný výsledek projekce |
| `NodeAssistContext` | Kontext pro node-level AI |
| `NodeAssistProposal` | Výstup node assist (preview bez apply) |
| `NodeAssistResult` | Strukturovaný AI návrh pro node |
| `NodePath` | Adresace node (entita + node) |
