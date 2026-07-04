# Epiky a implementační řezy (Slices)

> Rozpad práce na epiky a slices. Každý epic obsahuje logicky uzavřenou oblast práce.
> Slices jsou menší implementační řezy, které lze zadávat postupně.

---

## Epic 1 — Governance a project scaffold

**Cíl:** Založit nový projekt s markdown-first governance od prvního dne.

| Slice | Popis | Závislosti |
|-------|-------|------------|
| 1.1 | Vytvoření solution a kořenové governance soubory | Žádné |
| 1.2 | Markdown-first workflow instrukční soubor | 1.1 |
| 1.3 | Skill definice pro markdown-first režim | 1.2 |
| 1.4 | README.md s quick start a architektonickým přehledem | 1.1 |

---

## Epic 2 — Core vrstva

**Cíl:** Vytvořit Core typový model.

| Slice | Popis | Závislosti |
|-------|-------|------------|
| 2.1 | Založení projektu MetaForge.Core + základní abstrakce | 1.1 |
| 2.2 | DataTypes — základní typový systém | 2.1 |
| 2.3 | Elements — Class, Property, Method, Enum (simplified) | 2.2 |
| 2.4 | Catalog — CatalogManager, presety, ValueObjects, providers (built-in, filesystem, marketplace, ForgeBlockRegistry) | 2.3 |
| 2.5 | ForgeBlockPackages — registrační infrastruktura + capability/discovery kontrakty | 2.4 |
| 2.6 | Discovery — tag-based discovery metadata | 2.5 |
| 2.7 | Validation — IValidatable, validační pipeline | 2.3 |
| 2.8 | Expressions — computed properties/behaviors (Expression, ComputedExpression, Statement, renderer registry, primitives) | 2.3 |
| 2.9 | Inference — constraint a boundary inference | 2.2, 2.1 |
| 2.10 | StandardLibraries — mapování sémantických operací na standardní knihovnu | 2.1 |
| 2.11 | ValueObjects — StrongType, ConversionOptions, validation rules | 2.2 |

---

## Epic 3 — BusinessModel vrstva

**Cíl:** Vytvořit BusinessAuthoringDocument, CommandLog a replay mechanismus.

| Slice | Popis | Závislosti |
|-------|-------|------------|
| 3.1 | Založení projektu MetaForge.BusinessModel | 1.1 |
| 3.2 | BusinessAuthoringDocument — entity, atributy, chování, relace | 3.1 |
| 3.3 | CommandLog — CommandEnvelope, append-only store | 3.2 |
| 3.4 | Replay — rekonstrukce stavu z commandů | 3.3 |
| 3.5 | PatchEngine — atomické mutace dokumentu | 3.2 |
| 3.6 | Persistence — JSON serializace/deserializace | 3.2 |
| 3.7 | Validation — business pravidla dokumentu | 3.5 |
| 3.8 | Identity — BusinessIdAllocator | 3.2 |
| 3.9 | CustomTypes — registr, auto-registrace | 3.5, 2.4 |

---

## Epic 4 — Translator vrstva

**Cíl:** Vytvořit facade, projekci, enrichment a write-back.

| Slice | Popis | Závislosti |
|-------|-------|------------|
| 4.1 | Založení projektu MetaForge.Translator | 1.1 |
| 4.2 | BusinessAuthoringHostFacade — hlavní orchestrace | 4.1, 3.5 |
| 4.3 | ProjectionReadService — replay + read path | 4.2, 3.4 |
| 4.4 | DefaultBusinessTranslator — business → Core překlad | 4.1, 2.4 |
| 4.5 | Write-back — enrichment zpět do BusinessModel | 4.4, 3.5 |
| 4.6 | ExpertProjection — diagnostika, suggestions | 4.3, 4.4 |
| 4.7 | Telemetrie — trace, metriky | 4.2 |

---

## Epic 5 — Host surfaces

**Cíl:** Tenké CLI a MCP host surfaces.

| Slice | Popis | Závislosti |
|-------|-------|------------|
| 5.1 | Založení projektu MetaForge.Cli | 1.1 |
| 5.2 | CLI commands — basic authoring flow | 5.1, 4.2 |
| 5.3 | Založení projektu MetaForge.Mcp | 1.1 |
| 5.4 | MCP tools — authoring tools pro AI klienta | 5.3, 4.2 |
| 5.5 | CLI chat integrace (volitelné) | 5.2, 6.2 |

---

## Epic 6 — AI integrace

**Cíl:** Volitelná AI vrstva s graceful fallback.

| Slice | Popis | Závislosti |
|-------|-------|------------|
| 6.1 | Založení projektu MetaForge.Ai | 1.1 |
| 6.2 | IAiTranslator abstrakce + provider registrace | 6.1 |
| 6.3 | Prompt building — kontext z projekce | 6.2, 4.3 |
| 6.4 | AiTranslationService — enrichment přes AI | 6.3, 4.5 |
| 6.5 | Graceful fallback — deterministický path bez AI | 6.2 |

---

## Epic 7 — Generators (C#-first)

**Cíl:** CSharpGenerator jako jediný aktivní generátor.

| Slice | Popis | Závislosti |
|-------|-------|------------|
| 7.1 | Založení projektu MetaForge.Generators | 1.1 |
| 7.2 | BaseCodeGenerator abstrakce | 7.1, 2.3 |
| 7.3 | CSharpGenerator — základní generace | 7.2 |
| 7.4 | Template engine — Scriban nebo string-based | 7.3 |
| 7.5 | Package manifest generator | 7.3 |

---

## Epic 8 — ForgeBlock balíky

**Cíl:** Capability balíky pro standardní operace.

| Slice | Popis | Závislosti |
|-------|-------|------------|
| 8.1 | ForgeBlock.Math | 2.5 |
| 8.2 | ForgeBlock.String | 2.5 |
| 8.3 | ForgeBlock.Validation | 2.5, 3.7 |
| 8.4 | ForgeBlock.Random | 2.5 |
| 8.5 | ForgeBlock.DateTime | 2.5 |

---

## Epic 9 — Testovací infrastruktura

**Cíl:** Průběžně s každou vrstvou — testy jako first-class citizen.

| Slice | Popis | Závislosti |
|-------|-------|------------|
| 9.1 | Test helpers, factory builders | 2.1, 3.1 |
| 9.2 | Core.Tests — typový model, katalog | 2.7 |
| 9.3 | BusinessModel.Tests — replay, patches, validation | 3.7 |
| 9.4 | Translator.Tests — facade, projekce, write-back | 4.7 |
| 9.5 | Generators.Tests — C# output verification | 7.5 |

---

## DAG závislostí (zjednodušený)

```
Epic 1 (Governance)
    ├── Epic 2 (Core)
    │       ├── Epic 7 (Generators)
    │       └── Epic 8 (ForgeBlocks)
    ├── Epic 3 (BusinessModel)
    │       └── Epic 4 (Translator)
    │               ├── Epic 5 (Host Surfaces)
    │               └── Epic 6 (AI)
    └── Epic 9 (Testy) — paralelně s každým epicem
```
