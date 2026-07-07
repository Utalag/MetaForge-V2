# PROP-035 C#-First Core Migration

Typ výsledku: Candidate Proposal
Zdroj podnětu: Koumák — Perplexity konverzace 05663298 (5 dotazů o C#-first paradigmatu)
Stav životního cyklu: Candidate
Rozhodovací owner:
Poslední revize: 2026-07-07

Priorita: High
Oblast: Core, Translator, Generators, Tests, Docs
Owner:
Datum vytvoření: 2026-07-07
Aktualizováno: 2026-07-07

Navazuje na:
- Perplexity konverzace: https://www.perplexity.ai/search/05663298-ed0e-4ab2-bfb9-8472bbbe455a
- PROP-031 (Core Statement System) — rozšířit o Switch, ForEach, TryCatch, Using, LocalFunction
- PROP-025 (Monetizace) — C#-first kompatibilní
- GitHub task "Implement the plan" (8 kroků) — pokrývá kroky 1, 2, 3, 8

Blokuje:
- —

Související soubory:
- `Src/MetaForge.Core/Elements/`
- `Src/MetaForge.Core/Elements/Types/ClassElement.cs`
- `Src/MetaForge.Core/Elements/Members/MethodElement.cs`
- `Src/MetaForge.Translator/`
- `Tests/MetaForge.Core.Tests/`

## 1. Kontext

MetaForge Core byl navržen jako language-agnostic — abstraktní typový model, elementy přes `RootElement`, exprese přes abstraktní `Expression`. V praxi je ale 95% použití C#. Aktuální kód už je fakticky napůl C#-first — `ClassElement` má `IsRecord`, `IsPartial`, `IsSealed`; `MethodElement` má `IsAsync`, `IsAbstract`, `IsOverride`. Další setrvávání v "agnostic" narativu jen prodlužuje architektonický dluh.

## 2. Problém dnes

- **Falešná neutralita:** Elementy předstírají language-agnostic, ale reálně nesou C# sémantiku.
- **Chybějící C# koncepty:** `Namespace`, `PrimaryConstructorParameters`, `ExpressionBody`, `TypeParameters` — bez nich generátor nemůže produkovat validní C#.
- **Komplexita Translatoru:** Translator dělá heuristické mapování, protože Core nedává jasné C# sémantické signály.
- **AI vrstva trpí:** Prompt engineering je složitější, když interní model neodpovídá výstupu 1:1.

## 3. Cíl

- Explicitně deklarovat MetaForge.Core jako **C# semantic model** (ne C# syntaktický).
- Doplnit chybějící C# first-class koncepty do elementů.
- Zredukovat Translator na tenčí mapovací vrstvu.
- Připravit půdu pro jednodušší AI prompting a generování.

## 4. Architektonické invarianty

- BusinessAuthoringDocument zůstává source of truth.
- CommandLog zůstává append-only.
- Core = C# sémantický model (ne syntaktický — neobsahuje `string CSharpCode`).
- Translator = projekční vrstva pro ne-C# výstupy.
- AI je volitelná vrstva, ne podmínka základní funkčnosti.
- **Red lines:** Core nikdy neobsahuje raw kód, formatting, prompt texty, pricing/licensing flagy, Roslyn závislosti, `Dictionary<string,object>` feature bagy.

## 5. Scope

### In scope
- 7 commitů migrace: RootElement → ClassElement+InterfaceElement+StructElement → MethodElement → Expressions → Translator → Tests → Docs
- Aditivní změny (přidání, ne mazání) — postupná evoluce, ne revoluce.
- C#-first sémantické vlastnosti: Namespace, PrimaryConstructor, TypeParameters (na všech type elementech), GenericConstraint model, ExpressionBody, IsExtension.
- Nové expression typy: LambdaExpression, NewExpression, DefaultExpression, ConversionExpression, AwaitExpression, SwitchExpression, IsPatternExpression, NullCoalescingExpression.
- NamedArgument podpora v MethodCallExpression.
- PROP-031 rozšířit o SwitchStatement, ForEachStatement, TryCatchStatement, UsingStatement/UsingDeclarationStatement, LocalFunctionStatement.
- Zredukovat Translator mapování.
- Nové validační testy na invarianty elementů.

### Out of scope
- Mazání existujících API — `BaseClassName` jako `string?` zůstává jako compatibility.
- Přejmenovávání na `ICSharp*` — není potřeba.
- Masivní rewrite Translatoru — jen zredukovat.
- Automatická migrace existujícího kódu.

## 6. Návrh řešení

### Migrační strategie: 7 commitů

| # | Commit | Pracnost | API změna | Maže se |
|---|--------|----------|-----------|---------|
| 1 | RootElement | 2-4h | `+Namespace`, `+XmlSummary`, `+WithNamespace()`, `+WithXmlSummary()` | Nic |
| 2 | ClassElement + InterfaceElement + StructElement | 6-10h | `+TypeParameters`, `+TypeConstraints` na Class/Interface/Struct; `GenericConstraint` model (where T : class, new(), IComparable\<T\>); `+PrimaryConstructorParameters` (Class/Struct); `+PrimaryRecord()`, `+Generic()` factories | Nic |
| 3 | MethodElement | 4-8h | `+ExpressionBody`, `+IsExtension`, `+TypeParameters`, `+TypeConstraints`, `+Generic()` factory | Nic |
| 4 | Expressions | 8-14h | `+LambdaExpression`, `+NewExpression`, `+DefaultExpression`, `+ConversionExpression`, `+AwaitExpression`, `+SwitchExpression`, `+IsPatternExpression`, `+NullCoalescingExpression`, `+NamedArgument` (název parametru vedle pozičního), `ExpressionKind` rozšíření | Nic |
| 5 | Translator | 4-8h | Zredukovat mapování, `LanguageCapabilityProfile` s exact/lossy/unsupported | Zastaralé heuristiky |
| 6 | Tests | 4-8h | Nové invariant testy (IsAsync+IsAbstract invalid atd.), golden tests, snapshot testy | Nic |
| 7 | Docs | 2-4h | Architecture Decision Record, migration guide, aktualizace New_Architecture/ | Nic |

**Celkem: 34-60h (4,5-8 dní)**

### Princip: C#-first sémantický, ne syntaktický

```
❌ Syntaktický: Code = "public record Foo { }" jako string
✅ Sémantický:  new ClassElement { IsRecord = true, Accessibility = Public }
⚠️ Agnostic:   new TypeElement { Features = ["immutable", "value-based"] }
```

### C# sémantika jako first-class citizens
- `record`, `init`, `required`, `partial`, `async/await`, `expression-bodied members`
- ne jako string tagy v `Dictionary<string, object>`

### Core = sémantika, Translator = projekce
- Core rozhoduje o **sémantice** (co to je).
- Translator rozhoduje o **projekci** (jak to vypadá jinde).
- Generator/Renderer rozhoduje o **finálním textu**.

## 7. Implementační dopad

### Změněné projekty nebo soubory

- `Src/MetaForge.Core/Elements/RootElement.cs` — `+Namespace`, `+XmlSummary`
- `Src/MetaForge.Core/Elements/Types/ClassElement.cs` — `+PrimaryConstructorParameters`, `+TypeParameters`, `+TypeConstraints`
- `Src/MetaForge.Core/Elements/Types/InterfaceElement.cs` — `+TypeParameters`, `+TypeConstraints`
- `Src/MetaForge.Core/Elements/Types/StructElement.cs` — `+TypeParameters`, `+TypeConstraints`, `+PrimaryConstructorParameters`
- `Src/MetaForge.Core/Elements/Types/GenericConstraint.cs` — nový: where T : class, struct, new(), BaseType, interface
- `Src/MetaForge.Core/Elements/Members/MethodElement.cs` — `+ExpressionBody`, `+IsExtension`, `+TypeParameters`, `+TypeConstraints`
- `Src/MetaForge.Core/Elements/Expressions/` — 8 nových expression typů + NamedArgument
- `Src/MetaForge.Core/Elements/Statements/` — 6 nových statement typů (Switch, ForEach, TryCatch, Using, UsingDecl, LocalFunc)
- `Src/MetaForge.Translator/` — redukce mapování
- `Tests/MetaForge.Core.Tests/` — validační invariant testy
- `Docs/` — ADR + migration guide

### API a kontrakty

- Všechny změny jsou **aditivní** (přidávají se nové property/factory).
- Nic se nemaže v Commitech 1-4.
- `BaseClassName` zůstává jako `string?` (compatibility).
- Breaking change až v budoucím release (Commit 5+).

### Testy

- Validační invariant testy: `IsAsync && IsAbstract => invalid`, `abstract && Body != null => invalid` atd.
- Golden tests: 10-20 reprezentativních scénářů (basic class, record s primary constructorem, async method, generic method).
- Snapshot testy na přesný výstup.
- Round-trip testy: C# snippet → model → C# snippet (sémanticky ekvivalentní).

### Dokumentace

- Architecture Decision Record: "MetaForge.Core je C# semantic model"
- Migration guide v `Docs/`
- Aktualizace `New_Architecture/04-Core-Elements.md`

## 8. Implementační fáze

### Fáze 1: RootElement + ClassElement (Commit 1-2)
- Namespace, XmlSummary do RootElement
- PrimaryConstructor, TypeParameters do ClassElement
- Nové factory metody
- Existující testy musí projít beze změny
- **DoD:** Build OK, 3+ nové unit testy, fluent API funguje

### Fáze 2: MethodElement + Expressions (Commit 3-4)
- ExpressionBody, IsExtension do MethodElement
- Await, Switch, IsPattern, NullCoalescing expressiony
- **DoD:** Lze vytvořit async expression-bodied generic metodu

### Fáze 3: Translator + Tests (Commit 5-6)
- Zredukovat Translator mapování
- LanguageCapabilityProfile
- Validační invariant testy, golden tests
- **DoD:** 90%+ existujících testů projde bez změny, přibyde 20+ nových testů

### Fáze 4: Docs (Commit 7)
- ADR, migration guide, New_Architecture aktualizace
- **DoD:** Všechny dokumenty existují a cross-odkazují správně

## 9. Otevřené otázky

- OQ-035-01: Kdy udělat breaking release (Commit 5+), kde se smažou compatibility shimy?
- OQ-035-02: Má se `BaseClassName` převést z `string?` na `TypeModel?`? (Perplexity doporučuje až později)
- OQ-035-03: Jak přesně má vypadat `LanguageCapabilityProfile` pro Translator?

## 10. Rizika a trade-offy

- Riziko regrese: Změny jsou aditivní — minimalizované.
- Riziko false economy: C#-first může komplikovat budoucí TypeScript/Python generátory. Řešení: `LanguageCapabilityProfile` v Translatoru.
- Riziko semantic leakage: C#-first ≠ C# syntaktický. Core stále modeluje sémantiku, ne text.
- Trade-off: Krátkodobě víc kódu, dlouhodobě míň komplexity v Translatoru a AI vrstvě.

## 11. Validace

- Build: Projde bez změny existujících testů v Commitech 1-4.
- Testy: Validační invarianty, golden tests, snapshoty.
- Smoke scénáře: Vytvoření record s primary constructorem, async generic metody, expression-bodied memberu.
- Ruční kontrola: Code review proti "red lines".

## 12. Výsledek po dokončení

Vyplnit až při uzavření návrhu.
