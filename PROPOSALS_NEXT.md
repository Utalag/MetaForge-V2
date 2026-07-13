# PROPOSALS_NEXT — Zásobník kandidátních návrhů

> Návrhy, které jsou identifikované, ale zatím neschválené k implementaci.
> Nikdy neimplementovat přímo z tohoto souboru — vždy přesunout do PROPOSALS.md.
> Plány jsou uloženy v `Docs/Plans/PROP-XXX-*.md` a jsou součástí návrhu.
> Poslední aktualizace: 2026-07-12

## Kandidátní návrhy

| ID | Název | Vrstva | Priorita | Odhad | Poznámka |
|----|-------|--------|----------|-------|----------|
| **CODE-001** | CLI — `generate`/`export` command | Host Surfaces | 🔴 Kritická | 2-3 dny | Blokátor B2: pipeline BusinessModel→Core→C# není v CLI propojená. Nutno přidat command, který propojí Translator + CodeGenerator. |
| **CODE-002** | Perzistence v CLI/MCP — JsonCommandLogRepository + JsonDocumentRepository | Infrastructure | 🔴 Kritická | 1-2 dny | Blokátor B3: obě host surfaces používají in-memory storage. Data přežijí jen do restartu. |
| **CODE-003** | Monetizace — IGenerationCostPolicy, tier licence, billing gate | Generators, Monetization | 🟡 Vysoká | 3-5 dní | Blokátor B4: chybí jakákoli implementace monetizace. Nutno před produkcí. |
| **PROP-053** | Web Frontend — Blazor Server s MudBlazor (strom modelu, konfigurace, ForgeBlock výběr) | Frontend | ⚪ Na zvážení | ~5 dní | Návrh: `Docs/Plans/PROP-053-Web-Frontend-Blazor.md` |

## Odložené návrhy

| ID | Název | Důvod odložení | Datum |
|----|-------|-----------------|-------|
| PROP-018 | Translator — ExpertProjection a ProjectionOptions | Nejasná funkcionalita pro host surfaces — potřeba upřesnit scope | 4.7.2026 |
| PROP-020-F5 | BusinessModel — Fáze 5: BusinessBehaviorInputNode, PendingQuestion rozšíření | Nízká priorita, neblokuje core flow; PROP-020 Fáze 1–4 dokončeny | 4.7.2026 |

## Issues — Stav k 2026-07-12

> Všechny issues vyřešeny. 15 přesunuto do `Docs/Issues/Solved-Issues/`.
> Jediná zbývající: ISS-007 (OllamaAiTranslator duplicita) — **By design** (PROP-019 Varianta A).

| # | Stav | Popis |
|---|------|-------|
| ISS-001..ISS-016 | ✅ Vyřešeno | 15 issues přesunuto do `Docs/Issues/Solved-Issues/` |
| ISS-007 | ℹ️ By design | Duplicita OllamaAiTranslator — PROP-019 Varianta A, řešit po stabilizaci AI API |

### PROP-027: AI Layer — MetaForge.Ai projekt, OllamaAdapter, PromptRegistry

Více: [`Docs/Plans/PROP-027-AI-Layer-MetaForge-Ai.md`](Docs/Plans/PROP-027-AI-Layer-MetaForge-Ai.md)

### PROP-028: Infrastructure — Persistence, Konfigurace, Caching

Více: [`Docs/Plans/PROP-028-Infrastructure-Persistence-Config-Cache.md`](Docs/Plans/PROP-028-Infrastructure-Persistence-Config-Cache.md)

### PROP-029: ForgeBlocks — Rozšíření a marketplace

Více: [`Docs/Plans/PROP-029-ForgeBlocks-Expansion-Marketplace.md`](Docs/Plans/PROP-029-ForgeBlocks-Expansion-Marketplace.md)

### PROP-030: Bezpečnost a stabilita — Schema Migration, Validation, Health

Více: [`Docs/Plans/PROP-030-Security-Stability-Schema-Validation-Health.md`](Docs/Plans/PROP-030-Security-Stability-Schema-Validation-Health.md)

### PROP-031: Core — Statement System a upgrade Expression pro těla metod

Více: [`Docs/Plans/PROP-031-Core-Statement-System.md`](Docs/Plans/PROP-031-Core-Statement-System.md)

### PROP-032: Integrační testy — Core + Generators (Snapshot-based)

Více: [`Docs/Plans/PROP-032-Integration-Tests-Core-Generators.md`](Docs/Plans/PROP-032-Integration-Tests-Core-Generators.md)

> Testovací matice: [`Docs/Integration/01-Integration-Test-Matrix.md`](Docs/Integration/01-Integration-Test-Matrix.md)

### PROP-034: Core Reference Documentation + Support Matrix

Více: [`Docs/Plans/PROP-034-Core-Reference-Documentation.md`](Docs/Plans/PROP-034-Core-Reference-Documentation.md)

> **Vychází z:** IDEA-001, IDEA-002, IDEA-003 (Perplexity analýza d773bf6a)
> **Obsahuje:** Docs/Core/ sada (Overview, Support Matrix, Type System, Type Kinds, Value Objects, Methods, Expressions, Roundtrip, Examples)
> **OQ-034-01:** Jak udržovat support matici živou? (ručně vs generovaně)
> **OQ-034-02:** Je roundtrip cílový stav nebo jen užitečná vlastnost?
> **OQ-034-03:** Má se matice verzovat s Core?

### IDEA-004 Follow-up: Method Expression Boundary

> **Follow-up k:** PROP-031 (Core Statement System)
> **Popis:** Doplnit `MethodBodyKind` (None/Structured/Text/AiBody) do `MethodElement` a vymezit hranici "co je AST a co už ne".
> **OQ:** Jaký je default `MethodBodyKind` pro nově parsované metody?
> **Zdroj:** IDEA-004 z `Docs/ideas/old_ideas/`

### PROP-035: C#-First Core Migration

Více: [`Docs/Plans/PROP-035-CSharp-First-Core-Migration.md`](Docs/Plans/PROP-035-CSharp-First-Core-Migration.md)

> **Vychází z:** Perplexity konverzace 05663298 (5 dotazů)
> **Obsahuje:** 7 commitů migrace (RootElement → ClassElement → MethodElement → Expressions → Translator → Tests → Docs)
> **Princip:** C#-first sémantický Core, ne syntaktický. Red lines: nikdy raw kód, formatting, prompt texty, pricing flagy.
> **OQ-035-01:** Kdy udělat breaking release?
> **OQ-035-02:** Převést BaseClassName na TypeModel??
> **OQ-035-03:** Přesný tvar LanguageCapabilityProfile?

### PROP-036: Core Specification Layer

Více: [`Docs/Plans/PROP-036-Core-Specification-Layer.md`](Docs/Plans/PROP-036-Core-Specification-Layer.md)

> **Vychází z:** Perplexity konverzace 05663298 (5 dotazů)
> **Obsahuje:** InvariantDefinition, InvariantExpression boolean AST, IInvariantEvaluator se Local/Scoped/Relational/Global scope, test generation z invariantů.
> **Princip:** Jeden zdroj pravdy pro invarianty → runtime validace + test generation + AI guardraily.
> **OQ-036-01:** GeneratorIntent na StrongType nebo odvodit z invariantů?
> **OQ-036-02:** Granularita AI-generated invariantů?
> **OQ-036-03:** IInvariantEvaluator v Core nebo Infrastructure?

### PROP-037: C# Completeness — Chybějící konstrukty + Projektová metadata + Roslyn Importer

Více: [`Docs/Plans/PROP-037-CSharp-Completeness-Importer.md`](Docs/Plans/PROP-037-CSharp-Completeness-Importer.md)

> **Vychází z:** GitHub task "Implement the plan" kroky 4, 5, 6
> **Obsahuje:** DelegateElement, EventElement, OperatorElement; rozšířený ProjectElement (TargetFramework, PackageReference); MetaForge.Core.Framework (DI, ASP.NET, EF); MetaForge.Importer (Roslyn-based, 2 režimy).
> **Celkem:** 47-69h, 6-9 dní
> **OQ-037-01:** MetaForge.Core.Framework jako samostatný projekt?
> **OQ-037-02:** Granularita framework metadata?
> **OQ-037-03:** Importer s přímou Roslyn závislostí nebo abstrakcí?

### PROP-038: Core DX & Diagnostics — Fluent Builder, DiagnosticBag, AttributeModel, XmlDocModel

Více: [`Docs/Plans/PROP-038-Core-DX-Diagnostics.md`](Docs/Plans/PROP-038-Core-DX-Diagnostics.md)

> **Vychází z:** Perplexity konverzace e0609fe1
> **Obsahuje:** Fluent Builder API (Class("X").WithProperty(...).Build()); DiagnosticBag s Console/JSON/InMemory reportéry; AttributeElement jako first-class; XmlDocElement strukturovaně.
> **Celkem:** 8-13h, 1-2 dny. Additivní změny.
> **OQ-038-01:** Attributes jako breaking change, nebo vedle starých?
> **OQ-038-02:** Extension methods nebo vnořené třídy pro buildery?

### PROP-039: Core Composability — TransformPipeline, Mixin/Trait, ConventionRegistry

Více: [`Docs/Plans/PROP-039-Core-Composability.md`](Docs/Plans/PROP-039-Core-Composability.md)

> **Vychází z:** Perplexity konverzace e0609fe1
> **Obsahuje:** TransformPipeline (middleware nad immutable modelem); Mixin/Trait (build-time expanze); ConventionRegistry (PascalCase, I-prefix, Async suffix).
> **Celkem:** 9-15h, 1-2 dny.
> **OQ-039-01:** TransformPipeline v Core nebo samostatný projekt?
> **OQ-039-02:** Mixiny + atributy + XML doc?
> **OQ-039-03:** Per-element override konvencí?

---

## Legenda priorit

- 🔴 Kritická — musí se implementovat co nejdříve
- 🟡 Vysoká — důležité pro další vývoj
- 🟢 Nízká — nice to have
- ⚪ Odloženo — zatím se neimplementuje
