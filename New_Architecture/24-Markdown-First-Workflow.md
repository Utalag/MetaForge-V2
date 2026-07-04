# Markdown-First Workflow

> Kompletní popis markdown-first pracovního režimu pro nový projekt.
> Tento režim je tvrdý workflow guardrail, ne volitelná poznámka.

---

## Proč markdown-first

1. **Transparentnost** — každý vidí stav všech návrhů, rozhodnutí a poznatků.
2. **Persistence** — rozhodnutí nepřežívají jen v chat historii, ale v commitnutých souborech.
3. **Verzovatelnost** — git history ukazuje kdy a proč se co rozhodlo.
4. **Onboarding** — nový člověk nebo agent si přečte markdown a má kontext.
5. **Governance** — schvalování, tracking a accountability jsou explicitní.
6. **AI-friendly** — malý model dostane kontext z markdown, ne z nestrukturovaného chatu.
7. **Rollback-friendly** — revert commitu vrátí i governance změnu.

---

## Source of planning truth

| Soubor | Role |
|--------|------|
| `PROPOSALS.md` | Master checklist — co se implementuje a v jakém pořadí |
| `Docs/Plans/*.md` | Detail každého návrhu — full spec, scope, DoD |
| `PROPOSALS_NEXT.md` | Zásobník kandidátů — nápady čekající na schválení |
| `Progress.md` | Co bylo realizováno — chronologický log |
| `Memories.md` | Co jsme se naučili — provozní knowledge |

---

## Vztah mezi dokumenty

```
PROPOSALS_NEXT.md          (nápady, kandidáti)
        │
        │ schválení
        ▼
PROPOSALS.md               (master checklist aktivních návrhů)
        │
        │ detailní rozpracování
        ▼
Docs/Plans/plan-XX.md      (full spec jednoho návrhu)
        │
        │ implementace
        ▼
Progress.md                (záznam o dokončení)
        │
        │ při problému
        ▼
Memories.md                (provozní poznatek)
```

---

## Update discipline

### Kdy aktualizovat PROPOSALS.md
- Nový návrh přibyl → přidat řádek s 📝
- Návrh schválen a implementace začíná → změnit na 🚧
- Implementace dokončena → změnit na ✅
- Návrh zamítnut → přesunout do archivní sekce nebo smazat

### Kdy aktualizovat Docs/Plans/*.md
- Při tvorbě nového návrhu (povinné)
- Při změně scope nebo DoD
- Při dokončení části návrhu (checkboxy)

### Kdy aktualizovat Progress.md
- Po dokončení každého slice nebo tasku
- Po dokončení celého návrhu
- Záznam na začátek souboru (nejnovější nahoře)

### Kdy aktualizovat Memories.md
- Při opakující se chybě
- Při nově objeveném guardrail
- Při dependency nebo tooling problému
- Při workflow lesson learned

### Kdy aktualizovat PROPOSALS_NEXT.md
- Nový nápad → přidat do kandidátů
- Kandidát schválen → přesunout do PROPOSALS.md
- Kandidát zamítnut → přesunout do zamítnutých s důvodem

---

## Anti-patterny (co se nesmí vracet)

| Anti-pattern | Proč je problém |
|-------------|-----------------|
| Implementace bez návrhu | Žádný review, žádný tracking, nekonzistentní směr |
| Plán jen v chatu | Nepřežije session, neonboarduje nového agenta |
| Progress se nezapisuje | Nikdo neví co je hotovo |
| Memories.md zastaralý | Stejné chyby se opakují |
| PROPOSALS.md neaktuální | Chaos v prioritách |
| Detailní návrh přímo v PROPOSALS.md | Soubor se stane nečitelný — detail patří do Plans/ |
| Rozsáhlý update všech governance souborů najednou | Atomické updaty po každém tasku |

---

## Návrh skill/workflow instrukce

Tento soubor by měl být v novém projektu umístěn jako:

```
Docs/workflow-markdown-first.md
```

nebo jako agent skill:

```
.github/agents/metaforge-markdown-first-workflow.md
```

### Obsah instrukce pro agenta

```markdown
# Markdown-First Workflow — Instrukce pro agenta

## Tvrdé pravidlo
Markdown dokumenty jsou primární nosič návrhu, backlogu, governance a implementačního plánu.

## Před implementací
1. Ověř, že návrh existuje v PROPOSALS.md.
2. Ověř, že detail existuje v Docs/Plans/.
3. Ověř, že jsi přečetl Memories.md pro relevantní poznatky.

## Po implementaci
1. Zapiš do Progress.md co bylo implementováno.
2. Aktualizuj PROPOSALS.md (stav na ✅ pokud dokončeno).
3. Pokud jsi narazil na nový guardrail nebo chybu, zapiš do Memories.md.

## Kontrolní otázky
- [ ] Je návrh schválený v PROPOSALS.md?
- [ ] Existuje detailní markdown?
- [ ] Je Progress.md aktuální?
- [ ] Jsou relevantní Memories přečtené?
- [ ] Jsou nové poznatky zapsané?

## Anti-patterny k hlídání
- Implementace bez schváleného návrhu
- Rozhodnutí žijící jen v chat historii
- Neaktuální governance soubory
```

---

## Adopce v novém projektu

Nový projekt MUSÍ:
1. Mít všechny governance soubory (PROPOSALS.md, PROPOSALS_NEXT.md, Progress.md, Memories.md) od prvního commitu.
2. Mít `Docs/workflow-markdown-first.md` jako explicitní workflow instrukci.
3. Mít markdown-first guardian skill v agent konfiguraci.
4. Vynutit review governance souborů při každém PR.
5. Orchestrátor musí kontrolovat aktuálnost governance souborů před delegací implementace.
