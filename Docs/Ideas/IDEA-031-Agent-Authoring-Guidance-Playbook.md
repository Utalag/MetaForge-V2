# IDEA-031 Agent Authoring Guidance — Modeling Playbook + Discovery Loop

Stav: Idea
Oblast: AI, MCP, Docs, Workflow
Zdroj: Koumák + Perplexity konverzace db7f49c1-8f18-4f2e-8da2-b9b6e1f5c122
Datum vytvoření: 2026-07-11

## 1. Kontext

Z diskuze o tom, jak AI agenti pracují s MetaForge, vyplynulo, že chybí "návod jak přemýšlet". Agent má tendenci myslet jako programovací jazyk (třídy, C# syntaxe), ale MetaForge chce sémantický pohled (entity, atributy, vztahy, metadata).

Perplexity: *"Necheš agenta učit 'jak zapsat C#', ale 'jak převést uživatelův záměr do authoring dat, která dávají smysl pro BusinessAuthoringDocument a Facade'."*

Zároveň se diskutoval **agent decision loop** s využitím presetů a capabilities z katalogu (IDEA-030).

## 2. Problém dnes

- AI agent neví, jak přemýšlet v doméně MetaForge — myslí v C# syntaxi, ne v sémantice
- Agent neví o existenci presetů a capabilities — modeluje vše od nuly
- Chybí guidance dokument, který by agentovi vysvětlil mentální model platformy
- Agent nemá jasný rozhodovací loop — kdy použít preset, kdy modelovat ručně
- Bez guidance je agent neefektivní a dělá zbytečnou práci

## 3. Agent Decision Loop

```
┌─────────────────────────────────────────────────────┐
│ 1. INTENT → SÉMANTICKÝ SKETCH                      │
│    Agent odvodí entity, atributy, vztahy            │
│    Ne C# syntaxi, ale business koncepty             │
├─────────────────────────────────────────────────────┤
│ 2. CATALOG DISCOVERY                                │
│    Pro každý koncept: existuje preset/capability?   │
│    Hledá od největší granularity k nejmenší:         │
│    bundle → entity → attribute → capability          │
├─────────────────────────────────────────────────────┤
│ 3. APPLY SHORTCUTS                                  │
│    Použij nalezené presety a capabilities            │
│    Nahraj je do BusinessAuthoringDocument           │
├─────────────────────────────────────────────────────┤
│ 4. MANUAL FILL                                      │
│    Co nenašel v katalogu, domodeluj ručně           │
│    Přes standardní MCP authoring tooly              │
├─────────────────────────────────────────────────────┤
│ 5. PROJECTION CHECK                                 │
│    Stáhni projekci, ověř konzistenci                │
│    Write path = Read path                           │
└─────────────────────────────────────────────────────┘
```

## 4. Modeling Playbook — struktura dokumentu

Soubor: `Docs/AI/Agent-Authoring-Guidance.md`

### 4.1 Mentální model platformy
- MetaForge není textový programovací jazyk, ale authoring systém
- Source of truth = `BusinessAuthoringDocument`
- Core a C# výstup jsou derivace, ne primární cíl
- Mysli v entitách, atributech, vztazích, metadatech — ne v C# třídách a properties

### 4.2 Jak rozkládat záměr
- Uživatelský intent → business entity → atributy → typy → vztahy → metadata
- "Potřebuju email" → ne "string Email", ale atribut s `Validation.Email`
- "Potřebuju auditní stopu" → ne ručně `CreatedAt`, `UpdatedAt`, ale preset `Auditable`

### 4.3 Jak formulovat výstup pro MCP
- Používej MCP tooly: `addentity`, `addattribute`, `getprojection`
- Inkrementální změny, ne jednorázová generace
- Po každé dávce změn ověř projekci

### 4.4 Guardrails (tvrdé věty)
- "Neformuluj řešení jako hotový C# kód, pokud není výslovně chtěn export."
- "Nejprve navrhni business strukturu, teprve potom ji zapisuj."
- "Po větších změnách vždy čti projekci."
- "Preferuj malé inkrementální kroky a slices."
- "Preset je zkratka, ne black-box — po aplikaci se dál normálně upravuje."

### 4.5 End-to-end příklady
Minimálně 3 příklady ve formátu:
| Krok | Co ukázat |
|------|-----------|
| Vstup | Uživatelský intent |
| Sémantický rozpad | Entity, atributy, vztahy |
| MCP kroky | Sekvence volání `addentity`, `addattribute`, `catalog.recommendForIntent` |
| Business view | Stav `BusinessAuthoringDocument` / projekce |
| Výsledný kód | Odpovídající C# (jako derivace, ne jako primární cíl) |

Příklady:
1. **Jednoduchá evidence** — `Vehicle` s atributy
2. **Relace mezi entitami** — `Customer` → `RepairOrder` → `Vehicle`
3. **S enrich/presetem** — použití `Auditable`, `email`, `money`

### 4.6 Anti-patterns
- ❌ "Rovnou generovat C# kód"
- ❌ "Dělat přímé mutace bez Facade"
- ❌ "Míchat metadata a C# attributes bez rozlišení"
- ❌ "Modelovat `CreatedAt`, `UpdatedAt` ručně místo preset `Auditable`"

## 5. Tři vrstvy playbooku

| Vrstva | Soubor | Pro koho | Formát |
|--------|--------|----------|--------|
| **Obecný playbook** | `Docs/AI/Agent-Authoring-Guidance.md` | Všechny agenty, lidské čtení | Markdown |
| **Planning Skill** | `Skills/MetaForge.Modeling.Skill.md` | Agent — rozhodovací heuristika | Markdown + JSON |
| **Planning JSON schema** | `Docs/Schemas/agent-planning.schema.json` | Agent — strojově čitelný scaffold | JSON Schema |
| **Doménové recipes** | `Docs/AI/Domain-Recipes/` | Specifické use cases | Markdown |

### 5.1 Planning JSON scaffold — interní mentální kostra

Skill definuje JSON strukturu, kterou agent používá jako **pracovní desku** (ne jako výstup pro uživatele). Oddělení MCP vs Skill:

| Vrstva | Co dělá | Příklad |
|--------|---------|---------|
| **MCP** | Runtime data a operace | `catalog.listPresets`, `addentity` |
| **Skill** | Rozhodovací heuristika | Jak hledat presety, kdy jít ručně, jak validovat |
| **BusinessAuthoringDocument** | Source of truth výsledku | Stav po aplikaci |

```json
{
  "intent": {
    "domain": "car dealership",
    "goal": "manage vehicles, customers, sales"
  },
  "semanticSketch": {
    "concepts": [
      {
        "name": "Vehicle",
        "kind": "entity",
        "semanticTags": ["inventory", "asset", "car"],
        "keyAttributes": ["Brand", "Model", "Year", "LicensePlate"]
      }
    ]
  },
  "catalogStrategy": "largest-first",
  "conceptPlans": [
    {
      "concept": "Vehicle",
      "presetCandidates": [
        {"id": "domain.vehicle", "score": 0.95, "decision": "use"}
      ],
      "manualGaps": [{"attribute": "Color", "reason": "custom request"}],
      "decision": "apply-preset-then-enrich"
    }
  ],
  "operationPlan": [
    {"step": 1, "action": "catalog.recommendForIntent", "params": {"intent": "Vehicle entity"}},
    {"step": 2, "action": "applyPreset", "params": {"presetId": "domain.vehicle"}},
    {"step": 3, "action": "getprojection", "params": {}}
  ],
  "checkpoints": [
    "getProjection after each entity application",
    "validate syntax of generated C#"
  ],
  "status": "planning | executing | verifying | done",
  "errors": []
}
```

### 5.2 Skill pravidla (explicitní, pro agenta)

1. **Začni od doménových konceptů**, ne od C# syntaxe.
2. **Hledej největší relevantní preset dřív** než menší stavebnice (bundle → entity → attribute → capability).
3. **Co nenajdeš v katalogu, modeluj ručně** přes standardní MCP authoring flow.
4. **Po každé větší změně validuj výsledek přes projekci** (`getprojection`).
5. **Preset je zkratka pro zápis** do business modelu, ne finální neprůstřelná černá skříňka; po aplikaci je možné další upravování.
6. **Host surface nesmí nést business logiku**; skill jen vede agenta, neobchází architekturu.

### 5.3 Versioned rollout

| Verze | Co | Kdy |
|-------|-----|-----|
| V1 | Playbook text + planning JSON scaffold | Hned — i bez MCP catalog discovery |
| V2 | MCP catalog discovery + `catalog.recommendForIntent` | Až bude IDEA-030 hotové |
| V3 | `applyPreset` MCP tool + scoring/prioritizace | Až budou presety v katalogu |

## 6. Strojově čitelný guidance payload

## 7. Signál hodnoty

- **Agent nemusí hádat** — má jasný mentální model a rozhodovací loop
- **Efektivita** — využívá presety a capabilities místo modelování od nuly
- **Konzistence** — všichni agenti používají stejný playbook
- **Méně chyb** — guardrails zabraňují běžným anti-patternům
- **Testovatelnost** — playbook definuje očekávané chování agenta

## 8. Doporučený další krok

**Candidate Proposal** — odhad: 2-3 dny pro:
1. Sepsat `Docs/AI/Agent-Authoring-Guidance.md` (obecný playbook)
2. Vytvořit 3 end-to-end příklady
3. Definovat agent decision loop jako diagram/pseudokód
4. (Volitelně) Vytvořit `agent-guidance.json` pro strojové čtení

Navazuje na: `IDEA-030` (Shared Authoring Contract), `IDEA-029` (ProductionHub)
Závisí na: MCP tool existence, `catalog.recommendForIntent` (z IDEA-030)
