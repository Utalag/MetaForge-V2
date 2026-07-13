# PROP-051 Support Matrix — Strojově čitelný contract map

Typ výsledku: Candidate Proposal
Zdroj podnětu: IDEA-024 (Koumák + Perplexity)
Stav životního cyklu: Candidate
Rozhodovací owner:
Poslední revize: 2026-07-11

Priorita: Medium
Oblast: Governance, Docs, Tests
Owner:
Datum vytvoření: 2026-07-11
Aktualizováno: 2026-07-11

Navazuje na:
- PROP-034 (Core Reference Documentation — hotovo; vytvořil lidsky čitelnou matici)
- PROP-032 (Integration Tests — hotovo)
- PROP-048 (Generator Render Core Tests — kandidát)
- PROP-049 (Test Framework Consolidation — kandidát)

Blokuje:
- —

Související soubory:
- `Docs/Core/00-Support-Matrix.md` — existující lidsky čitelná matice (81 položek)
- `Docs/Core/00-Support-Matrix.yaml` — nový strojově čitelný zdroj

## 1. Kontext

PROP-034 vytvořil lidsky čitelnou Support Matrix (`Docs/Core/00-Support-Matrix.md`) s 81 položkami. Ta je cenná pro vývojáře, ale není strojově čitelná — AI agenti, CI pipeline a testovací nástroje ji nemohou konzultovat.

IDEA-024 (Perplexity konverzace) upozornila na potřebu explicitně rozlišovat:
- Co generátor **technicky umí** vyrenderovat
- Co je **oficiálně podporovaný kontrakt** (public supported)
- Co je **interní capability** (bez garance stability)

## 2. Problém dnes

- **Lidská matice existuje** (81 položek v Markdown tabulce), ale není strojově čitelná — AI agenti, CI ani testy ji nemohou konzultovat.
- **Chybí contract status**: není rozlišení "public supported" vs "internal capability" vs "experimental".
- **Není propojení s testy**: nelze zjistit, která featura má snapshot test, syntax test nebo unit test.
- **Strojová verze chybí**: AI agenti nemají rozhodovací vstup pro "mohu tuto featuru bezpečně použít?".
- **Stav některých položek v matici je zastaralý**: Delegate, Event, Operator jsou v matici jako "Planned", ale reálně jsou v kódu hotové (PROP-037, PROP-043).

## 3. Cíl

- Vytvořit YAML zdroj (`Docs/Core/00-Support-Matrix.yaml`) jako **jeden zdroj pravdy** pro contract statusy.
- Lidská Markdown matice se generuje z YAML (nebo se udržuje ručně s křížovým odkazem).
- Každá featura má explicitní contract status: `public-supported`, `advanced`, `internal`, `experimental`.
- AI agenti mohou YAML konzultovat pro rozhodování.
- YAML je verzovatelný a diffovatelný.

## 4. Architektonické invarianty

- YAML je source of truth pro contract statusy, ne pro implementaci.
- Testování se neřídí YAMLem — YAML je dokumentační a rozhodovací nástroj.
- Změna contract statusu je schvalovací proces, ne jen editace YAMLu.

## 5. Scope

### In scope
- Vytvoření `Docs/Core/00-Support-Matrix.yaml` s přepisem všech 81 položek z Markdown matice
- Definice contract statusů: `public-supported`, `advanced`, `internal`, `experimental`
- Propojení na test coverage (snapshot test, syntax test, unit test)
- Aktualizace Markdown matice (opravit zastaralé stavy — Delegate, Event, Operator)
- Stručná dokumentace procesu pro změnu contract statusu

### Out of scope
- Automatické generování Markdown matice z YAML (ruční udržování stačí)
- Automatická validace YAML proti kódu (generování z reflexe — overengineering)
- Změny v testovacím frameworku
- Generování testů z YAML

## 6. Návrh řešení

### Formát YAML

```yaml
# Support Matrix — MetaForge Core + Generators
# Contract statusy:
#   public-supported: garantovaná stabilita, testy povinné
#   advanced: stabilní, ale vyžaduje zkušeného uživatele
#   internal: generátor umí, ale není oficiálně vystaveno
#   experimental: může se změnit nebo zmizet
#   
# Test coverage:
#   snapshot: název snapshot testu (nebo true/false)
#   syntax: true/false — Roslyn syntax validation
#   unit: true/false — unit test rendereru

version: "1.0"
updated: 2026-07-11

type_kinds:
  - name: Class
    status: public-supported
    core: ClassElement
    generator: true
    tests:
      snapshot: ClassModifierSnapshots
      syntax: true
      unit: true
  - name: Sealed Class
    status: public-supported
    core: ClassElement.IsSealed
    generator: true
    tests:
      snapshot: C3_SealedClass
      syntax: true
  - name: Delegate
    status: public-supported
    core: DelegateElement
    generator: true
    tests:
      snapshot: false   # ❌ TODO: chybí snapshot test
      syntax: false
      unit: false
  - name: Event
    status: public-supported
    core: EventElement
    generator: true
    tests:
      snapshot: false
      syntax: false
  - name: Operator
    status: public-supported
    core: OperatorElement
    generator: true
    tests:
      snapshot: false
      syntax: false
  # ... další položky

members:
  - name: Method
    status: public-supported
    core: MethodElement
    generator: true
    tests:
      snapshot: MethodSnapshots
      syntax: true
  - name: Property (get/set)
    status: public-supported
    core: PropertyElement
    generator: true
    tests:
      snapshot: P1_GetSet
      syntax: true
  # ...

expressions:
  - name: Binary Operation
    status: public-supported
    core: BinaryExpression
    generator: true
    tests:
      snapshot: true  # E2E
      syntax: true
      unit: false     # ❌ TODO: chybí unit test (PROP-048)
  # ...

statements:
  - name: If/Else
    status: public-supported
    core: IfStatement
    generator: true
    tests:
      snapshot: true
      syntax: true
      unit: false     # ❌ TODO: chybí unit test (PROP-048)
  # ...
```

### Contract statusy

| Status | Význam | Testy | AI agent |
|--------|--------|-------|----------|
| `public-supported` | Garantovaná stabilita | Povinné (snapshot + syntax + unit) | Může bezpečně používat |
| `advanced` | Stabilní, složitější použití | Doporučené | Používat s upozorněním |
| `internal` | Technicky funguje, bez garance | Volitelné | Nepoužívat bez explicitního povolení |
| `experimental` | Může zmizet | Minimální | Nepoužívat v produkci |

### Proces změny contract statusu

1. Navrhovatel založí ISSUE nebo PROP
2. V PROP uvede, které featury mění status
3. Po schválení PROP se updatuje YAML
4. Testy se doplní dle nového statusu

## 7. Implementační dopad

### Změněné projekty nebo soubory
- `Docs/Core/00-Support-Matrix.yaml` — nový
- `Docs/Core/00-Support-Matrix.md` — aktualizace zastaralých stavů (Delegate, Event, Operator → ✅ Supported)

### API a kontrakty
- Žádné změny v produkčním kódu.

### Testy
- Žádné nové testy (YAML je dokumentační).

### Dokumentace
- Nový YAML soubor + oprava Markdown matice.

## 8. Implementační fáze

### Fáze 1: Draft YAML (~1 den)
- Přepis všech 81 položek z Markdown matice do YAML
- Přiřazení contract statusů
- Identifikace položek bez test coverage

### Fáze 2: Revize a oprava matice (~0.5 dne)
- Oprava zastaralých stavů (Delegate, Event, Operator)
- Křížová kontrola s PROP-045 (13/13 scénářů)
- Dokumentace procesu změny statusu

## 9. Otevřené otázky

- **OQ-051-01**: Má se YAML verzovat nezávisle (semver) nebo stačí git historie?
- **OQ-051-02**: Kdo rozhoduje o změně contract statusu? (Stejný proces jako PROP lifecycle — navrhne kdokoliv, schvaluje owner?)
- **OQ-051-03**: Máme generovat Markdown z YAML automatem v CI, nebo stačí ruční udržování? (Pro první verzi ručně.)

## 10. Rizika a trade-offy

- **Riziko zastarání**: YAML rychle zastará, pokud nebude udržovaný. Mitigace: součást review processu — každý PROP by měl aktualizovat YAML.
- **Riziko rigidity**: Ne každá featura potřebuje být "public supported". Mitigace: internal a experimental statusy pokrývají zbytek.
- **Vědomý kompromis**: YAML není generovaný z kódu — je ručně udržovaný. Automatizace by byla overengineering pro první verzi.

## 11. Validace

- Build: YAML je validní (projde YAML parserem)
- Smoke: AI agent přečte YAML a zjistí, že `Class` je public-supported
- Ruční kontrola: Markdown matice odpovídá YAML
- Jak poznáme, že je návrh hotový: YAML existuje s 81+ položkami; zastaralé stavy opraveny

## 12. Výsledek po dokončení

*— vyplnit při uzavření —*
