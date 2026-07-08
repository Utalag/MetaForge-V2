# PROP-044 Translator & BusinessModel — Workflow opravy, SyncState konsolidace, Facade thread safety

Typ výsledku: Candidate Proposal
Zdroj podnětu: AI — Perplexity Deep Research (konverzace e7299554)
Stav životního cyklu: Candidate
Rozhodovací owner:
Poslední revize: 2026-07-08

Priorita: High
Oblast: Translator, BusinessModel
Owner:
Datum vytvoření: 2026-07-08
Aktualizováno: 2026-07-08

Navazuje na:
- PROP-020 (BusinessModel Upgrade)
- Perplexity revize: https://www.perplexity.ai/search/e7299554-47b9-465b-94ef-7c3d1de1e092
- ISS-014 (Workflow bypasses PatchEngine)
- ISS-015 (Duplicate SyncState)

Blokuje:
- —

Související soubory:
- `Src/MetaForge.Translator/Host/BusinessAuthoringHostFacade.cs`
- `Src/MetaForge.Translator/Translation/DefaultBusinessTranslator.cs`
- `Src/MetaForge.Translator/Translation/WriteBackService.cs`
- `Src/MetaForge.Translator/Host/ProjectionReadService.cs`
- `Src/MetaForge.BusinessModel/Patches/Operations/*.cs`
- `Src/MetaForge.BusinessModel/Models/SyncState.cs`
- `Src/MetaForge.BusinessModel/Models/AttributeSyncState.cs`
- `Src/MetaForge.BusinessModel/CommandLog/ReplayEngine.cs`

## 1. Kontext

Perplexity Deep Research identifikoval 5 problémů v Translator a BusinessModel vrstvě:

1. **Workflow operace obcházejí PatchEngine** — AddWorkflow, AddWorkflowStep, AddWorkflowTransition mutují `_document` přímo přes `with`, bez CommandEnvelope
2. **Dvojí SyncState** — AttributeSyncState (enum, aktivně používán) + SyncState (discriminated union, mrtvý kód)
3. **_document jako mutable field** — race condition v BusinessAuthoringHostFacade
4. **ProjectionReadService mrtvé přetížení** — `GetProjection(CommandLogStore)` není nikdy voláno
5. **Žádné testy pro Translator**

## 2. Problém dnes

- Workflow změny nejsou persistentní přes replay — porušuje architektonický invariant
- SyncState discriminated union je 70+ řádků mrtvého kódu
- Facade není thread-safe — problém pro WebApi
- Translator nemá unit testy

## 3. Cíl

- Workflow operace přes PatchEngine (AddWorkflowOp, AddWorkflowStepOp, AddWorkflowTransitionOp)
- ReplayEngine větve pro workflow commandy
- Odstranit SyncState.cs, ponechat AttributeSyncState enum
- Facade thread safety (ImmutableInterlocked)
- Translator unit testy

## 4. Architektonické invarianty

- CommandLog je append-only historie změn — workflow nesmí být výjimka
- PatchEngine je jediná cesta pro mutace dokumentu
- AI je volitelná — Translator funguje i bez AI

## 5. Scope

### In scope
- 3 nové operace: AddWorkflowOp, AddWorkflowStepOp, AddWorkflowTransitionOp
- ReplayEngine větve pro workflow
- BusinessAuthoringHostFacade — přepojení workflow na PatchEngine, ImmutableInterlocked pro _document
- Odstranění SyncState.cs a SyncStateJsonConverter.cs
- Odstranění mrtvého GetProjection(CommandLogStore) přetížení
- Translator unit testy (DefaultBusinessTranslator, WriteBackService, workflow)

### Out of scope
- CoreDetail redesign (fáze 2)
- Workflow redesign (stávající workflow modely zůstávají)

## 6. Návrh řešení

### Workflow operace

```csharp
// AddWorkflowOp — přidá workflow do dokumentu
// AddWorkflowStepOp — přidá krok, validuje existenci workflow
// AddWorkflowTransitionOp — přidá přechod, validuje existenci kroků
```

### Facade změny

```csharp
// private readonly object _documentLock = new();
// Všechny write operace: lock (_documentLock) { _document = ...; }
// ImmutableInterlocked nebo ReaderWriterLockSlim pro read-heavy scénáře
```

### SyncState

- Smazat `SyncState.cs`, `SyncStateJsonConverter.cs`
- `AttributeSyncState.cs` zůstává a je jediná reprezentace
- Smazat testy pro SyncState (pokud existují)

## 7. Implementační fáze

### Fáze 1 — Workflow operace (kritické)
- [ ] AddWorkflowOp
- [ ] AddWorkflowStepOp
- [ ] AddWorkflowTransitionOp
- [ ] ReplayEngine větve
- [ ] Facade přepojení

### Fáze 2 — SyncState konsolidace
- [ ] Smazat SyncState.cs, SyncStateJsonConverter.cs
- [ ] Dokumentovat AttributeSyncState jako jedinou reprezentaci

### Fáze 3 — Facade thread safety
- [ ] Lock nebo ImmutableInterlocked pro _document
- [ ] Odstranit mrtvé GetProjection(CommandLogStore)

### Fáze 4 — Testy
- [ ] Translator tests (DefaultBusinessTranslator.Translate, WriteBackService.ApplyEnrichment)
- [ ] Workflow tests (AddWorkflowOp, AddWorkflowStepOp, AddWorkflowTransitionOp)
- [ ] Facade tests (workflow operace, thread safety)
