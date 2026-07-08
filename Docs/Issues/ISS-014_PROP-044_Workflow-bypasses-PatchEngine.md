# ISS-014 Workflow operace obcházejí PatchEngine

Datum: 2026-07-08
PROP: PROP-020, PROP-044
Soubor: `Src/MetaForge.Translator/Host/BusinessAuthoringHostFacade.cs`
Závažnost: 🔴 Kritická
Stav: Open
Owner:
Poslední revize: 2026-07-08

## 1. Kontext

Issue zjištěno při Perplexity Deep Research konverzace e7299554 (Translator & BusinessModel revize).

## 2. Popis problému

Metody `AddWorkflow`, `AddWorkflowStep`, `AddWorkflowTransition` v `BusinessAuthoringHostFacade` mutují `_document` přímo přes `with` expression — obcházejí `PatchEngine` úplně. Důsledky:

1. Žádný `CommandEnvelope` se nenapíše do `CommandLogStore`
2. Workflow změny jsou nereplikovatelné z `CommandLog`
3. Po `Replay()` na logu vznikne dokument **bez workflows**
4. To je v přímém rozporu s architektonickým invariantem: "CommandLog je append-only historie změn"

## 3. Dopad

- Workflow změny nejsou persistentní přes replay
- CommandLog není kompletní historie změn
- Narušuje architektonický invariant append-only CommandLog

## 4. Doporučené řešení

1. Vytvořit nové operace v `Patches/Operations/`:
   - `AddWorkflowOp` — přidá workflow do dokumentu
   - `AddWorkflowStepOp` — přidá krok do workflow
   - `AddWorkflowTransitionOp` — přidá přechod mezi kroky
2. Každá operace implementuje `IPatchOperation<BusinessAuthoringDocument>`
3. Přidat větve do `ReplayEngine.ApplyCommand` switche pro `"AddWorkflow"`, `"AddWorkflowStep"`, `"AddWorkflowTransition"`
4. Upravit `BusinessAuthoringHostFacade.AddWorkflow*` metody aby používaly `_patchEngine.Apply()`

## 5. Otevřené otázky

- Žádné.

## 6. Rozhodnutí

(čeká na user/owner)

---

## Související

- Vazby: PROP-020, PROP-044
- Blokuje: —
