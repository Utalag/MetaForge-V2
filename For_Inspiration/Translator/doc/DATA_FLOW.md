# Datové toky

## 1. Hlavní pipeline: User message → Document mutation

```
User message
    │
    ▼
BusinessAuthoringHostFacade.ProcessMessageAsync()
    │
    ▼
AuthoringConversationService.ProcessMessageAsync()
    │
    ├── TryHandleAuthoringCommandAsync()      ← translate, accept, reject
    │       │
    │       └── TryHandleProposalCommand()
    │           └── FinalizeEnvelope() → ApplyEnvelopeChanges()
    │
    └── [Conversation AI available?]
            │
            ├── YES → ProcessConversationTurnAsync()
            │           │
            │           ├── TryCompleteConversationAsync()
            │           │       → ConversationAiResult (with SemanticBrief or text)
            │           │
            │           ├── [Brief exists?]
            │           │   ├── YES → ShouldAutoTranslate()?
            │           │   │           ├── YES → TryCompleteTranslationEnvelopeAsync(brief)
            │           │   │           │           → FinalizeEnvelope() → ApplyEnvelopeChanges()
            │           │   │           └── NO  → CreatePendingBriefResult() (čeká na "translate")
            │           │   └── NO  → CreateResult() (pure text answer, no model change)
            │           │
            │           └── [Conversation AI failed?]
            │               └── TryBuildDeterministicEnvelope() (text matching)
            │
            └── NO → TryBuildDeterministicEnvelope()
                        ├── match → ApplyEnvelopeChanges()
                        └── no match → error result
```

## 2. Write path: Patche → Persistence

```
ApplyEnvelopeChanges(envelope)
    │
    ├── [envelope.Patches empty?] → error result
    │
    ▼
CommandWriteService.ApplyPatches(patches)
    │
    ├── BusinessPatchEngine.Apply(document, patches)
    │       ├── Success? → applyResult.Document
    │       └── Failure? → WriteApplyResult { Success = false, Issues }
    │
    ├── [Shadow log enabled?]
    │   ├── YES → TryAppendShadowCommands(patches)
    │   │           ├── BusinessPatchToCommandMapper.Map(patch, context)
    │   │           ├── JsonlShadowCommandStore.Append(envelope)
    │   │           ├── All success? → continue
    │   │           └── Failure? → WriteApplyResult { ShadowLogFailure }
    │   └── NO  → continue
    │
    └── PersistIfEnabled(applyResult.Document)
        ├── [Persistence.Enabled?] → BusinessDocumentStore.Save()
        └── NO → return document as-is
```

## 3. Two-phase AI pipeline (detail)

```
┌─────────────────────────────────────────────────┐
│ FÁZE 1: Conversation AI                         │
│ Vstup: ConversationPromptRequest                │
│   ├── userMessage                               │
│   ├── document                                  │
│   ├── currentTree                               │
│   ├── pendingBrief (pokud existuje)             │
│   └── authoringContext (workflow, questions)    │
│                                                 │
│ Prompt: AuthoringConversationModelPrompt        │
│ Odpověď: ConversationAiResult                   │
│   ├── assistantMessage (text zpráva)            │
│   ├── warnings / questions                      │
│   └── brief: SemanticBriefJson                  │
│         ├── translationIntent (state+reason)    │
│         ├── semanticChanges (co se změnilo)     │
│         ├── openQuestions (blokující dotazy)    │
│         └── translationHints                    │
└──────────────┬──────────────────────────────────┘
               │ brief exists?
               │
               ▼
┌─────────────────────────────────────────────────┐
│ FÁZE 2: Translation AI                          │
│ Vstup: AuthoringPromptRequest                   │
│   ├── userMessage                               │
│   ├── document                                  │
│   ├── semanticBrief (z fáze 1)                  │
│   ├── autoApplyModeApply                        │
│   └── requireConfirmationForPropose             │
│                                                 │
│ Prompt: AuthoringTranslationModelPrompt         │
│ Odpověď: AuthoringResponseEnvelope              │
│   ├── mode: Answer | Ask | Propose | Apply     │
│   ├── assistantMessage                          │
│   ├── warnings / questions                      │
│   └── patches: BusinessPatchOperation[]         │
│         ├── op: add_entity, update_attribute... │
│         ├── entityId / attributeId / behaviorId │
│         └── data: { key: value }                │
└──────────────┬──────────────────────────────────┘
               │
               ▼
        ApplyEnvelopeChanges()
```

## 4. Node Assist pipeline

```
BusinessAuthoringHostFacade.AssistNodeAsync(request)
    │
    ├── ProjectionReadService.GetProjectionAsync(options=NodeAssist)
    │   ├── IProjectionQueryService (replay z shadow logu)
    │   ├── ExpertProjectionBuilder (type resolution, presets)
    │   └── AuthoringContextBuilder (workflow, questions, discovery)
    │
    ├── NodeAssistContextBuilder.Build(projection, path)
    │   ├── [expert projection null?] → return null
    │   ├── [entity not found?] → return null
    │   ├── [specific attribute/behavior?] → scoped context
    │   └── [just entity?] → entity-level context
    │
    └── NodeAssistService.AssistAsync(context, prompt)
        ├── NodeAssistModelPrompt.BuildSystemPrompt()
        ├── NodeAssistModelPrompt.BuildUserPrompt(context, prompt)
        ├── IPromptCompletionAiClient.CompletePromptAsync()
        ├── AiJsonEnvelopeExtractor.ExtractJsonPayload()
        ├── JsonSerializer.Deserialize<NodeAssistResult>()
        └── NodeAssistOperationScopeValidator.Sanitize() → filtrování out-of-scope operací
```

## 5. Deterministický překlad: Document → DTO

```
DefaultBusinessTranslator.Translate(document, language)
    │
    ├── BusinessDocumentValidator.Validate(document)
    │   └── [blocking errors?] → throw InvalidOperationException
    │
    ├── Pro každou entitu:
    │   ├── CreatePrimitiveProperty("Id", Guid)  ← vždy
    │   ├── Pro každý atribut:
    │   │   └── CreateAttributeProperty(attribute, language)
    │   │       ├── CatalogManager.ResolveType(attribute.Type)
    │   │       ├── [IsPrimitive] → DataType z katalogu
    │   │       ├── [IsStrongType] → CatalogItem.DisplayName
    │   │       ├── [CustomType] → DataType.Custom + customTypeName
    │   │       └── [Fallback] → DataType.String
    │   ├── Pro každou relaci:
    │   │   └── CreateNavigationProperty(relation, ...)
    │   │       ├── BelongsTo & isSource → FK property (Guid)
    │   │       ├── HasMany & !isSource → Collection property
    │   │       ├── HasOne & !isSource → Navigation property
    │   │       └── ManyToMany → Collection property (obě strany)
    │   └── Pro každý behavior:
    │       └── CreateBehaviorMethod()
    │           ├── ResolveReturnType → void / DataType
    │           └── BuildBehaviorDocumentation → summary + notes
    │
    └── Vrací: MetaForgeTransportDto
```
