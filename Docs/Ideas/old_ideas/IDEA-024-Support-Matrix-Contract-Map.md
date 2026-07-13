# IDEA-024 Support Matrix — Contract-First Test Map

Stav: Idea
Oblast: Governance, Docs, Tests
Zdroj: Koumák + Perplexity konverzace db7f49c1-8f18-4f2e-8da2-b9b6e1f5c122
Datum vytvoření: 2026-07-11
Poslední revize: 2026-07-11

## 1. Kontext

Nápad vznikl z diskuze o test coverage Core + Generators. Perplexity upozornila, že čím víc kombinací modifikátorů a AST uzlů platforma podporuje, tím víc si zadělává na kombinatorickou explozi v testech a v mapování z BusinessModel/Translator vrstvy. Řešením není přidávat další testy slepě, ale vytvořit **support matrix**, která explicitně rozlišuje:

- Co generátor **technicky umí** vyrenderovat (low-level AST shape)
- Co **BusinessModel/Translator umí adresovat z authoringu**
- Co je **oficiálně podporovaný kontrakt** pro MetaForge V2
- Co je **interní capability** (bez garance stability)

## 2. Problém dnes

- Chybí explicitní registr toho, co je "public supported" vs "internal capability".
- Testy jsou psány ad-hoc podle toho, co zrovna vzniklo — není jasné, zda chybějící testy jsou díra, nebo záměr.
- Vývojář ani AI agent nemají rychlou odpověď na otázku: "Je tato featura součástí garantovaného contractu?"
- Kombinatorická exploze modifikátorů (8 class × 6 property × 4 struct × 3 enum × 15 expression × 12 statement) není nikde zmapovaná.
- `TieredCodeGenerator`, `IncrementalCodeGenerator`, `ProjectScaffoldGenerator` — není jasné, zda jsou target architecture, legacy, nebo experimental (poznámka Perplexity).

## 3. Předběžný směr řešení

Vytvořit **Support Matrix** — živý dokument (nebo YAML/JSON zdroj), který u každé featury eviduje:

| Feature | Vrstva | Authoring support | Snapshot test | Syntax test | Contract status |
|---------|--------|------------------|--------------|-------------|-----------------|
| Class (basic) | Core → Gen | Ano | ✅ C1 | ✅ | ✅ Public supported |
| Generic class | Core → Gen | Ne | ❌ | ❌ | ⚠️ Internal capability |
| Record struct | Core → Gen | Ne | ✅ S3 | ✅ | ⚠️ Internal capability |
| ... | ... | ... | ... | ... | ... |

Typy contract statusů:
- **Public supported** — garantovaná stabilita, testy povinné
- **Advanced contract** — stabilní, ale vyžaduje zkušeného uživatele
- **Internal capability** — generátor umí, ale není oficiálně vystaveno
- **Experimental** — může se změnit nebo zmizet

Dokument by byl propojený s `15-Test-Scaffold.md` a sloužil by jako vstup pro plánování testů a pro AI agenty (aby věděli, co mohou bezpečně použít).

## 4. Signál hodnoty

- **Governance**: explicitní rozhodnutí o tom, co je contract → předvídatelnost pro uživatele i agenty.
- **Testování**: jasná priorita — co má status "public supported" musí mít snapshot + syntax test.
- **AI spolupráce**: agenti mohou konzultovat matrix a generovat kód pouze proti garantovanému surface.
- **Plánování**: odhalí, zda `TieredCodeGenerator` apod. jsou cílová architektura nebo legacy — umožní rozhodnout, zda testovat nebo vyhodit.
- Zapadá do governance epiku (Epic 1) a test scaffoldu (Epic 9).

## 5. Rizika a nejasnosti

- Dokument rychle zastará, pokud nebude udržovaný — nutná automatizace (generování matrix z kódu/testů?).
- Kdo rozhoduje o zařazení do "public supported"? Potřeba governance procesu.
- Nebezpečí přehnané rigidity — ne každá capability potřebuje být "public supported", ale měla by být viditelně označená.
- Možná je lepší začít jako YAML soubor (strojově čitelný) a dokument generovat, než opačně.

## 6. Aktuální stav

✅ Převedeno na Candidate → PROP-051

## 7. Doporučený další krok

**Follow-up** — nejdříve vytvořit draft support matrix pro Generators (20-30 záznamů) a ověřit, zda formát dává smysl. Teprve pak rozhodnout o automatizaci a rozšíření na celou platformu.

Navazuje na: `15-Test-Scaffold.md`, `10-Generators.md`, `PROPOSALS.md` (PROP-032, PROP-033, PROP-037)
Otevřená otázka: Kdo a jak spravuje contract statusy?
