# Core Reference Documentation — Overview

> Účel Core vrstvy, její hranice a vztah k ostatním vrstvám MetaForge.

## Co je Core?

Core vrstva (`MetaForge.Core`) je **sémantický model** programovacích konstrukcí. Není to syntaktický parser ani code generator — je to abstraktní reprezentace typů, členů, výrazů a statementů, ze které lze generovat kód v libovolném jazyce.

Core je **source of truth** pro strukturu kódu. Ostatní vrstvy:
- **BusinessModel** — ukládá uživatelské záměry jako CommandLog, ze kterého se Core model sestavuje.
- **Translator** — překládá mezi BusinessModel a Core modelem (projekce, write-back).
- **Generators** — transformuje Core model do textového kódu (C#, atd.).
- **AI** — volitelně obohacuje Core model o sémantické informace.

## Hranice Core

### Co Core reprezentuje

| Kategorie | Příklady |
|-----------|----------|
| **Typy** | Class, Interface, Struct, Enum, Record |
| **Členy** | Method, Property, Parameter, Field |
| **Výrazy** | Binary, Unary, MethodCall, Lambda, Switch, atd. |
| **Statements** | If, For, While, Return, Block, Switch, TryCatch, Using, atd. |
| **Metadata** | Atributy, XML doc, using direktivy, namespace |
| **Validace** | Invarianty, CoreValidator, DiagnosticBag |
| **Value Objects** | StrongType, ValueObjectValidationRule |
| **ForgeBlocky** | Znovupoužitelné balíky elementů |

### Co Core NEREPREZENTUJE

- ❌ Raw kód jako string (kromě `MethodElement.Body` string fallbacku)
- ❌ Formátování kódu (odsazení, mezery)
- ❌ C#-specifické syntaktické detaily
- ❌ Projektové soubory (.csproj, .sln)
- ❌ NuGet package reference (patří do ProjectElement v Infrastructure)
- ❌ Build proces, kompilace
- ❌ Pricing/licensing flagy

## Architektonické invarianty

1. **BusinessAuthoringDocument je source of truth** — Core model se z něj odvozuje.
2. **CommandLog je append-only** — Core model se z něj přehrává.
3. **Core nenese business logiku** — je to čistě datový model.
4. **AI je volitelná** — Core funguje bez AI.
5. **Jeden zdroj pravdy pro invarianty** — `InvariantDefinition`, ne duplicitní validace.

## Navazující dokumenty

| Dokument | Obsah |
|----------|-------|
| [00-Support-Matrix.md](00-Support-Matrix.md) | Kompletní matice podpory C# konstrukcí |
| [01-Type-System.md](01-Type-System.md) | Typový systém: primitiva, nullable, kolekce, generika |
| [02-Type-Kinds.md](02-Type-Kinds.md) | Typové druhy: Class, Struct, Interface, Enum, Record |
| [03-Value-Objects.md](03-Value-Objects.md) | StrongType, ValueObject, validace, katalog |
| [04-Methods.md](04-Methods.md) | Metody: signatura, parametry, async, modifikátory |
| [05-Expressions-and-AST.md](05-Expressions-and-AST.md) | Expression a Statement model, hranice AST |
| [06-Roundtrip-Boundary.md](06-Roundtrip-Boundary.md) | C# → Core → C#: co projde beze ztráty |
| [07-Examples.md](07-Examples.md) | Reálné příklady: C# kód → Core reprezentace |
