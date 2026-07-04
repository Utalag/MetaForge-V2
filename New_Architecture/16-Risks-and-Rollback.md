# Rizika a rollback

> Identifikace rizik a strategie pro rollback.

---

## Hlavní rizika

### R1 — Ztráta replay kompatibility

- **Popis:** Změna PatchEngine nebo CommandLog formátu bez verzování může rozbít replay dřívějších commandů.
- **Pravděpodobnost:** Střední
- **Dopad:** Vysoký — ztráta historie
- **Mitigace:**
  - CommandEnvelope formát definovat v prvním commitu a neměnit.
  - Zavést versioning do CommandEnvelope (`SchemaVersion`).
  - Napsat regression test: "existující command log se replayuje po změně implementace".
- **Rollback:** Revert na předchozí CommandLog implementaci.

### R2 — Facade coupling

- **Popis:** BusinessAuthoringHostFacade se stane god class s příliš mnoha odpovědnostmi.
- **Pravděpodobnost:** Střední
- **Dopad:** Střední — špatná udržovatelnost
- **Mitigace:**
  - Facade deleguje na specializované services (ProjectionReadService, WriteBackService).
  - Facade je orchestrátor, ne implementátor.
  - Code review guardrail: facade method nesmí mít víc než 20 řádků.
- **Rollback:** Extrahovat novou service z facade.

### R3 — ForgeBlock registrace complexity

- **Popis:** Registrační mechanismus ForgeBlocků se stane příliš komplexní.
- **Pravděpodobnost:** Nízká
- **Dopad:** Nízký — ovlivňuje pouze plugin autory
- **Mitigace:**
  - Minimální povinné API: Handle, Capabilities, Discovery.
  - Jeden register metoda bez overloads.
  - Příklady v Tests/Examples/.
- **Rollback:** Zjednodušit IForgeBlockPackage interface.

### R4 — AI fallback nedostatečný

- **Popis:** Deterministický path bez AI není funkčně ekvivalentní AI path.
- **Pravděpodobnost:** Střední
- **Dopad:** Střední — degradovaný UX bez AI
- **Mitigace:**
  - Deterministický enrichment přes CatalogManager jako baseline.
  - AI pouze doplňuje — nikdy nenahrazuje deterministickou cestu.
  - Test: "celý authoring flow bez AI" musí projít.
- **Rollback:** N/A — je to design decision, ne implementační risk.

### R5 — Governance overhead

- **Popis:** Markdown-first režim přidává overhead a zpomaluje vývoj.
- **Pravděpodobnost:** Nízká
- **Dopad:** Nízký — workflow lze zjednodušit
- **Mitigace:**
  - Governance Clerk agent automatizuje většinu updatů.
  - Skeleton šablony minimalizují manuální práci.
  - Anti-pattern: governance se neaktualizuje = viditelné z PR review.
- **Rollback:** Zjednodušit šablony, snížit povinné sekce.

---

## Rollback strategie

### Per-task rollback

Každý atomický task je navržen jako jeden commit. Rollback = `git revert {commit}`.

**Pravidla:**
- Jeden task = jeden commit.
- Commit message česky, popisuje co bylo přidáno.
- Žádný task nesmí záviset na partial state jiného commitu.

### Per-slice rollback

Slice je sekvence tasků. Rollback celého slice = revert sekvence commitů.

**Pravidla:**
- Slice nesmí mít side-effects mimo svůj scope.
- Po rollback slice musí build procházet.
- Test suite musí procházet i po rollback.

### Per-epic rollback

Epic je celá oblast (Core, BusinessModel, ...). Rollback celého epicu je drastický.

**Pravidla:**
- Rollback epicu se dělá jen při zásadním architektonickém problému.
- Vyžaduje manuální review.
- Může vyžadovat rollback závislých epiců.

---

## Dependency DAG pro rollback

```
Epic 8 (ForgeBlocks) → závisí na Epic 2 (Core)
Epic 7 (Generators) → závisí na Epic 2 (Core)
Epic 5 (Host) → závisí na Epic 4 (Translator)
Epic 6 (AI) → závisí na Epic 4 (Translator)
Epic 4 (Translator) → závisí na Epic 2 (Core) + Epic 3 (BusinessModel)
Epic 3 (BusinessModel) → nezávisí na Core přímo
Epic 2 (Core) → nezávisí na ničem
Epic 1 (Governance) → nezávisí na ničem
```

**Důsledek:** Rollback Epic 2 (Core) vyžaduje rollback všeho nad ním. Proto Core musí být extra stabilní a extra dobře otestované.

---

## Checkpoint strategie

Po dokončení každého epicu:
1. Taggovat v gitu: `checkpoint/epic-{N}-done`
2. Zapsat do Progress.md
3. Ověřit build + test suite
4. Review governance souborů

Tím vznikají bezpečné rollback body.
