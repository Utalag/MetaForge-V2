# IDEA-015 Workflow Projekce a Write-Back

Stav: Idea
Oblast: BusinessModel, Translator, Workflow
Zdroj: For_Inspiration/Architecture-Define/09-Authoring-Kernel-and-Multi-Output-Model.md
Datum vytvoření: 2026-07-08
Poslední revize: 2026-07-08

## 1. Kontext

Původní koncept definuje workflow jako first-class sekci `BusinessAuthoringDocumentu` — sibling k entitám a relacím, ne detail. Současná implementace (PROP-020) má základní `BusinessWorkflowNode` a `BusinessWorkflowStepNode`, ale chybí workflow projekce, write-back a readiness.

Nápad vychází z `09-Authoring-Kernel-and-Multi-Output-Model.md`, kde je workflow popsáno jako:
- Sekce `BusinessAuthoringDocument` s kroky, vazbami, podmínkami, schvalováním, capability bindingy
- `WorkflowProjectionView` jako projekční sekce
- Workflow write-back (binding detail, sync metadata)
- Workflow export jako budoucí premium směr

## 2. Problém dnes

- PROP-020 implementoval workflow uzly, ale chybí:
  - `WorkflowProjectionView` — workflow není vidět v projekci
  - Workflow write-back — capability binding a enrichment se nezapisuje zpět
  - `WorkflowBindingSyncState` — nelze zjistit, zda je workflow binding aktuální
  - Workflow export — neexistuje
- Workflow je "druhořadý" občan — entity a atributy mají projekci, workflow ne.
- AI nevidí workflow kontext při assistování.

## 3. Předběžný směr řešení

- `WorkflowProjectionBuilder` — staví `WorkflowProjectionView` z workflow sekce dokumentu
- `WorkflowProjectionView` — kroky, přechody, bindingy, sync metadata
- Workflow write-back commandy: `bind_capability`, `enrich_workflow_step`, `update_workflow_binding`
- `WorkflowBindingSyncState` — stavový automat (New → Synced → Edited → Conflict)
- Napojení na `ProjectionReadService` a `ProjectionOptions`

Dotčené vrstvy: BusinessModel (rozšíření workflow modelu), Translator (projekce + write-back), Core (capability metadata pro binding).

## 4. Signál hodnoty

- Workflow přestává být "poznámka v dokumentaci" a stává se synchronizovanou součástí modelu.
- AI má přístup k procesnímu kontextu.
- Otevírá cestu k workflow exportu (premium směr dle TentativePlan).
- Uživatel vidí stav bindingů a může je opravit.

## 5. Rizika a nejasnosti

- Workflow binding vyžaduje stabilní capability metadata z Core — nejdřív musí být hotový capability model.
- OQ-xxx: Jak přesně vypadá workflow binding capability? (tool handle → workflow step → parameter mapping)
- OQ-xxx: Má workflow export běžet jako codegen nad stejnou pipeline, nebo samostatně?

## 6. Doporučený další krok

Follow-up k PROP-020 (Fáze 5+). Měl by být plánován po stabilizaci workflow modelu a capability metadat.

Vazby: PROP-020 (BusinessModel upgrade), PROP-029 (ForgeBlock capability), PROP-035 (C#-first — workflow jako first-class)
