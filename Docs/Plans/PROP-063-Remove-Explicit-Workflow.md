# PROP-063: Remove Explicit Workflow Modeling

**Status:** Proposed
**Owner:** Utalag
**Date:** 2026-07-18
**Layer:** BusinessModel, Translator
**Priority:** 🟡 Vysoká
**Estimate:** 1–2 dny
**Depends on:** PROP-062 (FlowGraphSection)
**Related commit/tag (před odstraněním):** `archive/workflow-last`
**Related docs:** 07-BusinessModel.md, 08-Translator.md, ReadMe-Architecture-Summary.md, Progress.md

---

## 1. Context

Workflow je v aktuální architektuře explicitní součást `BusinessAuthoringDocument` (PROP-020, 2026-07-04). Obsahuje 6 modelových typů, 3 patch operace, 3 facade metody a ReplayEngine dispatch. Současně pro něj:
- Neexistuje využitý authoring scénář
- Není napojení na doménový model (žádné CLI/MCP commandy, žádná projekce)
- Není propojení s reálným use-casem
- Náhrada: `FlowGraphSection` (PROP-062) poskytuje odvozenou flow vizualizaci z entit a relací bez nutnosti explicitního workflow modelování

## 2. Decision

Odstranit explicitní workflow modeling z write modelu platformy. Nahrazeno `FlowGraphSection` v `DocumentProjection` (PROP-062). Workflow nebude dále authorováno jako first-class business koncept.

## 3. Why

- Chybí business use-case pro workflow authoring — workflow je implementované, ale mrtvé
- Chybí napojení workflow na doménový model — 0 CLI/MCP commandů, 0 projekcí
- Flow pohled lze odvodit z entit a relací (`FlowGraphSection`) bez samostatného modelu
- Sníží se komplexita write modelu (6 typů, 3 operace, 3 facade metody, ReplayEngine dispatch, 194 řádek testů)
- Udržuje se architektonický princip: source of truth v dokumentu, read path přes projekce

---

## 4. Current Implementation Map

### BusinessModel — modely (6 souborů k ODSTRANĚNÍ)
- `Src/MetaForge.BusinessModel/Models/BusinessWorkflowNode.cs`
- `Src/MetaForge.BusinessModel/Models/BusinessWorkflowStepNode.cs`
- `Src/MetaForge.BusinessModel/Models/BusinessWorkflowTransitionNode.cs`
- `Src/MetaForge.BusinessModel/Models/BusinessWorkflowStepKind.cs`
- `Src/MetaForge.BusinessModel/Models/BusinessWorkflowStepBindingDetail.cs`
- `Src/MetaForge.BusinessModel/Models/WorkflowBindingSyncState.cs`

### BusinessModel — document (MODIFIKACE)
- `Src/MetaForge.BusinessModel/Models/BusinessAuthoringDocument.cs` — odstranit `Workflows` property
- `Src/MetaForge.BusinessModel/Identity/BusinessIdAllocator.cs` — odstranit `CreateWorkflowId()`

### BusinessModel — patch operace (3 soubory k ODSTRANĚNÍ)
- `Src/MetaForge.BusinessModel/Patches/Operations/AddWorkflowOp.cs`
- `Src/MetaForge.BusinessModel/Patches/Operations/AddWorkflowStepOp.cs`
- `Src/MetaForge.BusinessModel/Patches/Operations/AddWorkflowTransitionOp.cs`

### BusinessModel — ReplayEngine (MODIFIKACE)
- `Src/MetaForge.BusinessModel/CommandLog/ReplayEngine.cs` — odstranit 3 case větve z dispatche + 3 `Apply*()` metody, přidat `default` skip pro neznámé commandy

### Translator — facade (MODIFIKACE)
- `Src/MetaForge.Translator/Host/BusinessAuthoringHostFacade.cs` — odstranit `AddWorkflow()`, `AddWorkflowStep()`, `AddWorkflowTransition()`

### Translator — projekce (BEZ ZMĚNY)
- `ProjectionReadService` a `ProjectionBuilder` workflow nikdy neobsahovaly

### Testy (1 soubor k ODSTRANĚNÍ)
- `Tests/MetaForge.Translator.Tests/Translation/WorkflowOperationTests.cs`

### Dokumentace — New_Architecture (MODIFIKACE)
- `New_Architecture/07-BusinessModel.md` — odstranit definice workflow modelů, `Workflows` property, `CreateWorkflowId()`
- `New_Architecture/08-Translator.md` — odstranit/upravit workflow reference ve facade sekci
- `New_Architecture/00-Index.md` — upravit popis 07-BusinessModel.md
- `New_Architecture/01-Architectural-Guardrails.md` — odstranit placeholder "Workflow guardraily"
- `New_Architecture/27-ForgeBlock-External-Libraries.md` — anotovat sekci State Machines / Workflow

### Dokumentace — Docs (ANOTACE, ne mazání)
- `Docs/Ideas/IDEA-015-Workflow-Projection-WriteBack.md` — "SUPERSEDED by PROP-062"
- `Docs/Plans/Implemented/PROP-020-BusinessModel-Architecture-Upgrade.md` — "Workflow sekce odstraněna v PROP-063"
- `Docs/Plans/Implemented/PROP-044-Translator-BusinessModel-Fixes.md` — "Workflow operace odstraněny v PROP-063"
- `Docs/Plans/PROP-053-Web-Frontend-Blazor.md` — upravit/odstranit workflow page reference
- `Docs/Issues/ISS-013_PROP-022_Diff-Modify-not-detected.md` — anotovat workflow reference
- `Docs/Issues/ISS-015_PROP-044_Duplicate-SyncState.md` — anotovat `WorkflowBindingSyncState`

### Dokumentace — Root (MODIFIKACE)
- `Progress.md` — přidat záznam
- `PROPOSALS.md` — přidat PROP-063

---

## 5. Dependency Impact

- **JSON deserializace:** `System.Text.Json` ignoruje neznámé properties → staré dokumenty se načtou bez migrace
- **ReplayEngine:** `default` větev přeskočí staré `"AddWorkflow"` commandy s varováním → není nutná migrace
- **Schema version:** Není nutný bump — absence `Workflows` pole není breaking change
- **Facade API:** Metody mizí, ale nikdy nebyly volané z host surface → žádný breaking change
- **Testy:** `WorkflowOperationTests.cs` se maže celý. Ostatní testy nezasaženy.
- **Dokumentace:** ~12 souborů vyžaduje úpravu

---

## 6. Removal Plan

### Fáze 0: Commity a značení
1. ✅ PROP-062 (FlowGraphSection) je implementován a otestován — commitnut
2. Vytvořit tag `archive/workflow-last` na aktuálním HEAD **před jakýmkoliv mazáním**
3. Poznamenat commit hash: `[doplnit]`

### Fáze 1: Odstranění BusinessModel vrstvy
4. Smazat 6 modelových souborů
5. Upravit `BusinessAuthoringDocument.cs` — odstranit `Workflows` property
6. Upravit `BusinessIdAllocator.cs` — odstranit `CreateWorkflowId()`
7. Smazat 3 patch operace
8. Upravit `ReplayEngine.cs` — odstranit workflow case + apply metody, přidat `default` skip

### Fáze 2: Odstranění Translator vrstvy
9. Upravit `BusinessAuthoringHostFacade.cs` — odstranit 3 workflow metody

### Fáze 3: Testy a dokumentace
10. Smazat `WorkflowOperationTests.cs`
11. Upravit ~12 dokumentačních souborů

### Fáze 4: Commit a záznam
12. `dotnet build` + `dotnet test` — ověřit
13. Commit: `PROP-063: Remove explicit workflow modeling (replaced by FlowGraphSection PROP-062)`
14. Zaznamenat commit hash: `[doplnit]`
15. Zaznamenat commit hash do PROP-064

---

## 7. Rollback

- **Tag:** `archive/workflow-last` → `[doplnit]`
- **Obnova:** `git checkout archive/workflow-last -- [soubory]` nebo cherry-pick
- Tento dokument obsahuje kompletní mapu všech dotčených míst

---

## 8. Risks

| Risk | Úroveň | Mitigation |
|------|--------|------------|
| Workflow nebyl exponován do host surfaces | Nízká | 0 CLI/MCP usage |
| JSON deserializace starých dokumentů | Nízká | System.Text.Json ignoruje neznámé properties |
| Workflow testy izolované | Nízká | Jeden soubor, žádné závislosti |
| Zapomenuté reference v dokumentaci | Střední | Explicitní mapa všech souborů výše |

---

## 9. Non-goals

- Nenavrhuje se nové BPMN nebo process engine řešení
- Nenahrazuje se workflow query engine (path finding) — future vrstva nad FlowGraphSection
- Neřeší se execution engine workflow
- Nemažou se historické dokumenty — pouze se anotují

---

## 10. Final Traceability

- **Tag před odstraněním:** `archive/workflow-last` → `[doplnit]`
- **Commit odstranění:** `[doplnit]` — `PROP-063: Remove explicit workflow modeling (replaced by FlowGraphSection PROP-062)`
- **Související:** PROP-062 (FlowGraphSection), PROP-064 (post-removal check)
