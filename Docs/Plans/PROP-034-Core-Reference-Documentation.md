# PROP-034 Core Reference Documentation + Support Matrix

Typ výsledku: Candidate Proposal
Zdroj podnětu: Koumák — analýza Perplexity konverzace (d773bf6a)
Stav životního cyklu: Candidate
Rozhodovací owner:
Poslední revize: 2026-07-07

Priorita: High
Oblast: Core, Docs, Governance
Owner:
Datum vytvoření: 2026-07-07
Aktualizováno: 2026-07-07

Navazuje na:
- IDEA-001 Core Reference Documentation
- IDEA-002 Support Matrix as Implementation Backlog
- IDEA-003 Roundtrip Boundary Definition

Blokuje:
- —

Související soubory:
- `New_Architecture/04-Core-Elements.md`
- `New_Architecture/05-Core-Behaviors.md`
- `New_Architecture/08-Methods-and-SelfHealing.md`
- `New_Architecture/09-Authoring-Kernel-and-Multi-Output-Model.md`
- `PROP-031-Core-Statement-System.md`
- `PROP-032-Integration-Tests-Core-Generators.md`

## 1. Kontext

Perplexity Computer agent analyzoval `New_Architecture/` dokumentaci a identifikoval, že Core vrstvě chybí ucelená "support matrix" — přehled toho, co Core skutečně umí reprezentovat z C# světa, v jakém stavu podpory to je a kde jsou hranice.

Dnes dokumentace popisuje architekturu (vrstvy, elementy, chování), ale chybí referenční dokument, podle kterého by vývojář nebo AI agent mohl ověřit: "Tahle C# konstrukce projde Core modelem beze ztráty?"

## 2. Problém dnes

- **Žádná support matrix:** Není přehled, které C# konstrukce jsou Supported / Partial / Planned / Unsupported.
- **Nejasná hranice metod:** `08-Methods-and-SelfHealing.md` ukazuje signatury, ale není jasné, co z těla metody je Core a co už ne.
- **Nejasná hranice AST:** Expressions a Statement systém existuje, ale není explicitně vymezené, co je strukturované a co text/AI body.
- **Chybějící roundtrip kontrakt:** Není definované, co jde převést C# → Core → C# bez ztráty.
- **Ad-hoc plánování:** Rozhodování o tom, co implementovat dál, není podložené daty o stavu podpory.

## 3. Cíl

- Vytvořit sadu referenčních dokumentů `Docs/Core/`, které definují podporu C# konstrukcí v Core modelu.
- Každá konstrukce má stav podpory, C# ukázku, Core reprezentaci a omezení.
- Hlavní index (00-Support-Matrix.md) slouží jako živý backlog pro implementaci.
- Dokumentace slouží vývojářům, AI agentům i jako podklad pro plánování.

## 4. Architektonické invarianty

- BusinessAuthoringDocument zůstává source of truth.
- CommandLog zůstává append-only.
- Core nesmí nést logiku, která patří do vyšší vrstvy.
- AI je volitelná vrstva, ne podmínka základní funkčnosti.

## 5. Scope

### In scope
- Vytvořit `Docs/Core/00-Overview.md` — účel Core, hranice vůči Translator a Generators.
- Vytvořit `Docs/Core/00-Support-Matrix.md` — indexová matice všech konstrukcí se stavem podpory.
- Vytvořit `Docs/Core/01-Type-System.md` — primitiva, custom types, nullable, kolekce, generika.
- Vytvořit `Docs/Core/02-Type-Kinds.md` — class, struct, interface, enum, record; stav podpory.
- Vytvořit `Docs/Core/03-Value-Objects.md` — value objects, presets, catalog, CoreDetail write-back.
- Vytvořit `Docs/Core/04-Methods.md` — signatura, parametry, returns, async, command/query.
- Vytvořit `Docs/Core/05-Expressions-and-AST.md` — co je AST, co text/AI body, hranice.
- Vytvořit `Docs/Core/06-Roundtrip-Boundary.md` — C# → Core → C# pravidla, ztrátové oblasti.
- Vytvořit `Docs/Core/07-Examples.md` — příklady "C# kód → Core reprezentace → popis".
- Každá položka v dokumentech má jednotnou šablonu: Název, Stav podpory, C# kód, Core reprezentace, Popis mapování, Omezení, Odkaz na test/kód.

### Out of scope
- Automatická generace matice z kódu (to bude samostatný follow-up).
- Implementace chybějících konstrukcí — matice slouží jako backlog, implementace patří do samostatných PROP.
- Změna Core API — dokumentace popisuje existující stav, ne redesignuje Core.

## 6. Návrh řešení

### Cílový návrh

Sada `Docs/Core/` dokumentů s jednotnou strukturou:

Dokument | Obsah
---------|------
`00-Overview.md` | Účel Core, hranice, source-of-truth, vztah k Translator/Generators
`00-Support-Matrix.md` | Indexová matice: konstrukce × stav podpory × PROP vazba × priorita
`01-Type-System.md` | Primitiva, nullable, kolekce, generika, custom types
`02-Type-Kinds.md` | Class, struct, interface, enum, record (každý se stavem podpory)
`03-Value-Objects.md` | StrongType/ValueObject, presets, catalog, CoreDetail write-back
`04-Methods.md` | Signatura, parametry, return typy, async/void, MethodBodyKind
`05-Expressions-and-AST.md` | Expression model, Statement AST, hranice "co je AST a co ne"
`06-Roundtrip-Boundary.md` | Full roundtrip / Lossy / Unsupported, diagnostiky
`07-Examples.md` | Reálné příklady C# → Core → popis pro každou podporovanou konstrukci

### Formát každého záznamu

```
## Konstrukce: Class with auto-properties

Status: Supported

C#:
public class Customer
{
    public string Name { get; set; }
    public int Age { get; set; }
}

Core:
- TypeKind: Class
- Name: Customer
- Properties:
  - Name: string
  - Age: int

Popis:
Mapuje se na základní typový model třídy s vlastnostmi.

Omezení:
Nepopisuje implementační logiku metod — pouze signatury a properties.

Test: Tests/MetaForge.Core.Integration.Tests/.../C1_BasicClass
```

### Podporová matice (Support Matrix)

| Konstrukce | Stav | Core reprezentace | PROP | Priorita |
|-----------|------|-------------------|------|----------|
| Class | Supported | TypeKind.Class | PROP-002 | ✅ |
| Class with auto-properties | Supported | ClassElement + PropertyElement | PROP-002 | ✅ |
| Sealed Class | Supported | ClassElement.IsSealed | PROP-024 | ✅ |
| Abstract Class | Supported | ClassElement.IsAbstract | PROP-024 | ✅ |
| Static Class | Supported | ClassElement.IsStatic | PROP-024 | ✅ |
| Partial Class | Partial | ClassElement s poznámkou | — | Medium |
| Record Class | Partial | ClassElement + IsRecord | PROP-024 | High |
| Record Struct | Planned | StructElement + IsRecord | — | Medium |
| Struct | Supported | TypeKind.Struct | PROP-002 | ✅ |
| ReadOnly Struct | Supported | StructElement.IsReadOnly | PROP-024 | ✅ |
| Interface | Supported | TypeKind.Interface | PROP-002 | ✅ |
| Enum | Supported | TypeKind.Enum | PROP-002 | ✅ |
| Flags Enum | Partial | EnumElement s Flags | — | Low |
| Method (signatura) | Supported | MethodElement | PROP-002 | ✅ |
| Method (tělo jako AST) | Partial | MethodElement.Body = Statement | PROP-031 | 🔵 |
| Method (async) | Supported | MethodElement.IsAsync | PROP-024 | ✅ |
| Property (get/set) | Supported | PropertyElement | PROP-002 | ✅ |
| Property (get-only) | Supported | PropertyElement.IsReadOnly | PROP-024 | ✅ |
| Property (init-only) | Partial | PropertyElement s poznámkou | — | Medium |
| Property (required) | Planned | PropertyElement.IsRequired | — | High |
| Field | Partial | FieldElement | — | Low |
| Constructor | Partial | ConstructorElement | — | Medium |
| Expression (arithmetic) | Supported | ComputedExpression | PROP-024 | ✅ |
| Expression (comparison) | Supported | ComputedExpression | PROP-024 | ✅ |
| If/Else statement | Partial | IfStatement | PROP-031 | 🔵 |
| For statement | Partial | ForStatement | PROP-031 | 🔵 |
| While statement | Partial | WhileStatement | PROP-031 | 🔵 |
| Return statement | Partial | ReturnStatement | PROP-031 | 🔵 |
| Block statement | Partial | BlockStatement | PROP-031 | 🔵 |
| Primary Constructor | Unsupported | — | — | Low |
| Deconstruct | Unsupported | — | — | Low |
| Local functions | Unsupported | — | — | Low |
| Preprocessor directives | Unsupported | — | — | Low |

### Rozdělení odpovědností

- **Core:** Nic se nemění — dokumentace popisuje existující stav.
- **Docs:** Nová sada `Docs/Core/` dokumentů.
- **Tests:** Propojení s existujícími snapshot testy (PROP-032) jako living documentation.
- **Governance:** Matice slouží jako backlog — Planning agent z ní čerpá.

### Proč je tento návrh správný

- Místo dalšího rozšiřování nejdřív udělat z Core čitelný kontrakt.
- Pomůže to produktově — lze přesně říct, jaký C# kód jde převést do metadat.
- AI agenti (Perplexity, Copilot) dostanou referenční dokument, podle kterého mohou pracovat.
- Matice je přímý backlog — každý `Planned` nebo `Partial` řádek je kandidát na nový PROP.

## 7. Implementační dopad

### Změněné projekty nebo soubory

- Nové: `Docs/Core/00-Overview.md`
- Nové: `Docs/Core/00-Support-Matrix.md`
- Nové: `Docs/Core/01-Type-System.md`
- Nové: `Docs/Core/02-Type-Kinds.md`
- Nové: `Docs/Core/03-Value-Objects.md`
- Nové: `Docs/Core/04-Methods.md`
- Nové: `Docs/Core/05-Expressions-and-AST.md`
- Nové: `Docs/Core/06-Roundtrip-Boundary.md`
- Nové: `Docs/Core/07-Examples.md`
- Možná aktualizace: `New_Architecture/04-Core-Elements.md` (odkaz na Docs/Core/)

### API a kontrakty

- Žádné změny API — čistě dokumentační úkol.

### Testy

- Propojit s existujícími Verify snapshot testy (PROP-032) — snapshoty jako living documentation.
- Doplnit odkazy na testy v každém záznamu Core reference.

### Dokumentace

- Toto je primárně dokumentační úkol.
- Nová složka `Docs/Core/` s referenčními dokumenty.

## 8. Implementační fáze

### Fáze 1: Přehled a matice
- Vytvořit `00-Overview.md` a `00-Support-Matrix.md`.
- Matice obsahuje všechny známé konstrukce z PROP-002, PROP-024, PROP-031.
- Projít existující kód a doplnit stavy podpory.

### Fáze 2: Typový systém a typové druhy
- Vytvořit `01-Type-System.md` a `02-Type-Kinds.md`.
- Popsat primitiva, nullable, kolekce, generika.
- Popsat class, struct, interface, enum, record — pro každý stav podpory.

### Fáze 3: Value objects a metody
- Vytvořit `03-Value-Objects.md` a `04-Methods.md`.
- StrongType/ValueObject, presets, catalog, CoreDetail.
- Signatura metod, parametry, async/void, MethodBodyKind.

### Fáze 4: Expressions, AST a roundtrip
- Vytvořit `05-Expressions-and-AST.md` a `06-Roundtrip-Boundary.md`.
- Expression model, Statement AST, hranice.
- Full roundtrip / Lossy / Unsupported.

### Fáze 5: Příklady a finalizace
- Vytvořit `07-Examples.md` s reálnými příklady.
- Propojit se snapshot testy.
- Aktualizovat `New_Architecture/` cross-odkazy.

## 9. Otevřené otázky

- OQ-034-01: Jak udržovat support matici živou? Ručně, nebo generovat z kódu (např. z atributů `[Supported]`)?
- OQ-034-02: Je roundtrip C# → Core → C# cílový stav, nebo pouze užitečná vlastnost?
- OQ-034-03: Má se matice verzovat spolu s Core verzí?

## 10. Rizika a trade-offy

- Riziko zastarání: Dokumentace se musí udržovat při každé změně Core — hrozí "mrtvý dokument".
- Riziko neúplnosti: Matice nemusí pokrýt všechny konstrukce — nutná pravidelná revize.
- Trade-off: Ručně psaná matice vs generovaná — ruční je rychlejší na start, generovaná je udržitelnější.

## 11. Validace

- Build: N/A (dokumentační úkol)
- Testy: Propojení s existujícími snapshot testy
- Smoke scénáře: Perplexity / Copilot agent dostane Docs/Core/ jako referenci — ověří se, že dokumentace je použitelná pro AI
- Ruční kontrola: Koumák nebo Reviewer ověří úplnost matice
- Hotovo: Všech 9 dokumentů existuje, matice pokrývá všechny známé konstrukce

## 12. Výsledek po dokončení

Vyplnit až při uzavření návrhu.
