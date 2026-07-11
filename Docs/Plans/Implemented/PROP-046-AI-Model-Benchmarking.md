# PROP-046 AI Model Benchmarking — Referenční vs lokální modely

Typ výsledku: Candidate Proposal
Zdroj podnětu: User
Stav životního cyklu: Candidate
Rozhodovací owner:
Poslední revize: 2026-07-09

Priorita: 🟡 Vysoká
Oblast: AI / Tests
Owner:
Datum vytvoření: 2026-07-09
Aktualizováno: 2026-07-09

Navazuje na:
- PROP-027 (AI Layer — MetaForge.Ai, OllamaAdapter, PromptRegistry)
- PROP-019 (Translator — IAiTranslator, AI-assisted překlad)

Blokuje:
- PROP-027 Fáze 2 (výběr výchozího Ollama modelu pro produkci)

Související soubory:
- `Tests/MetaForge.Ai.Tests/`
- `Src/MetaForge.Ai/Translation/AiTranslationService.cs`
- `Src/MetaForge.Ai/Adapters/OllamaAdapter.cs`
- `Src/MetaForge.Ai/Abstractions/IAiBackendAdapter.cs`

## 1. Kontext

Platforma MetaForge používá AI na dvou místech:
1. **Attribute enrichment** — `ITranslationService.EnrichAsync()` → AI navrhuje C# typy, validace, default hodnoty
2. **Překlad přirozeného jazyka → Core elementy** — budoucí rozšíření (prompt → ClassElement/PropertyElement/MethodElement strom)

Uživatel se dostane maximálně na `BusinessAuthoringHostFacade`. Zbytek je buď deterministicky generovaný (PatchEngine → CommandLog → ReplayEngine → Core elementy → Generator), nebo pomocí AI.

Otázka: **Které lokální modely (Ollama) zvládnou stejnou kvalitu jako cloudové modely (GPT-4, Claude)?**

Cílem je najít nejslabší (= nejlevnější, nejrychlejší) lokální model, který ještě produkuje srovnatelný výstup.

## 2. Problém dnes

- Nemáme žádné měřítko kvality AI výstupu
- Nevíme, jestli gemma3:12b, llama3.2:3b, phi3, nebo jiný model dává použitelné výsledky
- Prompt engineering probíhá naslepo — nevíme, co které modely potřebují (více kontextu, examples, delší prompt)
- Není způsob, jak automaticky ověřit, že upgrade modelu nezhoršil kvalitu

## 3. Cíl

Vytvořit **benchmark test suite**, který:

1. **Referenční sada**: X promptů (přirozený jazyk) + referenční výstupy od cloudového modelu (GPT-4o / Claude 3.5 Sonnet)
2. **Testovatelné lokální modely**: gemma3:12b, llama3.2:3b, phi3:mini, mistral, codellama, deepseek-coder...
3. **Metrika**: Strukturální ekvivalence Core elementů (stejné názvy tříd/properties/metod, správné typy)
4. **Výstup**: Matice model × scénář → pass/fail, report co který model nezvládl

Cílový stav: Víme, který nejslabší model zvládne 90%+ scénářů, a co potřebují ty slabší (better prompt, few-shot examples, delší kontext).

## 4. Architektonické invarianty

- AI je volitelná — benchmark testy jsou volitelné (vyžadují běžící Ollama)
- Kontrakty oddělené od implementací — testujeme přes `IAiBackendAdapter`
- BusinessAuthoringDocument zůstává source of truth — AI výstup se validuje jako Core elementy

## 5. Scope

### In scope
- Sada 10-20 referenčních promptů pokrývajících běžné use-casy
- Generování referenčních výstupů cloudovým modelem (jednorázově, uloženo jako snapshot)
- Test runner, který pošle stejné prompty na Ollama modely
- Strukturální komparátor Core elementů (ClassElement, PropertyElement, MethodElement)
- Matice výsledků: model × scénář
- Integrace s `PromptRegistry` — testuje se aktuální produkční prompt

### Out of scope
- Plná sémantická ekvivalence (stejné expression trees) — stačí strukturální
- Testování atributového enrichmentu (to je `ITranslationService.EnrichAsync`, jiný scope)
- Automatické ladění promptů — to je až další krok
- Cloudové API integrace — referenční výstup se vygeneruje jednorázově ručně

## 6. Návrh řešení

### 6.1 Referenční sada (snapshot files)

```
Tests/MetaForge.Ai.Tests/
└── Benchmark/
    ├── Prompts/
    │   ├── 001-simple-class.txt          "Vytvoř třídu User s properties Id (int), Name (string), Email (string)"
    │   ├── 002-class-with-methods.txt    "Vytvoř třídu Calculator s metodami Add(a,b), Subtract(a,b)..."
    │   ├── 003-entity-with-enum.txt      "Vytvoř entitu Order s enumem OrderStatus (Draft, Confirmed, Shipped)"
    │   ├── 004-interface-and-impl.txt    "Vytvoř IRepository<T> s GetByIdAsync a AddAsync"
    │   ├── 005-dto-with-validation.txt   "Vytvoř DTO CreateUserRequest s validacemi (NotEmpty, Email, MinLength)"
    │   └── ...
    └── References/
        ├── 001-simple-class.json         ← referenční výstup (Core element tree)
        ├── 002-class-with-methods.json
        └── ...
```

### 6.2 Strukturální komparátor

```csharp
public static class CoreElementComparer
{
    /// Vrátí true pokud oba stromy mají stejnou strukturu (názvy, typy, signatury)
    public static bool AreStructurallyEquivalent(ClassElement reference, ClassElement candidate);
    
    /// Detailní diff — co přesně chybí/nesouhlasí
    public static IReadOnlyList<string> Diff(ClassElement reference, ClassElement candidate);
}
```

Porovnává:
- Název třídy, IsStatic, base type
- Properties: název + typ (ignoruje access modifikátory)
- Methods: název + return type + parametry (název + typ)
- Enum: název + members (název + hodnota)
- **Ignoruje**: těla metod, expression trees, XML komentáře, atributy

### 6.3 Test runner

```csharp
[Theory]
[MemberData(nameof(GetBenchmarkScenarios))]
public async Task Model_X_Matches_Reference(string promptFile, string referenceFile)
{
    var prompt = File.ReadAllText(promptFile);
    var referenceJson = File.ReadAllText(referenceFile);
    var reference = JsonSerializer.Deserialize<ClassElement>(referenceJson);
    
    var result = await _aiService.TranslateAsync(prompt); // ← toto ještě neexistuje!
    // nebo:
    var result = await _backend.SendJsonAsync<ClassElement>(prompt);
    
    CoreElementComparer.AreStructurallyEquivalent(reference, result).Should().BeTrue();
}
```

### 6.4 Výstupní matice

```
Model              | 001 | 002 | 003 | 004 | 005 | ... | Pass rate
-------------------|-----|-----|-----|-----|-----|-----|----------
gpt-4o (reference) |  ✓  |  ✓  |  ✓  |  ✓  |  ✓  |     | 100%
gemma3:12b         |  ✓  |  ✓  |  ✗  |  ✗  |  ✓  |     | 60%
llama3.2:3b        |  ✓  |  ✗  |  ✗  |  ✗  |  ✗  |     | 20%
phi3:mini          |  ✓  |  ✓  |  ✓  |  ✗  |  ✓  |     | 80%
mistral:7b         |  ✓  |  ✓  |  ✓  |  ✓  |  ✓  |     | 100%  ← kandidát!
```

### 6.5 Prompt variants

Pro každý model testujeme i varianty promptu:
- **Bare**: holý prompt bez examples
- **With context**: prompt + kontext (existující entity, použité typy)
- **Few-shot**: prompt + 2-3 examples (vzorový vstup → výstup)

To ukáže, jestli stačí vylepšit prompt, nebo je model prostě moc slabý.

## 7. Implementační dopad

### Změněné projekty nebo soubory
- `Tests/MetaForge.Ai.Tests/Benchmark/` — nový adresář s prompty a referencemi
- `Tests/MetaForge.Ai.Tests/Benchmark/CoreElementComparer.cs` — strukturální komparátor
- `Tests/MetaForge.Ai.Tests/Benchmark/AiModelBenchmarkTests.cs` — testy
- `Src/MetaForge.Ai/Translation/` — možná nová metoda `TranslateToCoreElementsAsync(string prompt)` na `IAiBackendAdapter`

### API a kontrakty
- `IAiBackendAdapter` — možná přidat `SendJsonAsync<T>` (už existuje!)
- `CoreElementComparer` — nový helper pro testy

### Testy
- `MetaForge.Ai.Tests.Benchmark` — 10-20 benchmark testů
- Každý test = prompt → Ollama model → strukturální porovnání s referencí
- Testy se přeskakují (`Skip = "Ollama not available"`) když Ollama neběží

### Dokumentace
- `Docs/Plans/PROP-046-AI-Model-Benchmarking.md` (tento soubor)
- `Docs/Benchmark/` — výsledky benchmarku (report)

## 8. Implementační fáze

### Fáze 1 — Referenční sada (1 den)
- Vytvořit 10-20 promptů pokrývajících běžné scénáře
- Jednorázově vygenerovat referenční výstupy (GPT-4o / Claude)
- Uložit jako JSON snapshoty do `Tests/MetaForge.Ai.Tests/Benchmark/References/`
- Implementovat `CoreElementComparer` (strukturální komparátor + diff)

### Fáze 2 — Test runner (1 den)
- Implementovat `AiModelBenchmarkTests` s `[Theory]` napojenou na soubory
- Integrovat `OllamaAdapter` s různými modely (config)
- Implementovat skip logiku když Ollama neběží
- Generovat maticový report (Markdown tabulka)

### Fáze 3 — Prompt variants (1 den)
- Pro každý scénář 3 varianty promptu: bare, with context, few-shot
- Spustit na všech modelech
- Vyhodnotit, jestli vylepšený prompt zvedne pass rate u slabších modelů

### Fáze 4 — Analýza a ladění (1 den)
- Analyzovat diffy (co přesně model pokazil)
- Navrhnout úpravy promptů pro problémové modely
- Dokumentovat findings: "gemma3:12b potřebuje few-shot examples pro interface, jinak zvládne všechno"

## 9. Otevřené otázky

- Měl by benchmark testovat novou metodu `TranslateToCoreElementsAsync(string prompt)` na `IAiBackendAdapter`, nebo použít existující `SendJsonAsync<T>`?
- Referenční model: GPT-4o vs Claude 3.5 Sonnet — který použít jako referenci? (Návrh: GPT-4o, je dostupnější)
- Má se referenční výstup validovat i přes `CoreValidator` pro jistotu?
- Má benchmark zahrnovat i `ITranslationService.EnrichAsync` (atributový enrichment)?

## 10. Rizika a trade-offy

- Riziko: Referenční výstup od GPT-4o nemusí být perfektní — může obsahovat chyby. Validovat přes CoreValidator.
- Riziko: Strukturální komparátor může být příliš přísný (AI nikdy nevygeneruje přesně stejný strom). Použít fuzzy matching.
- Riziko: Malé modely mohou mít problémy s JSON output formátem. Přidat JSON schema constraint do promptu.
- Vědomý kompromis: Netestujeme sémantickou správnost (jestli metoda dělá to co má), jen strukturální.

## 11. Validace

- Build: `dotnet build Tests/MetaForge.Ai.Tests`
- Testy: `dotnet test --filter "Benchmark"` (s běžící Ollama)
- Smoke scénáře: 1 prompt → 1 model → strukturální porovnání s referencí
- Ruční kontrola: Projít diffy u prvních 3 scénářů pro jistotu
