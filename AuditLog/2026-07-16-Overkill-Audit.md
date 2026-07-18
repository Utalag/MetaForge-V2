# 2026-07-16 Overkill Audit — Analýza hotových PROP/CODE

> Datum: 2026-07-16
> Zdroj: Hloubková analýza po Perplexity konverzaci e2801d78
> Rozsah: 7 kandidátních PROP (022, 036, 037, 039, 041, 046, 051)
> Metoda: Čtení detailních plánů + verifikace proti kódu v Src/
> **Aktualizace 2026-07-18:** Přidána sekce 6 — PROP-063 jako pozitivní příklad odstranění overkill infrastruktury.

---

## 1. Souhrn

| PROP | Skóre | Verdikt | Klíčový důvod |
|------|:-----:|---------|---------------|
| PROP-041 Constructor/Field | 1/5 | ✅ **WORTH IT** | 2 třídy, rovnou zapojeno do CodeGenerator, zaplňuje reálnou díru |
| PROP-051 Support Matrix YAML | 1/5 | ✅ **WORTH IT** | 73-položkový YAML, nízká cena, strojově čitelný pro AI agenty |
| PROP-037 C# Completeness | 3/5 | ✅ **WORTH IT (částečně)** | Delegate/Event/Operator elementy se používají; Roslyn Importer nikdy nevznikl |
| PROP-036 Specification Layer | 3/5 | ⚠️ **NEUTRÁLNÍ** | Shelfware dnes, ale PROP-057 na něm staví — osud závisí na PROP-057 |
| PROP-046 AI Benchmarking | 3/5 | ❌ **OVERKILL** | Test harness postaven, benchmark nikdy neproběhl |
| PROP-022 Observability (OTel) | 2/5 | ❌ **OVERKILL** | ActivitySource stuby bez jediného `StartActivity()` v celém kódu |
| PROP-039 Composability | 4/5 | ❌ **OVERKILL** | 8 typů, nic nezapojeno; jediný přeživší: ElementFingerprint |

---

## 2. Detailní rozbor

### PROP-022 — Observability (OpenTelemetry) — ❌ OVERKILL

**Co bylo dodáno:**
- `BusinessModelActivitySource.cs` — statický `ActivitySource` s názvem `"MetaForge.BusinessModel"`
- `TranslatorActivitySource.cs` — statický `ActivitySource` s názvem `"MetaForge.Translator"`
- `BusinessDocumentDiffer.cs` — `BusinessDocumentDiff` record, `Diff()` metody

**Co NEBYLO dodáno:**
- Žádné `TelemetryExtensions.cs` (DI registrace)
- Žádné OTLP/Jaeger exportéry
- **KRITICKÉ: Ani jedno `ActivitySource.StartActivity()` v celém `Src/` — nula.** Mrtvá lešenářská infrastruktura.

**Proč je to overkill:** Implementovat telemetrické lešení předtím, než existují konzumenti. Mělo být součástí CLI stabilizace (CODE-001/002), ne samostatný PROP.

**Závisí na tom něco?** Ne. Žádný aktivní PROP.

---

### PROP-036 — Core Specification Layer — ⚠️ NEUTRÁLNÍ

**Co bylo dodáno:**
- `InvariantDefinition` — record s `Code`, `TargetKind`, boolean AST (`When`/`Must`)
- `InvariantExpression` — 7 typů AST (PropertyRef, Constant, Eq, Not, And, Or, Exists)
- `IInvariantEvaluator` + `ReflectionBasedInvariantEvaluator`
- `BuiltInInvariants` — 12 invariantů (MF_METHOD_001-005, MF_CLASS_001-003 atd.)

**Co NEBYLO dodáno:**
- `CompiledInvariant<T>` pro výkon
- FsCheck integrace
- AI guardrails

**Proč je to neutrální:** `IInvariantEvaluator.Evaluate()` není NIKDY voláno v produkční pipeline — `CoreValidator` ho nepoužívá. ALE: PROP-057 (ElementContract) explicitně staví na `InvariantDefinition`. Osud = závisí na PROP-057.

**Závisí na tom něco?** PROP-057 (ElementContract), PROP-059 (Healing).

---

### PROP-037 — C# Completeness — ✅ WORTH IT (částečně)

**Co bylo dodáno:**
- `DelegateElement` — s factory metodami, type parameters, constraints
- `EventElement` — s EventType, IsStatic, AddAccessor/RemoveAccessor
- `OperatorElement` + `OperatorKind` enum (25 operátorů)
- Všechny 3 integrovány do `CodeGenerator` (`GenerateDelegate`, `GenerateEvent`, `GenerateOperator`)
- Snapshot testy

**Co NEBYLO dodáno:**
- `MetaForge.Importer` projekt (Roslyn C# → Core) — nikdy nevznikl
- `MetaForge.Core.Framework` namespace
- Round-trip testy

**Proč je to worth it:** Elementy se aktivně používají v generátoru. Bez nich nelze generovat knihovny s delegáty, eventy, operátory. Samotný PROP byl přescopován (Roslyn Importer = 18-26h odhad, nikdy nevznikl), ale dodaná podmnožina je správná.

**Závisí na tom něco?** PROP-052 (follow-up snapshot testy).

---

### PROP-039 — Core Composability — ❌ OVERKILL (nejhorší)

**Co bylo dodáno:**
- `ElementMixin` — record s `ConflictStrategy`, `Mixins.Auditable`, `Mixins.SoftDelete`
- `ConventionRegistry` — s `IConvention`, `ConventionScope`
- `ElementFingerprint` — SHA256 hash, `IEquatable`, `Compute()`, `Empty`
- `IModelTransform` + `TransformPipeline` — řetězitelná pipeline

**Co NEBYLO dodáno / zapojeno:**
- `ApplyMixinTransform` — nikdy neimplementován
- `PascalCasePropertiesConvention`, `InterfacePrefixConvention`, `AsyncSuffixConvention` — nikdy neimplementovány
- `Element.Fingerprint` property — NIKDY nepřidána do `Element` base class
- `ConventionRegistry` — nikdy neinstanciován v produkčním kódu
- `TransformPipeline` — nikdy neinstanciován v produkčním kódu

**Proč je to overkill:** 8 nových typů pro kompozici, která nikdy nebyla zapojena do build pipeline. Jediná přeživší část: `ElementFingerprint` (potřebuje ho PROP-057). Zbytek je mrtvá váha.

**Závisí na tom něco?** PROP-057 (ElementFingerprint).

**Doporučení:** `ElementFingerprint` zachovat. Zbytek zvážit odstranění z Core, pokud se do 3 měsíců nenajde consumer.

---

### PROP-041 — ConstructorElement + FieldElement — ✅ WORTH IT (zlatý standard)

**Co bylo dodáno:**
- `ConstructorElement` — s Parameters, AccessModifier, Body (BlockStatement), Initializer, factory metodami
- `FieldElement` — s Type, AccessModifier, IsReadOnly, IsStatic, DefaultValue, factory metodami
- Oba implementují `IMemberElement`
- Integrovány do `ClassElement.Constructors` / `.Fields` i `StructElement`
- `CodeGenerator` renderuje oba (`RenderConstructor`, `RenderField`)

**Proč je to worth it:** 2 jednoduché třídy. Žádné abstrakce, žádné frameworky, žádné pipelines. Rovnou zapojeno do generátoru. Zaplňuje reálnou díru: bez konstruktorů a fieldů generátor neumí DI-friendly třídy. Tohle je vzor, jak má PROP vypadat.

**Závisí na tom něco?** EndToEndScenariosTests, PROP-040 (Member Consistency).

---

### PROP-046 — AI Model Benchmarking — ❌ OVERKILL

**Co bylo dodáno:**
- `CoreElementComparer` — statická třída s `AreStructurallyEquivalent()` a `Diff()`
- `AiModelBenchmarkTests` — 4 unit testy na comparer samotný
- `ModelsToTest` array: `["gemma3:12b", "llama3.2:3b", "phi3:mini", "mistral:7b"]`

**Co NEBYLO dodáno:**
- Žádné `Benchmark/Prompts/` — nula prompt souborů
- Žádné `Benchmark/References/` — nula referenčních výstupů
- Žádné skutečné benchmark testy — žádné `[Theory]` testy volající Ollama modely
- Žádná matice modelů
- **Původní cíl "který model použít jako výchozí" nebyl nikdy zodpovězen**

**Proč je to overkill:** Test harness postaven, experiment nikdy neproveden. `CoreElementComparer` je užitečný, ale nepotřeboval celý PROP — stačilo 50 řádků v test helperu. Benchmark jako koncept je dobrý nápad, ale měl být 1-denní spike, ne 4-fázový PROP.

**Závisí na tom něco?** Ne.

---

### PROP-051 — Support Matrix YAML — ✅ WORTH IT

**Co bylo dodáno:**
- `Docs/Core/00-Support-Matrix.yaml` — 73 položek, 5 kategorií (type_kinds, members, expressions, statements, other)
- 4 contract statusy: `public-supported`, `advanced`, `internal`, `experimental`
- Verzováno (`1.0`), strojově čitelný YAML

**Proč je to worth it:** Je to dokumentace, ne kód. Nízká cena (~1.5 dne ruční transkripce). Pragmatický YAML formát — žádná automatická generace Markdownu ani naopak (dobrá zdrženlivost). AI agenti můžou programově dotazovat support status. PROP-057 explicitně odlišuje svůj "sémantický kontrakt" od PROP-051 "API stability kontraktu".

**Závisí na tom něco?** PROP-057 (konceptuální odlišení).

---

## 3. Vzorec: Co dělá PROP overkill vs worth it

| 🔴 Overkill pattern | 🟢 Worth it pattern |
|---------------------|---------------------|
| Staví **infrastrukturu bez konzumenta** (ActivitySource bez StartActivity, TransformPipeline bez volání) | Staví **konkrétní artefakt** rovnou zapojený do pipeline (ConstructorElement → CodeGenerator) |
| Vytváří **framework** (8 typů pro kompozici) místo **feature** (2 typy pro konstruktory) | Řeší **skutečnou díru** (bez FieldElement nelze generovat `private readonly`) |
| Testuje **sám sebe** (4 unit testy na comparer, ne reálný scénář) | Testuje **E2E integraci** (snapshot testy s vygenerovaným C#) |
| Plánuje **4 fáze**, dodá jen první (benchmark harness bez benchmarku) | Dodá **kompletní funkční celek** v jedné fázi |
| Vytváří **abstrakce před potřebou** (`IConvention`, `IModelTransform`) | Přidává **konkrétní elementy**, které chybí (`FieldElement`) |

---

## 4. Doporučení

| PROP | Akce |
|------|------|
| **PROP-022** | ActivitySource stuby **nechat být** (nízká režie), ale **nerozšiřovat** dokud není consumer |
| **PROP-039** | `ElementFingerprint` **zachovat** (PROP-057). `ElementMixin`, `ConventionRegistry`, `TransformPipeline` — zvážit **odstranění** z Core do 3 měsíců bez consumera |
| **PROP-046** | `CoreElementComparer` **přesunout** do test helperu. Benchmark koncept — realizovat jako **1-denní spike**, ne PROP |
| **PROP-036** | Osud **závisí na PROP-057**. Pokud PROP-057 vyjde → `InvariantDefinition` se stane užitečným. Pokud ne → shelfware k odstranění |
| **PROP-037** | Roslyn Importer fáze — **formálně dropnout** (nikdy nevznikne). Hotové elementy zachovat |
| **PROP-041** | Vzor pro budoucí PROP — **takto se to má dělat** |
| **PROP-051** | **Udržovat** YAML při změnách Core API. Nízká režie, vysoká hodnota |

---

## 5. Lekce pro PROP-057/058/059

1. **Neinfrastruktura bez konzumenta** — každý nový interface musí mít min. 1 reálné volání v pipeline
2. **Ne 4 fáze bez Fáze 1** — pokud Fáze 1 nedodá kompletní funkční celek, zbytek je riziko
3. **ElementContract** (PROP-057) — riziko: stane se další "abstrakcí bez consumera". Mitigace: povinně dodat s min. 1 reálným použitím (např. `EntityContract` pro `Auto` entitu v ProductionHub)
4. **Sandbox** (PROP-058) — riziko: test harness bez reálného spuštění (jako PROP-046). Mitigace: CLI command `metaforge preview run-method` MUSÍ fungovat end-to-end před uzavřením PROP
5. **Healing** (PROP-059) — riziko: framework bez consumera (jako PROP-039). Mitigace: odloženo, dokud data neukážou potřebu

---

## 6. Follow-up 2026-07-18: PROP-063 — Pozitivní příklad úklidu

PROP-063 (Remove Explicit Workflow Modeling) validuje hlavní tezi tohoto auditu:

- **Workflow model (6 typů)** byl v platformě od PROP-020 (2026-07-04), ale nikdy neměl:
  - Authoring use-case (0 CLI/MCP commandů)
  - Projekci (0 read-path integrace)
  - Napojení na doménový model
- Byl to přesně ten pattern: **infrastruktura bez konzumenta**
- PROP-063 ho kompletně odstranil (10 souborů, 4 modifikace) a nahradil `FlowGraphSection` — odvozenou read-only vizualizací z entit a relací (PROP-062)
- **612/612 testů, 0 regresí**
- Tag `archive/workflow-last` (`be1c052`) pro zpětnou dohledatelnost

**Lekce:** Když se najde overkill infrastruktura, je správné ji odstranit — ne "nechat být". Workflow model tu byl 14 dní jako mrtvá váha. Čím dřív se overkill identifikuje a odstraní, tím míň kódu na něm začne záviset.

**Doporučení pro budoucí audity:** Periodicky (každé 2–4 týdny) kontrolovat, zda nepřibyla další "infrastruktura bez konzumenta" — zejména v Core vrstvě, kde je riziko nejvyšší (viz PROP-039).
