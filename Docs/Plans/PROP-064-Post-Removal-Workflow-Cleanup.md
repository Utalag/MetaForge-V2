# PROP-064: Post-Removal Workflow Artifact Cleanup & Verification

**Status:** Na zvážení *(spustit až po dokončení PROP-062 a PROP-063)*
**Owner:** Utalag
**Date:** 2026-07-18
**Depends on:** PROP-063 (Remove Explicit Workflow Modeling)

**Kontextové commity:**
- Před odstraněním: `archive/workflow-last` → `[doplnit]`
- Odstranění: `[doplnit]` — PROP-063 removal commit

---

## 1. Purpose

Po odstranění workflow v PROP-063 provést systematickou kontrolu, zda v kódu, testech, JSON fixtures, snapshot souborech a dokumentaci nezůstaly "duchové" — zapomenuté reference, usingy, importy, nebo zbytková data. Není to implementační plán, ale **kontrolní checklist**.

---

## 2. Kontrolní body — Kód

### K1 — Grep na klíčová slova napříč Src/ a Tests/

```powershell
Get-ChildItem -Recurse -Include *.cs,*.csproj,*.json -Path Src/,Tests/ |
    Select-String -Pattern "BusinessWorkflow|WorkflowStep|WorkflowTransition|WorkflowBinding|AddWorkflowOp|AddWorkflowStep|AddWorkflowTransition|WorkflowBindingSyncState|BusinessWorkflowStepKind|CreateWorkflowId" -CaseSensitive
```

- Očekávaný výsledek: **0 výskytů v .cs, .csproj, .json souborech**
- Výskyty v .md jsou očekávané (anotované historické dokumenty)

### K2 — Grep na using statementy

```powershell
Get-ChildItem -Recurse -Include *.cs | Select-String -Pattern "using.*Workflow" -CaseSensitive
```

- Očekávaný výsledek: **0 výskytů**

### K3 — Grep na "Workflow" v test fixtures / test datech

```powershell
Get-ChildItem -Recurse -Include *.json -Path Tests/ | Select-String -Pattern "Workflow" -CaseSensitive
```

- Ověřit, že žádné testovací JSON dokumenty neobsahují `"Workflows"` sekci
- Pokud ano → odstranit, nebo přidat komentář proč zůstává

### K4 — Grep na "Workflow" v TestResults/

```powershell
Get-ChildItem -Recurse -Include *.trx -Path Tests/ | Select-String -Pattern "Workflow" -CaseSensitive
```

- TRX soubory mohou obsahovat historické názvy testů — lze ignorovat nebo smazat

---

## 3. Kontrolní body — Dokumentace

### K5 — Konzistence New_Architecture dokumentů

- [ ] `New_Architecture/07-BusinessModel.md` — neobsahuje definice workflow modelů ani `Workflows` property
- [ ] `New_Architecture/08-Translator.md` — neobsahuje workflow reference, obsahuje `FlowGraphSection`
- [ ] `New_Architecture/00-Index.md` — popis 07-BusinessModel.md neobsahuje "Workflow", obsahuje "FlowGraph"
- [ ] `New_Architecture/01-Architectural-Guardrails.md` — neobsahuje "Workflow guardraily" placeholder
- [ ] `New_Architecture/27-ForgeBlock-External-Libraries.md` — sekce State Machines / Workflow anotována

### K6 — Konzistence Docs/Plans/

- [ ] `PROP-020` — anotován "Workflow sekce odstraněna v PROP-063"
- [ ] `PROP-044` — anotován "Workflow operace odstraněny v PROP-063"
- [ ] `PROP-053` — workflow page reference odstraněny/upraveny

### K7 — Konzistence Docs/Ideas/

- [ ] `IDEA-015` — anotován "SUPERSEDED by PROP-062"
- [ ] `IDEA-016`, `IDEA-019`, `IDEA-031` — workflow reference anotovány nebo odstraněny

### K8 — Konzistence Docs/Issues/

- [ ] `ISS-013` a `ISS-015` — workflow reference anotovány

### K9 — Konzistence root souborů

- [ ] `Progress.md` — obsahuje záznamy o PROP-062 a PROP-063
- [ ] `PROPOSALS.md` — obsahuje PROP-062 a PROP-063
- [ ] `README.md` — neobsahuje zavádějící workflow reference

---

## 4. Kontrolní body — Build & Testy

### K10 — Clean build

```powershell
dotnet clean && dotnet build
```

- Očekávaný výsledek: 0 errors, 0 warnings souvisejících s workflow

### K11 — Full test run

```powershell
dotnet test
```

- Očekávaný výsledek: všechny testy projdou, včetně PROP-062 FlowGraphSection testů

### K12 — Test na starý JSON dokument (kompatibilita)

- Vzít JSON dokument s `"Workflows"` sekcí (pokud existuje v test fixtures)
- Ověřit, že `BusinessAuthoringDocument` ho deserializuje bez chyby
- Pokud test fixture neexistuje → vytvořit ručně a ověřit

### K13 — Test na starý CommandLog (kompatibilita)

- Vzít CommandLog s `"AddWorkflow"` commandem (pokud existuje)
- Ověřit, že `ReplayEngine.Replay()` ho přeskočí s varováním, nehodí výjimku
- Pokud test fixture neexistuje → vytvořit ručně a ověřit

---

## 5. Výstup

- PROP-064 se uzavře jako **checklist**, ne jako kód
- Výstup: `Progress.md` — "PROP-064: Post-removal workflow artifact check — clean / found N issues (fixed)"
- Nalezené problémy → opravit v rámci PROP-063 nebo follow-up commitem

---

## 6. Decision

- Tento PROP je **na zvážení** — neblokuje PROP-062 ani PROP-063
- Lze ho přeskočit, pokud PROP-063 proběhne čistě
- Doporučuje se spustit jako sanity check, zejména K1, K2, K10, K11
