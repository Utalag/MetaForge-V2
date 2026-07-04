# Skills a agenti

> Návrh skillů a agentů pro nový projekt. Jeden orchestrátor, specializovaní hidden specialisté.

---

## Principy

1. **Jeden hlavní orchestrátor** — přijímá úkoly, deleguje na specialisty.
2. **Specializovaní hidden specialisté** — každý má úzký scope, jasné vstupy/výstupy.
3. **Governance-first** — žádná implementace bez schváleného návrhu.
4. **Markdown-first** — jeden skill explicitně vynucuje markdown workflow.
5. **Minimum user-facing skillů** — uživatel nemusí vybírat ze 20 skillů.

---

## Orchestrátor

### metaforge-platform-orchestrator

- **Typ:** User-facing (hlavní vstupní bod)
- **Účel:** Vedení vývoje, delegace na specialisty, rozhodování o workflow.
- **Vstupy:** Úkoly od uživatele (implementace, refaktoring, debug, test, docs).
- **Výstupy:** Koordinace specialistů, finální odpověď uživateli.
- **Pravidla:**
  - Vždy nejprve zkontroluje PROPOSALS.md a Memories.md.
  - Deleguje implementaci na Implementation Forger.
  - Deleguje testy na Test Sentinel.
  - Deleguje governance na Governance Clerk.
  - Nikdy neimplementuje přímo pokud existuje vhodný specialista.

---

## Hidden specialisté

### metaforge-implementation-forger

- **Typ:** Hidden specialist
- **Účel:** Psaní kódu — ForgeBlocky, capability balíky, refaktoring, fix.
- **Vstupy:** Schválený návrh z PROPOSALS.md, konkrétní scope, soubory.
- **Výstupy:** Implementovaný kód, záznam do Progress.md.
- **Pravidla:**
  - Nesmí implementovat bez schváleného návrhu.
  - Musí respektovat architektonické guardraily.
  - Musí zapsat do Progress.md po dokončení.

### metaforge-test-sentinel

- **Typ:** Hidden specialist
- **Účel:** Návrh a údržba testů, validace regresí, coverage kontrola.
- **Vstupy:** Změněný kód, požadavek na test coverage.
- **Výstupy:** Nové nebo upravené testy, report o coverage.
- **Pravidla:**
  - Každá změna musí mít odpovídající test.
  - Nemaže existující testy bez silného důvodu.
  - Preferuje unit testy nad integration testy.

### metaforge-governance-clerk

- **Typ:** Hidden specialist
- **Účel:** Správa PROPOSALS.md, Progress.md, Memories.md, workflow disciplíny.
- **Vstupy:** Dokončené tasky, nové návrhy, provozní poznatky.
- **Výstupy:** Aktualizované governance soubory.
- **Pravidla:**
  - Progress.md se aktualizuje po každé dokončené implementaci.
  - Memories.md se aktualizuje po každém nově objeveném guardrail nebo chybě.
  - PROPOSALS.md se aktualizuje při změně stavu návrhu.

### metaforge-markdown-first-guardian

- **Typ:** Hidden specialist (governance)
- **Účel:** Vynucení markdown-first workflow režimu. Kontrola, že návrhy žijí v markdown.
- **Vstupy:** Pull requesty, nové návrhy, implementační rozhodnutí.
- **Výstupy:** Upozornění pokud je porušen markdown-first režim.
- **Pravidla:**
  - Každý nový návrh musí mít markdown soubor v Plans/.
  - PROPOSALS.md musí být aktuální.
  - Žádné implementační rozhodnutí nesmí žít jen v chat historii.

### metaforge-semantic-architect

- **Typ:** Hidden specialist
- **Účel:** Definice abstraktní sémantiky ForgeBlocků, boundary decisions, Core vs Generators rozdělení.
- **Vstupy:** Nový ForgeBlock požadavek, boundary question.
- **Výstupy:** Sémantický návrh, handle, capabilities.
- **Pravidla:**
  - Core je zaměřené na C#-first architekturu, striktní jazyková agnosticita se nevyžaduje.
  - ForgeBlock je capability balík, ne jen codegen plugin.
  - Každý ForgeBlock nese vlastní discovery metadata.

### metaforge-core-guardrails

- **Typ:** Hidden specialist
- **Účel:** Kontrola, že změny v Core neporušují guardraily.
- **Vstupy:** Navržené změny v Core vrstvě.
- **Výstupy:** Verdikt (povoleno / zamítnuto s důvodem).
- **Pravidla:**
  - Core nesmí záviset na vyšších vrstvách.
  - Core může obsahovat C#-specifika, pokud to zjednodušuje typový model — striktní jazyková agnosticita už není vyžadována.
  - Nové abstrakce potřebují silný důvod.

---

## Workflow instrukční skill

### metaforge-markdown-first-workflow (skill soubor)

- **Typ:** Workflow instrukce (skill)
- **Účel:** Explicitně ukotvuje markdown-first režim v novém projektu.
- **Trigger:** Při každém novém návrhu, implementačním rozhodnutí nebo PR review.
- **Obsah skill instrukce:**

```markdown
## Markdown-First Workflow Skill

### Kdy se aktivuje
- Při tvorbě nového návrhu
- Při dokončení implementace
- Při objevení nového guardrail nebo chyby
- Při review kódu

### Pravidla
1. Každý návrh MUSÍ mít detailní markdown v Docs/Plans/.
2. PROPOSALS.md MUSÍ obsahovat odkaz na každý aktivní návrh.
3. Po dokončení implementace MUSÍ být aktualizován Progress.md.
4. Opakované chyby a guardraily MUSÍ být zapsány do Memories.md.
5. PROPOSALS_NEXT.md slouží jako zásobník — nikdy se neimplementuje přímo z něj.
6. Žádné implementační rozhodnutí nesmí žít pouze v chat historii.

### Anti-patterny
- ❌ Implementace bez schváleného návrhu v PROPOSALS.md
- ❌ Plán pouze v hlavě nebo v chatu
- ❌ Progress se nezapisuje
- ❌ Memories.md se neaktualizuje po chybě
- ❌ PROPOSALS.md je zastaralý

### Kontrolní otázky
- Je návrh v PROPOSALS.md?
- Existuje detail v Docs/Plans/?
- Je Progress.md aktuální?
- Jsou nové poznatky v Memories.md?
```

---

## Agent picker strategie

| Situace | Vybraný agent |
|---------|---------------|
| Nový feature request | Orchestrátor → Semantic Architect → Implementation Forger |
| Bug fix | Orchestrátor → Implementation Forger → Test Sentinel |
| Refaktoring | Orchestrátor → Core Guardrails → Implementation Forger |
| Nový ForgeBlock | Orchestrátor → Semantic Architect → Implementation Forger → Test Sentinel |
| Governance check | Orchestrátor → Governance Clerk → Markdown-First Guardian |
| PR review | Orchestrátor → Core Guardrails → Test Sentinel |

---

## Prevence chaosu v agent pickeru

1. **Maximálně 3 user-facing skills** — orchestrátor, případně přímý přístup k Test Sentinel a Governance Clerk.
2. **Ostatní jsou hidden** — uživatel je nevybírá, orchestrátor je deleguje.
3. **Jasná odpovědnost** — žádný overlap mezi specialisty.
4. **Orchestrátor rozhoduje** — nikdy dva specialisté současně na stejném scope.
