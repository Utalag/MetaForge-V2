# IDEA-026 Generator Architecture Decision — TieredCodeGenerator, IncrementalCodeGenerator, ProjectScaffoldGenerator

Stav: Rozhodnuto (Open Question → Decision)
Oblast: Core, Generators, Governance, Monetization
Zdroj: Perplexity konverzace db7f49c1-8f18-4f2e-8da2-b9b6e1f5c122 — otázka: "Jsou to cílové součásti architektury, nebo historický/experimentační ballast?"
Datum vytvoření: 2026-07-11
Poslední revize: 2026-07-11 (finalizováno po diskusi s Perplexity)

## 1. Kontext

Perplexity položila zásadní otázku: *"Považuješ `TieredCodeGenerator`, `IncrementalCodeGenerator` a `ProjectScaffoldGenerator` za cílovou architekturu, nebo spíš za mezistav/starší experiment? Na tom se láme, jestli je máme testovat hned, nebo nejdřív zpochybnit jejich existenci."*

Dokumentace `10-Generators.md` popisuje jediný aktivní generátor (`CodeGenerator`) ale v kódu existují 3 další generátorové třídy, o kterých architektonická dokumentace mlčí. To vytváří nejistotu:
- Pokud jsou target → chybí testy a dokumentace (kritické).
- Pokud jsou experiment → chybí označení a rozhodnutí, zda mají přežít.
- Pokud jsou legacy → měly by být odstraněny.

## 2. Problém dnes

- **TieredCodeGenerator** — decorator nad CodeGenerator pro licencované/tiered generování. V kódu je, v dokumentaci ani v PROPOSALS není zmíněný. Existence dává smysl pro monetizaci, ale architektura umisťuje billing gate do `BusinessAuthoringHostFacade` přes `IGenerationCostPolicy`, ne do generátoru.

- **IncrementalCodeGenerator** — generator s dirty-tracking, pouze přegenerovává změněné elementy. Dokumentace `10-Generators.md` o něm mlčí. Není součástí target generator kontraktu.

- **ProjectScaffoldGenerator** — generuje .csproj a folder strukturu. Mimo scope "jediného generátoru, který čte Core elementy". Patří do tooling/governance vrstvy.

- Důsledek: nelze rozhodnout o test prioritách — zda testovat, označit, nebo odstranit.

## 3. Rozhodnutí (po diskusi s Perplexity)

### 3.1 TieredCodeGenerator → Legacy/Refactor

**Diskuse:**
- Původní návrh: Target — monetizace potřebuje credit gating.
- Perplexity argument: *"TieredCodeGenerator mi nesedí. Monetizace patří do `BusinessAuthoringHostFacade` přes `IGenerationCostPolicy`, ne do generator vrstvy. Target je **credit-gated code export**, nikoli `TieredCodeGenerator` jako samostatný generator pattern."*
- Uživatel potvrdil: *"dává to smysl, ok"*

**Verdikt:** ✅ Přijato
- Credit-gated code export je target capability.
- `TieredCodeGenerator` jako třída míchá renderování a licensing gate — špatné umístění.
- Řešení: Refaktorovat na `IGenerationCostPolicy` ve Facade, `TieredCodeGenerator` označit jako Legacy a naplánovat odstranění.

### 3.2 IncrementalCodeGenerator → Experimental

**Diskuse:**
- Původní návrh: Experimental — není v target generator kontraktu.
- Perplexity: souhlasí, není v cílové architektuře.
- Uživatel: souhlasí.

**Verdikt:** ✅ Přijato
- Označit `// [Experimental]` v kódu.
- Nerozšiřovat testy — pouze základní "neláme se to".
- Nastavit review datum (např. 2026-10-11).

### 3.3 ProjectScaffoldGenerator → Move out of generator scope

**Diskuse:**
- Původní návrh: Remove/Přesunout — není Core→C# transformace.
- Perplexity: souhlasí, scaffold je tooling/governance concern.
- Uživatel: souhlasí.

**Verdikt:** ✅ Přijato
- Naplánovat přesun do tooling/governance vrstvy (CLI nebo samostatný balík).
- Do té doby ponechat, neinvestovat do testů.

### 3.4 Decision statement (formulace Perplexity)

> **„Target je credit-gated code export, nikoli TieredCodeGenerator jako samostatný generator pattern; billing zůstává ve Facade a generator vrstva zůstává úzká, C-only a zaměřená na render + packaging."**

## 4. Důsledky

| Třída | Status | Akce | Priority testů |
|-------|--------|------|----------------|
| `TieredCodeGenerator` | 🟥 Legacy/Refactor | Naplánovat refactoring na `IGenerationCostPolicy` | ❌ Žádné — pouze ověřit, že se neláme stávající funkcionalita |
| `IncrementalCodeGenerator` | 🟨 Experimental | Označit `[Experimental]`, review za 3 měsíce | ⚠️ Pouze základní smoke test |
| `ProjectScaffoldGenerator` | 🟦 Move out | Naplánovat přesun | ❌ Žádné nové |
| `CodeGenerator` | ✅ Target | Stávající testy + rozšíření dle IDEA-025 | ✅ Prioritní |

## 5. Otevřené otázky

- Kdo implementuje `IGenerationCostPolicy` a kde bude umístěna (Translator/Facade)?
- Má `IncrementalCodeGenerator` smysl po vyřešení dirty-tracking na úrovni BusinessModel (PROP-xxx)?
- Kam přesunout `ProjectScaffoldGenerator` — do CLI, nebo do samostatného ForgeBlock balíku?

## 6. Doporučený další krok

**Předat Planning agentovi** jako podklad pro:
1. Rozhodnutí je hotové — zapsat do `OpenQuestions` jako uzavřené.
2. `IDEA-025` (Generator Render Core Tests) může běžet — už víme, co je target.
3. Vytvořit PROP pro refactoring `TieredCodeGenerator` → `IGenerationCostPolicy`.

Navazuje na: `10-Generators.md`, `29-Monetization.md`, `12-Host-Surfaces.md`
Závisí na: `IDEA-024` (Support Matrix), `IDEA-025` (teprve po tomto rozhodnutí)
