# IDEA-001 Core Reference Documentation

Stav: Idea
Oblast: Core, Docs
Zdroj: Koumák — analýza Perplexity konverzace (d773bf6a)
Datum vytvoření: 2026-07-07
Poslední revize: 2026-07-07

## 1. Kontext

Perplexity Computer agent analyzoval `New_Architecture/` dokumentaci a identifikoval, že Core vrstvě chybí ucelená referenční dokumentace, která by popsala, co Core skutečně umí reprezentovat — tedy jaké C# konstrukce jsou podporované, jak se mapují do Core modelu a kde jsou hranice.

## 2. Problém dnes

- Core dokumentace (`New_Architecture/04-Core-Elements.md`, `05-Core-Behaviors.md`) popisuje architekturu, ale chybí jí "support matrix" — přehled, které C# konstrukce jsou Supported / Partial / Planned / Unsupported.
- Method dokument (`08-Methods-and-SelfHealing.md`) ukazuje signatury a async/void chování, ale není jasné, co z těla metody je ještě Core a co už ne.
- Expressions a AST hranice není explicitně vymezená — není jasné, co je strukturovaně reprezentované a co už je jen text / AI body.
- Vývojář nebo AI agent nemá jediný dokument, kde by si ověřil, jestli jím napsaný kód půjde převést do metadat.

## 3. Předběžný směr řešení

Vytvořit sadu `Docs/Core/` referenčních dokumentů:

| Dokument | Obsah |
|----------|-------|
| `Docs/Core/00-Overview.md` | Účel Core, hranice vůči Translator a Generators |
| `Docs/Core/01-Type-System.md` | Primitiva, custom types, nullable, kolekce, generika |
| `Docs/Core/02-Type-Kinds.md` | Class, struct, interface, enum, record; stav podpory |
| `Docs/Core/03-Value-Objects.md` | Value objects, presets, catalog, CoreDetail write-back |
| `Docs/Core/04-Methods.md` | Signatura, parametry, returns, async, command/query |
| `Docs/Core/05-Expressions-and-AST.md` | Co je AST a co už text/AI body |
| `Docs/Core/06-Roundtrip-Boundary.md` | C# → Core → C# pravidla, ztrátové oblasti |
| `Docs/Core/07-Examples.md` | Příklady "C# kód → Core reprezentace → popis" |

Každá položka v dokumentech bude mít jednotnou šablonu:
- Název konstrukce
- Stav podpory: Supported / Partial / Planned / Unsupported
- Ukázka C# kódu
- Core reprezentace
- Stručný popis mapování
- Omezení a ztráty
- Odkaz na test nebo související typ v kódu

## 4. Signál hodnoty

- Developer experience: vývojář i AI agent ví přesně, co Core umí.
- Plánování: matice podpory slouží jako backlog pro další implementaci.
- Kvalita: round-trip boundary eliminuje dohady o tom, co jde převést bez ztráty.
- Produkt: lze přesně říct, jaký C# kód jde převést do metadat.

## 5. Rizika a nejasnosti

- Dokumenty musí být udržované — pokud se Core rozšiřuje, musí se aktualizovat i reference.
- Hrozí, že se dokumentace stane "mrtvým dokumentem", pokud se nepropojí s testy (např. Verify snapshots jako living documentation).
- Není jasné, kdo bude dokumentaci udržovat — zda Coding agent při každé změně Core, nebo samostatný dokumentační úkol.

## 6. Doporučený další krok

- Candidate Proposal — navrhuji vznik PROP-034 s detailním plánem.
- Doporučuji propojit s Verify snapshot testy, aby dokumentace žila z kódu.
